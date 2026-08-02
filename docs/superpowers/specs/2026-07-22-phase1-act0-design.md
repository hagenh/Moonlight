# Phase 1 — Act 0: The Tent Prologue (Design Spec)

Date: 2026-07-22
Status: Approved in brainstorming. Implements BuildPlan Phase 1 against the front-town empire redesign spec.

## Core Loop

Forage berries → wild-ferment in campfire pot → carry 2 jars to Tormod at dusk → sell → repeat until homestead is affordable → purchase and restore homestead → proper still unlocked → game proper begins.

Pacing: 20–40 real minutes, 2–3 in-game days. Player still in tent on day 4 = numbers wrong.

## Map

Minimal tilemap extension west of the current town strip: a small camp clearing (~8×6 tiles) with tent, campfire pot, and 2–3 berry bushes. Reachable by walking off the existing map edge. Full near-forest expansion is Phase 2's scope.

Player spawns at the camp clearing. **This clearing is the player's own land**, not just a random spot in the wilderness. The tent is the player's starter shelter on that land (cosmetic landmark: SpriteRenderer + trigger collider). The homestead (see below) is built/restored on this same plot rather than purchased as a separate building elsewhere; after restoration the tent stays as a crate-drop/stash marker beside it.

> **Design change pending (2026-07-23):** earlier drafts (and the Phase 1 implementation plan) placed the homestead as a derelict building "at town edge," bought and restored independently of the camp. That is superseded by the note above — the homestead now belongs on the camp clearing itself, as the upgrade path for land the player already owns. This has **not been implemented yet**; the current scene/code still reflect the old town-edge placement (see `2026-07-22-phase1-act0-tent-prologue.md` checkboxes and the commits that rebuilt Homestead as a standalone building). Reconciling scene layout is future work.

## New Content

### Items (ContentDb)

- `BerryShine` — `new ItemDef("berry_shine", "Berry Shine", false, 15, true)` — Act 0 moonshine, cheaper than Basic (25g).
- `Berry` — `new ItemDef("berry", "Berry", true, 2)` — foraged ingredient.

### Recipe (FermentManager)

- `"Berry Shine"` — 6h ferment, yields 2 jars, ingredient: 3 Berry. No yeast, no grain, no water (wild yeast). No building gate, no min reputation — always unlocked.

### Existing recipe change

- `"Basic Mash"` gets `unlockedByBuildingId = "Homestead"` (currently null). Berry Shine is the only recipe available in Act 0.

### Economy math

Berry Shine: 15g base × 2 jars = 30g per batch. Tormod pays `EconomyRules.GetSellPrice(item)`. Homestead price: 80g. 3 batches × 30g = 90g > 80g. Buffer for buying berries from cart if needed.

## Campfire Pot

A `FermentVat` placed in the camp clearing. No new component — reuses the full FermentVat/FermentBatch/FermentManager pipeline. The pot is a single vat (the existing `vat.State == VatState.Empty` guard already enforces one batch at a time). No special max-slots logic needed for Phase 1.

Visual: scene-level SpriteRenderer with a pot appearance, wired to the FermentVat component.

## Berry Bush

New `BerryBush : MonoBehaviour, IInteractable`.

- `InteractType` = new enum value `Forage`. GameHUD prompt: `"[E] Forage"`.
- `Interact()`: adds 1 Berry to `InventoryManager`, disables the bush's SpriteRenderer and collider.
- Subscribes to `GameEvents.DayEnded` in OnEnable: re-enables sprite and collider (respawn).
- Unsubscribes in OnDisable.
- Static `Create()` factory method per project conventions: builds GameObject with SpriteRenderer (purple/red tint on white texture), BoxCollider2D (trigger, ~0.6×0.8), Interactable layer.
- Placed in scene at camp clearing (2–3 instances).

## Tormod at the Roadhouse Back Door

Reuses the existing `SellerInteractable` + `SellManager` + `SellUI` pipeline.

`SellManager` additions:
- `[SerializeField] private Transform tormodPosition;` — scene marker near Roadhouse back door.
- `[SerializeField] private int tormodArriveHour = 18;`
- `[SerializeField] private int tormodLeaveHour = 6;`
- `private SellerInteractable _tormodInstance;`
- In `OnHourChanged`: spawn Tormod at `tormodArriveHour`, despawn at `tormodLeaveHour`. Same pattern as the TravelingCart.
- Tormod buys non-ingredient bottles via existing `EconomyRules.IsSellable` — Berry Shine qualifies.

`SellerType.Tormod` already exists. `SellerInteractable.Create` already handles the Tormod color case. No new enum values.

No special Tormod dialogue in Phase 1 — the sell interaction is the tutorial. Recruitment beat is Phase 5.

## Homestead

Sits on the player's own camp clearing (see Map above), not a separate town-edge lot — the player is upgrading land they already occupy, tent to proper house. This reframes the transaction as "build/restore your own homestead" rather than "buy someone else's derelict building," and gives the smash-boards/clear-debris steps a visible payoff: cleaning up the camp the player has been living in.

A regular `Building` component:
- `buildingName = "Homestead"`, `purchaseCost = 80` (the cost represents materials/labor to build, not a real-estate purchase)
- Full renovation pipeline: smash boards → clear debris → repair with timber/nails — this doubles as cleaning up the player's own camp
- Once `Restored`: interior accessible via `InteriorManager`; a `FermentVat` is placed inside (proper still), superseding the campfire pot
- The Basic Mash recipe is gated on `unlockedByBuildingId = "Homestead"`, so the proper still recipes only appear after restoration
- Progression framing: tent (temporary shelter) → homestead (owned, built by the player) on the same ground — reinforces "this is your land" rather than relocating to a new one

## InteractType Extension

Add `Forage` to the `InteractType` enum. Add `InteractType.Forage => "[E] Forage"` to the GameHUD prompt switch.

## GameEvents

No new events needed. Existing events cover all Phase 1 interactions: `InventoryChanged`, `DayEnded`, `VatStateChanged`/`BatchProgressed`, `SellerArrived`/`SellerLeft`, `BuildingStateChanged`, `CashChanged`.

## New Files

| File | Purpose |
|------|---------|
| `Assets/Scripts/BerryBush.cs` | IInteractable forage point, respawns daily |

## Modified Files

| File | Change |
|------|--------|
| `Assets/Scripts/ContentDb.cs` | Add Berry, BerryShine items + Register calls |
| `Assets/Scripts/FermentManager.cs` | Add Berry Shine recipe, gate Basic Mash behind Homestead |
| `Assets/Scripts/IInteractable.cs` | Add `Forage` to InteractType enum |
| `Assets/Scripts/UI/GameHUD.cs` | Add Forage prompt case |
| `Assets/Scripts/SellManager.cs` | Add Tormod dusk/dawn spawn logic |
| `Assets/Scenes/SampleScene.unity` | Camp clearing tilemap, tent, pot (FermentVat), berry bushes, Tormod position marker, homestead Building, player spawn point |

## What This Phase Does NOT Build

- Near-forest tilemap expansion (Phase 2)
- Deep woods, run routes, destinations (Phase 2–3)
- Tormod recruitment beat / dialogue (Phase 5)
- Stash container UI (tent is just a crate drop zone)
- Any new GameEvents (existing events sufficient)
- Interior scenes for the homestead beyond a basic room with a vat

## Pacing Guardrail

Hard gate: player still in tent on day 4 = numbers wrong, fix before proceeding. The 80g homestead price / 30g per batch ratio is tuned for 3 sales in 2–3 days. Berry bushes respawn daily, so each day yields 2–3 berries per bush. 3 bushes = 9 berries/day = 3 Berry Shine batches over 2 days if the player is efficient. Sleep advances fermentation.
