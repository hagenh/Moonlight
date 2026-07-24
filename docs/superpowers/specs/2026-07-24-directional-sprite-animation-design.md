# Directional Sprite Animation System

## Problem

Guards need animated idle and walking sprites in each cardinal direction. The project has no animation infrastructure — every entity uses a static procedural placeholder sprite. The player has `AnimatorParams` defined but no Animator Controller assets exist, and the project conventions avoid ScriptableObjects and frameworks.

## Decision

Build a reusable, code-driven sprite-swap animation system. No Unity Animator Controller or `.anim`/`.controller` assets required. Uses `SpriteRenderer.sprite` swapping on a timer.

## Data Structures

### DirectionalClip

A per-direction sprite array container:

```csharp
public class DirectionalClip
{
    public Sprite[] down;
    public Sprite[] up;
    public Sprite[] left;
    public Sprite[] right;
    public float framesPerSecond = 8f;
    public bool loop = true;
}
```

- Each direction holds 1+ sprite frames
- `framesPerSecond` controls playback speed per clip
- `loop` determines whether the clip loops or returns to the default clip on completion

### DirectionalAnimationSet

Named clip collection:

```csharp
public class DirectionalAnimationSet
{
    public Dictionary<string, DirectionalClip> clips;
    public string defaultClip = "idle";

    public DirectionalClip GetClip(string name);
    public void AddClip(string name, DirectionalClip clip);
}
```

- Clips keyed by name ("idle", "walk", etc.)
- `defaultClip` is played on start and when a non-looping clip finishes

## Component: DirectionalSpriteAnimator

A MonoBehaviour that drives frame swapping on a `SpriteRenderer`.

### Public API

| Method | Description |
|--------|-------------|
| `Play(string clipName)` | Switch to the named clip, reset to frame 0 |
| `SetFacing(FacingDirection facing)` | Update facing direction, swap to correct sprite array |
| `SetFacingFromVector(Vector2 movement)` | Convert movement vector to FacingDirection via FacingMath, update if changed |
| `Stop()` | Stop cycling, freeze on current frame |

### Behavior

- On `Awake()`: requires `SpriteRenderer` on same GameObject, plays default clip
- Frame cycling: track `_currentFrame` and `_frameTimer`, advance when timer exceeds `1/framesPerSecond`
- When facing changes mid-clip: keep frame index, swap to new direction's sprite array
- When a non-looping clip finishes: auto-switch to `defaultClip`
- `SetFacingFromVector` only updates facing when the vector magnitude exceeds a deadzone (matches player convention)

## Guard Integration

### Guard.cs Changes

- Add `[SerializeField] private DirectionalSpriteAnimator animator` (or get via `GetComponent` in Awake)
- In `UpdatePatrol()`:
  - When moving toward waypoint: `animator.SetFacingFromVector(dir)` then `animator.Play("walk")`
  - When pausing at waypoint: `animator.Play("idle")` (keep current facing)
- In `ResetToStart()`: `animator.Play("idle")`
- The vision cone continues using the continuous `_baseFacing` angle — no change to detection/cone logic

### GuardManager.cs Changes

- In `SpawnGuard()`:
  - Add `DirectionalSpriteAnimator` component
  - Assign the guard's `DirectionalAnimationSet` from ContentDb
  - Remove static placeholder sprite logic (the animator drives the SpriteRenderer)
  - Keep the `SpriteRenderer` component but don't assign a static sprite
- Remove `GetGuardSprite()` and the `_guardSprite` static field

### ContentDb.cs Changes

- Add `public static readonly DirectionalAnimationSet GuardAnimations` field
- Initialize in `Awake()` with placeholder frames (procedural tinted sprites) per direction per clip
- Placeholder: same 4x4 white pixel sprite tinted guard-blue, one frame per direction for idle, 3 frames per direction for walk (slightly offset positions to suggest movement)

## File Placement

| Type | Path |
|------|------|
| Data struct | `Assets/Scripts/DirectionalClip.cs` |
| Data collection | `Assets/Scripts/DirectionalAnimationSet.cs` |
| Component | `Assets/Scripts/DirectionalSpriteAnimator.cs` |
| Guard (modified) | `Assets/Scripts/Guard.cs` |
| GuardManager (modified) | `Assets/Scripts/GuardManager.cs` |
| ContentDb (modified) | `Assets/Scripts/ContentDb.cs` |

## Testing

- EditMode tests for `DirectionalSpriteAnimator`: frame cycling, clip switching, facing changes, loop/non-loop behavior
- EditMode tests for `DirectionalAnimationSet`: clip lookup, add/retrieve
- No PlayMode tests needed — purely visual logic with no cross-system integration

## Constraints

- No Unity Animator Controller or `.anim`/`.controller` assets
- No ScriptableObjects — all data is hardcoded or code-constructed
- Reuses existing `FacingDirection` enum and `FacingMath`
- Component is reusable by any entity (guards, residents, etc.)
- Vision cone facing (continuous angle) is separate from sprite facing (cardinal direction)
