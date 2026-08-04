# Infrastructure Grid Placement — Design

**Supersedes:** `GameDesign.md` line 368, "Placement is into predefined sockets. No free placement system." That line and line 36 ("Player-placed public infrastructure in predefined sockets") should be updated to describe free grid placement once this ships.

## Goal

Replace the planned "predefined sockets" model for Phase 5 public infrastructure (lamppost, plank sidewalk, bench, flower box, sign — `BuildPlan.md` Phase 5, `GameDesign.md:362-368`) with free placement on a world grid: any unobstructed cell in town is a legal placement spot, not just a curated list of authored sockets. Built generically enough that future placeable item types can reuse the same system.

**Explicitly out of scope:**
- Removing or moving already-placed infrastructure (placement is permanent for now)
- Rotation (all items place in a single fixed orientation)
- `BuildBook` acquisition rules — how items get unlocked and how counts increase (crafting, cost, recipe) is separate, later design work
- Persistence — no save/load system exists anywhere in the project yet (deferred to Phase 9 per `GameDesign.md`'s 2026-08-04 changelog entry). Placed infrastructure lives only as scene GameObjects for the session, consistent with every other piece of runtime state today (`BuildingManager` rebuilds from `FindObjectsByType` every scene load; nothing is serialized anywhere in `Assets/Scripts`)

## Architecture

Four independent pieces, in dependency order:

1. **Data** — `ItemDef` gains placement fields; a new `BuildBook` holds placeable entries.
2. **Grid + validity** — `PlacementGrid` wraps the scene's existing world `Grid` for cell math and tracks occupancy.
3. **Build mode** — a controller that owns the mouse-driven ghost cursor, independent of player movement.
4. **UI** — `BuildMenuUI`, a new panel for picking what to place.

### 1. Data: `ItemDef` + `BuildBook`

`ItemDef` (`Assets/Scripts/ItemDef.cs`) gains, following its existing flat-data-class pattern (no new type hierarchy):

```csharp
public bool isPlaceable;
public GameObject placedPrefab;
public int footprintWidth = 1;
public int footprintHeight = 1;
```

A new `BuildBook` class holds the set of currently-placeable entries, separate from the general 20-slot `Inventory` — this is a distinct "book" UI concept in this project already (the request book), not an inventory filter:

```csharp
public class BuildBookEntry
{
    public ItemDef item;
    public int available;
}

public class BuildBook
{
    public List<BuildBookEntry> Entries;
    public bool TryConsume(ItemDef item); // decrements available by 1, false if 0
}
```

How entries get added and how `available` increases is out of scope for this spec. For this design to be implementable ahead of that work, seed `BuildBook` with the 5 Phase 5 items at a placeholder fixed count (e.g. 5 each) so the placement flow is testable end-to-end; replacing the seed with real acquisition rules is a follow-up.

A thin marker component `PlacedInfrastructure` goes on instantiated prefabs, recording origin cell and footprint size — not used for removal (out of scope), but so `PlacementGrid`'s occupancy map has something concrete to point at, and so a future removal/persistence feature doesn't need to re-derive placement state from scratch.

```csharp
public class PlacedInfrastructure : MonoBehaviour
{
    public Vector3Int OriginCell { get; set; }
    public int FootprintWidth { get; set; }
    public int FootprintHeight { get; set; }
}
```

### 2. Grid + validity: `PlacementGrid`

Wraps the scene's existing `Grid` component (`m_CellSize: {x:1, y:1, z:0}`, already used by `FootstepPlayer` via its `surfaceMap` Tilemap child for surface-sound lookups). **Placed infrastructure are not Tilemap tiles** — they're ordinary prefab instances (GameObject + SpriteRenderer + collider) positioned with `grid.CellToWorld(cell)`. Reusing the `Grid` component only means reusing its cell-coordinate math, so placement aligns with the same 1-unit grid the rest of the world already uses — it shares nothing else with the `surfaceMap` Tilemap, which is untouched by this feature.

```csharp
public class PlacementGrid
{
    private readonly Grid _grid; // reference to the scene's existing Grid
    private readonly Dictionary<Vector3Int, PlacedInfrastructure> _occupied = new();

    public Vector3Int WorldToCell(Vector3 worldPos) => _grid.WorldToCell(worldPos);
    public Vector3 CellToWorld(Vector3Int cell) => _grid.CellToWorld(cell);

    public bool IsAreaFree(Vector3Int origin, int width, int height)
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var cell = origin + new Vector3Int(x, y, 0);
            if (_occupied.ContainsKey(cell)) return false;
            if (Physics2D.OverlapBox(CellToWorld(cell) + cellCenterOffset, cellSize, 0f) is Collider2D c && !c.isTrigger)
                return false;
        }
        return true;
    }

    public void Reserve(Vector3Int origin, int width, int height, PlacedInfrastructure instance) { /* fills _occupied */ }
}
```

No new Physics2D layer is needed. The project has no "Obstacle" layer today — solid geometry (building walls, etc.) uses plain non-trigger `Collider2D`s, while the existing interact-detection triggers (`Debris`, `Crate`, etc.) are all `isTrigger = true` on the "Interactable" layer. Filtering `!collider.isTrigger` distinguishes solid obstacles from interact triggers without any scene/layer changes.

### 3. Build mode controller

New `BuildModeController`, entered when the player picks an entry from `BuildMenuUI`:

- Player movement (WASD) is untouched — same `PlayerController` input as always.
- Each frame, `Camera.main.ScreenToWorldPoint(Mouse.current.position)` → `PlacementGrid.WorldToCell(...)` gives the target origin cell, independent of the player's position.
- A ghost preview (the item's sprite at reduced alpha, sized to its footprint) renders at the target cell, tinted green when `PlacementGrid.IsAreaFree(origin, width, height)` is true, red otherwise.
- Left-click, if the area is free: instantiate `placedPrefab` at `CellToWorld(origin)`, call `Reserve`, `BuildBook.TryConsume(item)`, exit build mode.
- Right-click or Escape: exit build mode, no `BuildBook` change, destroy the ghost preview.

### 4. UI: `BuildMenuUI`

New panel, separate from `InventoryUI`, listing `BuildBook.Entries` where `available > 0`. Selecting an entry closes the menu and calls `BuildModeController.Enter(entry.item)`. Follows the existing UI patterns in `Assets/Scripts/UI/` (e.g. `InventoryUI`'s self-contained input handling) rather than routing through `PlayerController`.

## Testing

- `PlacementGrid.IsAreaFree` / `Reserve`: EditMode tests covering a free cell, an occupied cell (already reserved), an obstacle-overlapping cell (mock/non-trigger collider), and a multi-cell footprint straddling one occupied cell.
- `BuildBook.TryConsume`: decrements on success, no-op returning false at 0.
- Manual Play Mode check: open build menu, select an item, confirm the ghost preview tracks the mouse independently of WASD movement, confirm valid/invalid tinting, confirm left-click placement and right-click/Escape cancel both behave correctly near obstacles and near previously-placed items.
- All existing EditMode/PlayMode tests must stay green.

## Follow-up work (not this spec)

- `BuildBook` acquisition: how the 5 Phase 5 items get unlocked/crafted and how `available` counts increase.
- Removal/moving placed infrastructure.
- Updating `GameDesign.md`/`BuildPlan.md` prose to describe grid placement instead of sockets once this ships.
