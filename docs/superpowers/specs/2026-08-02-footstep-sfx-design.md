# Footstep SFX — Design Spec

Date: 2026-08-02
Status: Approved
Resolves: `[S] Footstep SFX` in docs/backlog.md

## Goal

Play surface-appropriate footstep sounds when the player walks or sprints. Support grass and sand initially, designed to be extended with no code changes (just enum value + clips).

## Surface Types

`FootstepSurface` enum with values matching existing SFX assets:

| Value | Clip pool |
|-------|-----------|
| Dirt | Step 1-6 (`Assets/SFX/Steps/Step *.mp3`) |
| Sand | Step (sand) 1-4 (`Assets/SFX/Steps/Step (sand) *.mp3`) |
| Stone | Step (stone) 1-4 (`Assets/SFX/Steps/Step (stone) *.mp3`) |
| Water | Step (water) 1-4 (`Assets/SFX/Steps/Step (water) *.mp3`) |

New surfaces in the future = add an enum value + assign clips. No architecture changes.

## Components

### FootstepTile : TileBase

A custom tile that renders a sprite and carries a `FootstepSurface` metadata field.

Fields:
- `Sprite sprite` — rendered by the TilemapRenderer
- `FootstepSurface Surface` — which footstep clip pool to use
- `Tile.ColliderType ColliderType` — standard collision behavior

Painted in the tile palette identically to regular tiles. The surface dropdown is set per tile asset.

### FootstepPlayer : MonoBehaviour

Attached to the Player GameObject. Responsible for detecting the current surface and playing step clips at the right cadence.

Fields (serialized):
- `Tilemap groundTilemap` — the ground tilemap to sample
- `AudioSource audioSource` — single source for OneShot playback
- `AudioClip[] dirtClips` — 6 clips
- `AudioClip[] sandClips` — 4 clips
- `AudioClip[] stoneClips` — 4 clips
- `AudioClip[] waterClips` — 4 clips
- `float walkCadence` — seconds between steps while walking (default 0.4)
- `float sprintCadence` — seconds between steps while sprinting (default 0.25)
- `FootstepSurface defaultSurface` — fallback when tile is null or not a FootstepTile (default: Dirt)

Runtime state:
- `FootstepSurface currentSurface` — updated each frame from tile lookup
- `float stepTimer` — counts down, triggers clip playback when it hits zero

### Logic (per frame)

1. Convert player position to tilemap cell coordinates
2. `groundTilemap.GetTile<FootstepTile>(cellPos)` → read `Surface` if found, else `defaultSurface`
3. If `currentSurface != newSurface`, update immediately (no crossfade needed)
4. If player is moving (velocity magnitude > `moveDeadzone`, same threshold as PlayerController):
   - Decrement `stepTimer` by `Time.deltaTime`
   - When `stepTimer <= 0`:
     - Pick random clip from the current surface pool
     - `audioSource.PlayOneShot(clip)`
     - Reset timer to `walkCadence` or `sprintCadence` based on `PlayerController.IsSprintHeld`
5. If player stops moving, reset `stepTimer` to full cadence (so first step after stopping has normal timing)

### Sprint behavior

Same clips as walking, faster cadence. No pitch shift, no alternate clip pool.

### Carry state

Footsteps play identically in CarryState (same walk cadence).

## File Layout

| File | Location |
|------|----------|
| FootstepSurface enum + FootstepTile | `Assets/Scripts/FootstepTile.cs` |
| FootstepPlayer | `Assets/Scripts/FootstepPlayer.cs` |
| Step SFX | Already in `Assets/SFX/Steps/` |

## Editor Setup (one-time)

1. Create `FootstepTile` assets in the tile palette for each ground tile (grass tiles → `Dirt`, sand tiles → `Sand`, etc.)
2. Repaint the ground tilemap using the new `FootstepTile` assets
3. Add `FootstepPlayer` component to the Player prefab
4. Assign `groundTilemap` reference
5. Drag the step clips into the per-surface arrays on `FootstepPlayer`

## Extensibility

Adding a new surface (e.g., Wood):
1. Add `Wood` to `FootstepSurface` enum
2. Add `AudioClip[] woodClips` field to `FootstepPlayer`
3. Add the case to the clip-lookup method
4. Create `FootstepTile` assets with `Surface = Wood`
5. Place them in the tilemap

No architectural changes needed.
