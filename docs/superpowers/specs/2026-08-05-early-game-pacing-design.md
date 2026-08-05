# Early-Game Pacing: Day-1 Orders & Homestead Construction Rework — Design

**Touches:** `StandManager.cs`, `Homestead.cs`, `StonePile.cs`, `FallenLog.cs`, `Player/States/HammerState.cs`, `ContentDb.cs`, `InventoryManager.cs`, `GameHUD.cs`, `SampleScene.unity`. Does not supersede any existing spec.

## Goal

The player has little to do in the opening stretch of the game. Two changes address this from different angles:

1. The request book has live orders from the moment the game starts, instead of staying empty until the player's first sleep.
2. Homestead construction (Foundation, Frame) becomes a bigger, visible, incremental foraging-and-building loop instead of an instant lump-sum interact — more to actually do early on, with the extra cost paid for by a slower, more deliberate foraging loop rather than by blowing past the existing pacing target.

**Explicitly out of scope:**
- Walls (2 Wood + 3 Nails) — unchanged, stays lump-sum. Nails have exactly one source in the game today (Tormod's one-time 3-Nail gift via `TormodInteractable`); there is no repeatable Nails income until the Smithy (Phase F, not built), so Walls isn't touched by this pass.
- How Nails are obtained/produced — untouched.
- New art. Everything reuses existing assets: `StonePile`/`FallenLog`'s existing Grasslands-tileset sprites, `pick_t.png`, `sl_axe_t.png`, and the existing `HammerState` hold-to-progress pattern.
- `war_t.png` — turned out to be crossed swords, not a tool; reserved for a future sword item, untouched here.
- Save/load — nothing here is persisted, matching the rest of the project (plain serializable fields, extraction-ready per `BuildPlan.md`'s Rules section).
- `BuildPlan.md`'s Phase 1 "20-40 min to homestead shell" gate is **not** being superseded — the new costs are sized so that gate is still intended to hold (see Numbers below).

## Architecture

Three independent pieces:

1. Day-1 request seeding (`StandManager`)
2. Foraging rework: tools + multi-swing harvest (`StonePile`, `FallenLog`, generalized `HammerState`)
3. Homestead construction rework: incremental deposit + in-world grid-fill visual (`Homestead`)

### 1. Day-1 request seeding

`StandManager.Awake()` seeds the book immediately, reusing the exact same posting path `OnDayEnded` already calls (`RequestArrivalRules.Generate` × `RequestArrivalRules.NotesPerNight(startingSlots)`):

```csharp
private void Awake()
{
    ...
    _book = new RequestBook(startingSlots);
    PostNightNotes(0);
}
```

`PostNightNotes` already guards on `_book.FreeSlots` and `AvailableRecipes()`, so calling it once at boot is safe with no other changes. Every day after this still posts new notes via `OnDayEnded` exactly as today — this only adds one extra call at startup, it doesn't change the nightly cadence.

### 2. Foraging rework: tools + multi-swing harvest

**New items** (`ContentDb.cs`), granted once in `InventoryManager.Awake()` alongside the existing starting 3 Berry:

- `Pickaxe` — `isIngredient: false`, icon `pick_t.png`. Required to harvest `StonePile`.
- `HandAxe` — `isIngredient: false`, icon `sl_axe_t.png`. Required to harvest `FallenLog`.

Both are real inventory items (occupy a slot), granted once at game start, never consumed or removed. `StonePile`/`FallenLog` check `InventoryManager.Instance.Has(tool, 1)` before allowing the harvest interaction to begin — in practice the player always has it (day-1 grant, nothing removes it), but the check keeps the interface honest rather than assuming.

**Multi-swing harvest**: `StonePile.Interact()` and `FallenLog.Interact()` — both currently an instant single-tap yield — become a hold-to-progress interaction, generalizing the existing repair-building pattern (`Player/States/HammerState.cs`, `Building.HammerDuration`, `BuildingManager.TryHammerHit`):

- Extract a small interface both `Building` and the two forageables implement (e.g. `IHammerable { float SwingDuration { get; } void OnSwingCompleted(); }`), and generalize `HammerState` to work against that interface instead of taking a `Building` constructor param directly.
- `StonePile`/`FallenLog` each require **3 swings at 3 seconds hold each** (~9s total) before yielding their resource. Yield stays exactly as today (1 Stone / 1 Wood per completed harvest), and the existing daily respawn (`OnDayEnded` resets `_harvested`) is unchanged.
- Reuses the existing `GameEvents.HammerProgress`-style event → `GameHUD`'s existing progress-text pattern (`hammerProgressText`), now also firing for forage targets. No new UI element.

### 3. Homestead construction rework

**Cost changes** (`Homestead.cs`):

| Stage | Current cost | New cost |
|---|---|---|
| Site → Foundation | 3 Stone | 20 Stone |
| Foundation → Frame | 3 Wood | 12 Wood |
| Frame → Walls | 2 Wood + 3 Nails | unchanged |

**Incremental deposit**: `Homestead.Interact()` at the `Site` and `Foundation` stages no longer requires the full cost up front in one lump. Each interact deposits however much of the relevant material the player is currently carrying, up to whatever's still needed, tracked as running counters (`_stoneDeposited`, `_woodDeposited`). When a counter reaches the stage's cost, the stage auto-advances exactly as today (toast, `AdvanceStage`, `GameEvents.OnHomesteadBuildStageChanged`).

```csharp
case BuildStage.Site:
    int stoneNeeded = FoundationCost - _stoneDeposited;
    int carried = InventoryManager.Instance.CountOf(ContentDb.Stone);
    int toDeposit = Mathf.Min(stoneNeeded, carried);
    if (toDeposit <= 0) { GameEvents.OnToastRequested("Need Stone to keep building"); return; }
    InventoryManager.Instance.TryRemove(ContentDb.Stone, toDeposit);
    _stoneDeposited += toDeposit;
    UpdateFoundationFill(_stoneDeposited);
    if (_stoneDeposited >= FoundationCost) AdvanceStage(BuildStage.Foundation, "Foundation built!");
    break;
```

(Exact member names are illustrative — the implementation plan owns the final shape. `InventoryManager` may already expose an equivalent count query; if not, adding one is part of this work.)

Walls keeps its current all-or-nothing `Interact()` branch untouched.

**Grid-fill visual**: `HomesteadPreBuild.png` — the fenced construction-site sprite already shown throughout Site/Foundation/Frame/Walls today (currently just re-tinted per stage via `_stageColors`) — gets a grid of small sprite instances tiling into its fenced interior as material is deposited:

- **Foundation**: one stone-pile sprite (the same sprite `StonePile` already renders, sliced from `Grassland Spring@128x128.png`) revealed per Stone deposited. 20 cells total; a full grid means Foundation is complete.
- **Frame**: the same mechanic, same footprint, swapped to `FallenLog`'s wood sprite, 12 cells, filling as Wood is deposited.
- The grid is sized/arranged to roughly match the fence's interior area — exact rows/columns/cell size is an implementation-plan detail, not fixed here. Cells reveal in a fixed or lightly randomized order for an organic look, not strictly left-to-right.
- The existing per-stage color tint (`_stageColors`) is dropped in favor of this fill, or kept as a subtle base tint underneath it — implementer's call, doesn't change the mechanic either way.

**Progress readout**: `GameHUD`'s existing Homestead interact-prompt line (`[E] Homestead Site (need 3 Stone)`) is extended to show the running count alongside the visual fill, e.g. `[E] Deposit Stone (12/20)`.

## Numbers (tunable — flagged for playtest, matching this project's existing convention for invented numbers)

| Item | Current | New |
|---|---|---|
| Foundation cost | 3 Stone | 20 Stone |
| Frame cost | 3 Wood | 12 Wood |
| Walls cost | 2 Wood + 3 Nails | unchanged |
| Day-1 orders | 0 (wait for first night) | ~2 (`NotesPerNight(3)`), seeded at game start |
| Swings per harvest | 1 (instant) | 3, at 3s hold each (~9s total) |
| StonePile count (scene) | 6 | 14 |
| FallenLog count (scene) | 8 | 12 |

Pacing intent: node counts are sized so the full Foundation (20 Stone) and Frame+Walls (14 Wood) needs are each gatherable in one foraging sweep without waiting on the daily respawn — `BuildPlan.md` Phase 1's "20-40 min to homestead shell" gate is intended to still hold with these numbers, not be superseded by them.

## Testing

- `StandManager`: EditMode/PlayMode test confirming `Book.Active.Count > 0` immediately after `Awake()`, before any `OnDayEnded` call.
- `Homestead`: tests for a partial deposit (less than needed — counter increments, stage doesn't advance), an exact-completion deposit (advances), an over-carrying deposit (only consumes up to the remaining need, leftover stays in inventory), and confirmation Walls' lump-sum behavior is unchanged.
- `StonePile`/`FallenLog`: PlayMode tests for swing progress (partial holds don't yield), full completion (yields after 3 swings), daily respawn unchanged.
- Tool gating: PlayMode test confirming `Pickaxe`/`HandAxe` exist in inventory immediately at game start.
- Manual Play Mode check via Unity MCP: forage several Stone/Wood via the new hold interaction, deposit at the Homestead, confirm the grid-fill visual updates and the interact-prompt count is accurate, confirm Frame's fill swaps to the wood sprite, confirm Walls is still a single lump-sum interact.
- All existing EditMode/PlayMode tests stay green.

## Follow-up work (not this spec)

- Exact grid layout (rows/columns, cell size, reveal order) — implementation-plan detail.
- Extending this incremental-deposit + grid-fill pattern to Phase U's homestead site projects, if it proves out well here.
- Walls' Nail cost, once the Smithy (Phase F) gives a repeatable Nails source.
- Updating `BuildPlan.md` Phase 1/Phase S prose to reflect the new numbers once this ships.
