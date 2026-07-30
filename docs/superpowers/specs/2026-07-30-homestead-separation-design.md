# Homestead Separation from Building

## Problem

The Homestead currently reuses the `Building` prefab and `Building` component, inheriting renovation mechanics (purchase, smash boards, clear debris, repair) that are irrelevant to it. The Homestead has its own construction flow via `BuildSign` (Site→Foundation→Frame→Walls). Additionally, the `BuildingPreBuildingStage.prefab` uses 21 child GameObjects with individual SpriteRenderers where a single merged sprite would be simpler and more performant.

## Design

### New `Homestead` Component

File: `Assets/Scripts/Homestead.cs`

A lightweight `MonoBehaviour, IInteractable` with:

- `[SerializeField] private Sprite builtSprite` — the completed homestead sprite
- `[SerializeField] private Collider2D interactCollider` — the trigger collider over the sign area (bottom-left corner)
- `bool IsBuilt { get; private set; }`
- `InteractType => InteractType.Building`
- `Interact()` — when built, no-op for now (placeholder for future homestead interactions)
- `SetBuilt()` — swaps `SpriteRenderer.sprite` to `builtSprite`, sets `IsBuilt = true`

No renovation fields, no smash/repair logic, no debris, no income, no window lights.

### Editor Sprite Merge Tool

File: `Assets/Scripts/Editor/SpriteMerger.cs`

A menu item `Tools > Merge Homestead Sprites` that:

1. Reads all child SpriteRenderers from `BuildingPreBuildingStage.prefab`, composites their sprites at their grid positions into a single `Texture2D`, saves as `Assets/Sprite/HomesteadPreBuild.png`
2. Reads all child SpriteRenderers from `Building.prefab`'s `Building_Sprites` child object, composites them into `Assets/Sprite/HomesteadBuilt.png`
3. Both assets get imported as single sprites (Sprite mode: Single)

The grid is 6 wide × 6 tall (positions 0-5 in X, 0 to -5 in Y) with 128px tiles at 100 PPU → each tile is 1 world unit. Final texture size: 768×768 pixels.

### New `Homestead.prefab`

File: `Assets/Prefabs/Homestead.prefab`

Single root GameObject replacing `BuildingPreBuildingStage.prefab`:

- `SpriteRenderer` using `HomesteadPreBuild.png`
- `Homestead` component with `builtSprite` wired to `HomesteadBuilt.png` sprite
- `BoxCollider2D` (solid) — full bounds of the sprite
- `BoxCollider2D` (trigger) — positioned over bottom-left corner (sign area), this is the `interactCollider`

### Changes to Existing Files

#### `BuildSign.cs`
- Change `Building homesteadBuilding` field to `Homestead homestead`
- `CompleteBuild()` calls `homestead.SetBuilt()` instead of activating a Building and setting `BuildingState.Restored`
- Remove the `transform.Find("Square")` hack

#### `GameEvents.cs`
- No changes needed. `HomesteadBuildStageChanged` event stays as-is.

#### `BuildingManager.cs`
- No changes needed. Already only manages `Building` instances.

#### `GameHUD.cs`
- No changes needed. BuildSign interaction text stays as-is.

#### `BuildSignTests.cs`
- Replace `Building` references with `Homestead`
- Update `Interact_CompleteBuild_EnablesHomesteadAtRestored` to assert `homestead.IsBuilt` instead of `BuildingState.Restored`

### Files to Remove/Deprecate

- `BuildingPreBuildingStage.prefab` — replaced by `Homestead.prefab` with merged sprite

### Scene Changes

- Replace `BuildingPreBuildingStage` prefab instance in the scene with `Homestead` prefab instance
- Wire `BuildSign.homestead` to the new `Homestead` instance
- The Homestead's `Building` (Bakery) child is no longer needed; remove it from the scene hierarchy under the homestead area
