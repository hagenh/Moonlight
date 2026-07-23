# Homestead Build-from-Scratch Design

> Amendment to Phase 1 (Act 0). Replaces the shipped "Homestead purchase at town edge" with a build-from-scratch system on the player's own camp clearing.

## Problem

The Homestead is currently a pre-existing Abandoned building the player buys for 80g at the town edge. This means:
- The building is visible from the start, spoiling the "your own land" reveal
- Gold-gating means the player just sells moonshine and clicks Buy — no active engagement during ferment waits
- The purchase→smash→clear→repair pipeline doesn't fit the narrative of building something from nothing on your own plot

## New Flow

**Forage materials while fermenting → Build in 3 stages at camp clearing plot → Restored**

The player gathers Stone and Wood from the forest between ferment batches. Tormod gives Nails after the first sale. Each build stage is a single [E] interact at the plot.

## New Forage Sources

- **StonePile** — yields 1 Stone per forage, respawns daily. Same pattern as BerryBush (IInteractable, SpriteRenderer, BoxCollider2D trigger, `InteractType.Forage`).
- **FallenLog** — yields 1 Wood per forage, respawns daily. Same pattern as StonePile.
- 2-3 StonePiles and 2-3 FallenLogs scattered in the forest/near camp alongside the berry bushes.

## New Items

- **Stone** (`ContentDb.Stone`) — foraged, stackable (`canStack = true`), base price 1g. Used only for Homestead Foundation.
- **Wood** (`ContentDb.Wood`) — foraged, stackable, base price 2g. Used for Homestead Frame and Walls. Separate from Timber (a purchased renovation material for town buildings later).

## Build Stages at the Homestead Plot

| Stage | Materials | Interaction | Visual |
|-------|-----------|-------------|--------|
| 0 — BuildSign | None | `[E] Homestead Site (need 3 Stone)` or `[E] Build Foundation` | Small signpost sprite |
| 1 — Foundation | 3 Stone | `[E] Build Foundation` | Foundation outline |
| 2 — Frame | 3 Wood | `[E] Build Frame` | Wood frame |
| 3 — Walls | 2 Wood + 3 Nails | `[E] Build Walls` | Full building exterior, state = Restored |

After Walls: `BuildingState.Restored`, interior accessible via existing `InteriorManager`.

## Nails Source

Tormod gives the player 3 Nails during his recruitment dialogue (first sale interaction). This naturally gates Walls behind the player's first successful moonshine delivery.

The existing `SellerInteractable` / `SellManager` flow already handles the first-sale beat. The Nails grant happens as part of that same interaction (via `GameEvents` or a direct `InventoryManager.TryAdd` call in the sale completion path — implementation detail left to the plan).

## Plot Visibility

- **Before any interaction:** a `BuildSign` interactable at the camp clearing (small signpost SpriteRenderer + BoxCollider2D trigger + `BuildSign` component implementing `IInteractable`). Text shows material requirements.
- **After Foundation:** signpost replaced by foundation visual (sprite swap on the same GameObject).
- **After Frame:** frame visual (another sprite swap).
- **After Walls:** full building exterior (final sprite swap), state = `Restored`, interior accessible.

## State Machine Change

The Homestead skips the `Abandoned → Purchased → Cleared` pipeline entirely. New states:

```
BuildSign (new) → Foundation (new) → Frame (new) → Restored (existing)
```

This is **not** the same as the existing `BuildingState` enum. Options:
- Add `BuildingFoundation`, `BuildingFrame` to `BuildingState` enum (simplest, but mixes concerns)
- Keep `BuildingState` for the purchase-renovation pipeline and use a separate `HomesteadBuildStage` int/enum on a new `BuildSign` component (cleanest separation)

**Recommendation:** Separate `BuildSign` component with its own `BuildStage` enum. When build completes, it creates/activates the Homestead `Building` component at `Restored` state. This avoids polluting `BuildingState` with homestead-specific states and keeps the two pipelines completely independent.

Other buildings (Bakery, Mill, etc.) keep the existing Buy → Smash → Clear → Repair → Restored pipeline unchanged.

## Pacing Impact

- Day 1: Start with 3 Berry. Start Berry Shine ferment (3h). While waiting, forage berries + stone + wood near camp.
- First Tormod sale: get 3 Nails + gold. Start second ferment.
- While second ferment brews: finish Foundation + Frame, then build Walls with Wood + Nails from Tormod.
- Estimated time to Restored: ~25-35 min — within the 20-40 min target.

## Files Changed (Overview)

- New: `Assets/Scripts/BuildSign.cs` — IInteractable, build stage tracking, material consumption, sprite swaps
- New: `Assets/Scripts/StonePile.cs` — IInteractable, same pattern as BerryBush
- New: `Assets/Scripts/FallenLog.cs` — IInteractable, same pattern as BerryBush
- Modify: `Assets/Scripts/ContentDb.cs` — add Stone, Wood item defs
- Modify: `Assets/Scripts/GameEvents.cs` — add `HomesteadBuildStageChanged` event (or similar)
- Modify: `Assets/Scripts/UI/GameHUD.cs` — BuildSign interact prompt
- Modify: `Assets/Scenes/SampleScene.unity` — replace Homestead Building with BuildSign + forage objects, fix berry bush positions
- Modify: `Assets/Docs/BuildPlan.md` — update Phase 1 line items
- New: `Assets/Tests/EditMode/BuildSignTests.cs`
- New: `Assets/Tests/PlayMode/HomesteadBuildFlowTests.cs`

## Later Ideas (deferred)

- Tormod exclamation mark over head to guide player to talk to him and get nails (requires UI work)
- All UI except debug menu needs proper UI treatment (not just IMGUI placeholder styling)
- Interior construction stages for the Homestead (separate from exterior build)
- Nail economy beyond the initial Tormod gift (buying from General Store in later phases)
