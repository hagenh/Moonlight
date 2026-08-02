# Footstep SFX Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Play surface-appropriate footstep sounds when the player walks or sprints, using FootstepTile metadata to determine which clip pool to sample from.

**Architecture:** A `FootstepTile : TileBase` carries a `FootstepSurface` enum field. A `FootstepPlayer : MonoBehaviour` on the player samples the tilemap each frame, reads the surface type, and plays random clips from the matching pool at a walk/sprint cadence.

**Tech Stack:** Unity 6, C#, UnityEngine.Tilemaps, AudioSource.PlayOneShot

## Global Constraints

- No comments in code unless explicitly requested
- No ScriptableObjects for game data
- No dependency injection — use singletons and events
- UI is IMGUI (not relevant here but stating for completeness)
- Rules/ layer must be pure C# — FootstepTile and FootstepPlayer are Unity-dependent so they live in `Assets/Scripts/`
- Test naming: `Method_Condition_Expected`

---

### Task 1: FootstepTile

**Files:**
- Create: `Assets/Scripts/FootstepTile.cs`
- Test: `Assets/Tests/EditMode/FootstepTileTests.cs`

**Interfaces:**
- Produces: `FootstepSurface` enum (values: `Dirt`, `Sand`, `Stone`, `Water`), `FootstepTile` class with `Surface` property of type `FootstepSurface`

- [ ] **Step 1: Write the FootstepTile class**

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

public enum FootstepSurface
{
    Dirt,
    Sand,
    Stone,
    Water
}

[CreateAssetMenu(fileName = "FootstepTile", menuName = "Tiles/FootstepTile")]
public class FootstepTile : TileBase
{
    public Sprite sprite;
    public FootstepSurface Surface = FootstepSurface.Dirt;
    public Tile.ColliderType colliderType = Tile.ColliderType.Sprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = colliderType;
    }
}
```

- [ ] **Step 2: Write the test**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FootstepTileTests
{
    [Test]
    public void Surface_DefaultsToDirt()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        Assert.AreEqual(FootstepSurface.Dirt, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToSand()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Sand;
        Assert.AreEqual(FootstepSurface.Sand, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToStone()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Stone;
        Assert.AreEqual(FootstepSurface.Stone, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToWater()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Water;
        Assert.AreEqual(FootstepSurface.Water, tile.Surface);
    }

    [Test]
    public void GetTileData_SetsSprite()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        var tex = new Texture2D(1, 1);
        tile.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        var tileData = new TileData();
        tile.GetTileData(Vector3Int.zero, null, ref tileData);
        Assert.IsNotNull(tileData.sprite);
        Assert.AreEqual(tile.sprite, tileData.sprite);
    }

    [TearDown]
    public void TearDown()
    {
        var tiles = Resources.FindObjectsOfTypeAll<FootstepTile>();
        foreach (var t in tiles)
            Object.DestroyImmediate(t);
    }
}
```

- [ ] **Step 3: Run EditMode tests to verify they pass**

Run via Unity MCP or command line. Expected: all 5 tests PASS.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/FootstepTile.cs Assets/Tests/EditMode/FootstepTileTests.cs
git commit -m "feat: add FootstepTile with surface enum"
```

---

### Task 2: FootstepPlayer

**Files:**
- Create: `Assets/Scripts/FootstepPlayer.cs`
- Test: `Assets/Tests/EditMode/FootstepPlayerTests.cs`

**Interfaces:**
- Consumes: `FootstepSurface` enum from Task 1, `FootstepTile` from Task 1, `PlayerController.IsSprintHeld` (bool property), `PlayerController.RB` (Rigidbody2D property), `PlayerController.MoveInput` (Vector2 property), `PlayerController.moveDeadzone` (float, serialized private — FootstepPlayer uses its own deadzone field or reads velocity magnitude)
- Produces: `FootstepPlayer` MonoBehaviour with `GetClipsForSurface(FootstepSurface)` method and `CurrentSurface` property

- [ ] **Step 1: Write the FootstepPlayer class**

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] dirtClips;
    [SerializeField] private AudioClip[] sandClips;
    [SerializeField] private AudioClip[] stoneClips;
    [SerializeField] private AudioClip[] waterClips;
    [SerializeField] private float walkCadence = 0.4f;
    [SerializeField] private float sprintCadence = 0.25f;
    [SerializeField] private FootstepSurface defaultSurface = FootstepSurface.Dirt;
    [SerializeField] private float moveThreshold = 0.1f;

    private FootstepSurface currentSurface;
    private float stepTimer;
    private Rigidbody2D rb;

    public FootstepSurface CurrentSurface => currentSurface;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stepTimer = walkCadence;
    }

    private void Update()
    {
        UpdateCurrentSurface();

        if (IsMoving())
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = IsSprinting() ? sprintCadence : walkCadence;
            }
        }
        else
        {
            stepTimer = walkCadence;
        }
    }

    private void UpdateCurrentSurface()
    {
        if (groundTilemap == null) return;

        Vector3Int cellPos = groundTilemap.WorldToCell(transform.position);
        var tile = groundTilemap.GetTile<FootstepTile>(cellPos);

        if (tile != null)
            currentSurface = tile.Surface;
        else
            currentSurface = defaultSurface;
    }

    private bool IsMoving()
    {
        if (rb == null) return false;
        return rb.linearVelocity.magnitude > moveThreshold;
    }

    private bool IsSprinting()
    {
        if (PlayerController.Instance == null) return false;
        return PlayerController.Instance.IsSprintHeld;
    }

    public AudioClip[] GetClipsForSurface(FootstepSurface surface)
    {
        return surface switch
        {
            FootstepSurface.Sand => sandClips,
            FootstepSurface.Stone => stoneClips,
            FootstepSurface.Water => waterClips,
            _ => dirtClips
        };
    }

    private void PlayStep()
    {
        var clips = GetClipsForSurface(currentSurface);
        if (clips == null || clips.Length == 0) return;
        if (audioSource == null) return;

        var clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
```

- [ ] **Step 2: Write the test**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using Lamplight.TestSupport;

public class FootstepPlayerTests
{
    private GameObject playerGo;
    private FootstepPlayer footstepPlayer;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        playerGo = TestBootstrap.CreateGameObject("Player");
        playerGo.AddComponent<Rigidbody2D>();
        footstepPlayer = playerGo.AddComponent<FootstepPlayer>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void GetClipsForSurface_Dirt_ReturnsDirtClips()
    {
        var dirtClips = new AudioClip[2];
        footstepPlayer.dirtClips = dirtClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Dirt);
        Assert.AreSame(dirtClips, result);
    }

    [Test]
    public void GetClipsForSurface_Sand_ReturnsSandClips()
    {
        var sandClips = new AudioClip[2];
        footstepPlayer.sandClips = sandClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Sand);
        Assert.AreSame(sandClips, result);
    }

    [Test]
    public void GetClipsForSurface_Stone_ReturnsStoneClips()
    {
        var stoneClips = new AudioClip[2];
        footstepPlayer.stoneClips = stoneClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Stone);
        Assert.AreSame(stoneClips, result);
    }

    [Test]
    public void GetClipsForSurface_Water_ReturnsWaterClips()
    {
        var waterClips = new AudioClip[2];
        footstepPlayer.waterClips = waterClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Water);
        Assert.AreSame(waterClips, result);
    }

    [Test]
    public void CurrentSurface_DefaultsToDirt()
    {
        Assert.AreEqual(FootstepSurface.Dirt, footstepPlayer.CurrentSurface);
    }
}
```

Note: The serialized fields (`dirtClips`, `sandClips`, etc.) need to be accessible from tests. Mark them as `internal` or add `[field: SerializeField]` with internal access, or make the test assembly a friend assembly. The simplest approach: change the fields from `private` to `internal` with `[assembly: InternalsVisibleTo("Lamplight.EditModeTests")]` in `Assets/Scripts/AssemblyInfo.cs`.

- [ ] **Step 3: Add InternalsVisibleTo if not present**

Check `Assets/Scripts/AssemblyInfo.cs` for:
```csharp
[assembly: InternalsVisibleTo("Lamplight.EditModeTests")]
```

If missing, add it. Then change the `[SerializeField] private` clip fields on `FootstepPlayer` to `[SerializeField] internal`.

- [ ] **Step 4: Run EditMode tests**

Expected: all 5 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/FootstepPlayer.cs Assets/Tests/EditMode/FootstepPlayerTests.cs
git commit -m "feat: add FootstepPlayer with surface-based clip selection"
```

---

### Task 3: Editor setup — FootstepTile assets and Player prefab

**Files:**
- Modify: Player prefab (add FootstepPlayer + AudioSource components, assign references)
- Create: FootstepTile assets in the tile palette (editor-only, no script changes)

**Interfaces:**
- Consumes: `FootstepTile` from Task 1, `FootstepPlayer` from Task 2

This task is manual editor work. No code steps.

- [ ] **Step 1: Create FootstepTile assets in the tile palette**

For each ground tile used in the scene:
1. Right-click in the Project window → Create → Tiles → FootstepTile
2. Name it to match the original tile (e.g., `GrassTile_Footstep`, `SandTile_Footstep`)
3. Set the `Sprite` field to the original tile's sprite
4. Set the `Surface` field: grass/dirt tiles → `Dirt`, sand tiles → `Sand`, stone tiles → `Stone`, water tiles → `Water`
5. Set `Collider Type` to match the original

- [ ] **Step 2: Repaint the ground tilemap**

In the Scene view, use the Tile Palette to replace existing tiles with their `FootstepTile` equivalents. Use the paint tool to swap tiles in-place.

- [ ] **Step 3: Add FootstepPlayer to the Player prefab**

1. Open the Player prefab
2. Add Component → `FootstepPlayer`
3. Add Component → `AudioSource` (if not present)
4. On the `FootstepPlayer` component:
   - Drag the ground Tilemap from the scene hierarchy into `Ground Tilemap`
   - Drag the `AudioSource` into `Audio Source`
   - Set `Walk Cadence` = 0.4
   - Set `Sprint Cadence` = 0.25
   - Set `Default Surface` = Dirt

- [ ] **Step 4: Assign step clips to FootstepPlayer**

On the `FootstepPlayer` component in the Inspector:
- `Dirt Clips` (size 6): drag in `Step 1.mp3` through `Step 6.mp3`
- `Sand Clips` (size 4): drag in `Step (sand) 1.mp3` through `Step (sand) 4.mp3`
- `Stone Clips` (size 4): drag in `Step (stone) 1.mp3` through `Step (stone) 4.mp3`
- `Water Clips` (size 4): drag in `Step (water) 1.mp3` through `Step (water) 4.mp3`

- [ ] **Step 5: Playtest in the editor**

1. Enter Play mode
2. Walk on grass/dirt tiles — should hear Step 1-6 clips at ~0.4s cadence
3. Walk on sand tiles — should hear Step (sand) 1-4 clips
4. Sprint (hold sprint button) — same clips, faster cadence (~0.25s)
5. Stop moving — footsteps stop immediately
6. Carry debris and walk — footsteps still play at walk cadence

- [ ] **Step 6: Commit scene and prefab changes**

```bash
git add Assets/Prefabs/Player.prefab Assets/Scenes/SampleScene.unity
git commit -m "feat: wire up FootstepPlayer on Player prefab with step clips"
```

---

### Task 4: Backlog update

**Files:**
- Modify: `docs/backlog.md`

- [ ] **Step 1: Mark the footstep SFX backlog item as done**

Change:
```markdown
- [S] Footstep SFX — play footstep sounds when the player walks
```
To:
```markdown
- [x] ~~Footstep SFX — play footstep sounds when the player walks~~
```

- [ ] **Step 2: Commit**

```bash
git add docs/backlog.md
git commit -m "chore: mark footstep SFX as done in backlog"
```
