# Pre-Building-Stage Visual Design

**Date:** 2026-07-28

**Goal:** Replace the flat tinted placeholder square that every unbuilt/unrestored building shows with a hand-built "staked-out construction site" visual, in the two places that placeholder currently exists: the Homestead's `BuildSign` (Site stage only) and the shared `Building.prefab` used by all six town buildings (Abandoned state only).

## Current state (verified directly in the repo/scene)

- The user has already built a construction-site visual by hand in `SampleScene`: a root GameObject named `Building_PreBuilingStage` (typo) at world (13, 0), containing 21 child GameObjects, each a single Grasslands-tileset sprite (`Grassland Spring@128x128_*`, no colliders). The children's local positions trace a hollow 6×6 border — a fence/stake perimeter, empty in the middle — not a filled tile grid. Confirmed visually via in-editor screenshots.
- The bottom-left corner of that border is local (0, -5) within the group — `Grassland Spring@128x128_105`. The user has designated this corner as the interaction anchor ("where I have placed a sign").
- This cluster is **not** near either build system today. It sits at world (13, 0), disconnected from both:
  - The real, functional `BuildSign` (`Assets/Scripts/BuildSign.cs`), a scene-placed GameObject at world (-23, 7), scale (0.5, 0.5, 1), with `BoxCollider2D` triggers and the `homesteadBuilding` reference wired. It currently renders one procedural `SpriteRenderer`, tinted/scaled per `BuildStage` (`_stageColors`, `_stageScales` — Site/Foundation/Frame/Walls).
  - `Assets/Prefabs/Building.prefab`, the single shared prefab instanced (via `PrefabInstance` overrides of `buildingName`/`purchaseCost`/`dailyIncome`/etc.) for Bakery, General Store, Road House, Mill, Boarding House, Constable, **and** the Homestead. Its `Building.cs` cycles `Abandoned → Purchased → Cleared → Restored`, and `RefreshVisuals()` tints one `facadeRenderer` (a GameObject named `Square`, local scale 4×4, wired via `facadeRenderer: {fileID: 6299132845729380538}`) a different flat color per state. A separate always-present child, `Building_Sprites`, holds the real facade — dozens of individually placed 1×1 sprite tiles, same compositional pattern as the user's new construction-site cluster — and is revealed once `Square` (or, for the Homestead, `BuildSign`'s own completion logic) is turned off at `Restored`.
- There's also an unrelated, unused GameObject named `BuildSign` (no script, no sprite assigned) at world (-14, 2) — scene debris from earlier work, not connected to anything. **To be deleted** as part of this work.

## Decisions

1. **Scope:** only the *initial* pre-work state gets the new visual in both systems.
   - Homestead: `BuildStage.Site` only. Foundation/Frame/Walls keep the existing tint-and-scale placeholder unchanged.
   - Town buildings: `BuildingState.Abandoned` only. Purchased/Cleared keep the tinted `Square`, so the smash-debris-hammer renovation sequence still reads as visual progress. Restored is unaffected (`Building_Sprites` already handles it).
2. **Asset:** the hand-built cluster becomes a reusable prefab, `Assets/Prefabs/BuildingPreBuildingStage.prefab` (typo fixed: `PreBuilingStage` → `PreBuildingStage`). Its internal structure (21 child sprite tiles, local positions unchanged) is not otherwise modified — it already matches the project's existing convention for multi-tile composed art (`Building_Sprites` uses the identical pattern: one parent, many individually placed 1×1 tile children).
3. **Homestead wiring:** `BuildSign.cs` gets `[SerializeField] private GameObject siteVisual`. This references an **independent sibling** instance of the new prefab — *not* a child of `BuildSign`'s own transform, so it does not inherit `BuildSign`'s 0.5 scale or the `_stageScales` rescaling applied on `AdvanceStage`. That instance is positioned so its bottom-left tile (local (0, -5) within the prefab) lands exactly on `BuildSign`'s existing world position (-23, 7) — i.e. the instance root sits at world (-23, 12). `BuildSign`'s own collider and transform are untouched.
   - `Awake()` and `AdvanceStage()` set `siteVisual.SetActive(_stage == BuildStage.Site)`.
   - The existing procedural `SpriteRenderer` on `BuildSign` stops rendering while `Stage.Site` is active (its sprite is otherwise fully covered by `siteVisual`), and resumes its normal tint/scale behavior from Foundation onward.
4. **Town building wiring — corrected after checking the live scene.** `Square` (the tinted placeholder) is `activeSelf: false` on all 7 building instances today, and nothing in `RefreshVisuals()` ever activates it — it only sets `.color`. `Building_Sprites` (the real, finished facade) is `activeSelf: true` on all 7 instances unconditionally. So today, every building shows its finished facade regardless of state, including `Abandoned`/unpurchased — the state-tint code is dead in practice. Confirmed with the user this is a real gap worth fixing as part of this change, not a premise to preserve.

   `Building.prefab` gets a new child instance of the new prefab (alongside `Square` and the `Building_Sprites` container), at local position (-2.5, 2.5, 0) with scale (1, 1, 1) — this centers the 6×6 tile grid over the same area `Square`'s 4×4 currently covers (`Square` needs its own 4× scale because it's one 1×1 sprite; the new prefab's tiles are already laid out at native 1-unit spacing and need no scaling).
   - `Building.cs` gets `[SerializeField] private GameObject preBuildVisual`.
   - `RefreshVisuals()` sets `preBuildVisual.SetActive(State == BuildingState.Abandoned)` **and** `Building_Sprites`'s active state to `State != BuildingState.Abandoned` (needs a new `[SerializeField] private GameObject buildingSprites` reference, since `Building.cs` currently holds no reference to that container at all). `Square`'s dead tint-color code is left untouched — out of scope, not asked for.
   - Net effect: `Abandoned` now shows only the fence-outline (facade hidden); `Purchased`/`Cleared`/`Restored` show `Building_Sprites` exactly as they do today (unchanged, since it was already unconditionally on for those states).
   - Because this lives in the shared prefab, the change propagates to all six town buildings automatically — no per-building edits needed.
5. **Cleanup:** delete the stray unused `BuildSign`-named GameObject at (-14, 2) (no sprite, no script) as part of this work.
6. **Out of scope:** no new colliders (the fence border stays walk-through, matching how `Square`/`Building_Sprites` have no collision shaping of their own today); Purchased/Cleared/Restored art; the town buildings' final facades (already real art via `Building_Sprites`).

## File structure

| File | Fate | Responsibility |
|---|---|---|
| `Assets/Prefabs/BuildingPreBuildingStage.prefab` | Create (from the existing scene cluster) | The reusable construction-site visual |
| `Assets/Scripts/BuildSign.cs` | Modify | `siteVisual` field + toggle in `Awake`/`AdvanceStage` |
| `Assets/Scripts/Building.cs` | Modify | `preBuildVisual` field + toggle in `RefreshVisuals` |
| `Assets/Prefabs/Building.prefab` | Modify (editor) | New child instance, centered over `Square`'s footprint |
| `Assets/Scenes/SampleScene.unity` | Modify (editor) | Replace scratch `Building_PreBuilingStage` cluster with a properly positioned prefab instance wired to `BuildSign`; delete the stray `BuildSign` debris object |

## Testing

This is scene/prefab wiring plus two trivial one-line `SetActive` toggles keyed off existing enums — consistent with how `BuildSign`'s existing `_stageColors`/`_stageScales` array indexing is already inline rather than pulled into `Rules/`, there's no new pure-logic surface here worth a `Rules/` extraction or an EditMode test. Verification is a playtest: watch a town building through Abandoned → Purchased and confirm the visual swap and that colored-square progress feedback still reads for Purchased/Cleared; watch the Homestead through Site → Foundation and confirm the same for `BuildSign`.

## Self-review

**Placeholder scan:** no TBD/TODO — every position, scale, and field name above is a concrete decided value, not a placeholder.

**Internal consistency:** the Homestead's `siteVisual` is deliberately *not* parented under `BuildSign` (to dodge scale inheritance); the town buildings' `preBuildVisual` *is* parented under the `Building` root (to match how `Square`/`Building_Sprites` are already parented there) — these look like inconsistent choices but are correct for their own contexts: `BuildSign` rescales itself 1×→2×→4×→6× as a placeholder-progress signal that must not affect the new art, while `Building`'s root never rescales.

**Scope:** focused enough for one implementation plan — two scripts, one new prefab, one scene edit, one existing-prefab edit.

**Ambiguity check:** "bottom-left square" was the one genuinely ambiguous phrase in the original request; resolved by inspecting the scene directly rather than guessing (local (0,-5), `Grassland Spring@128x128_105`).
