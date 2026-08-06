# Known Issues

## Visual

### ~~FallenLog instances have the wrong sprite~~ (Fixed)
All 12 FallenLog scene instances now use the correct sub-sprite `Grassland Spring@128x128_62` from the Grassland tileset.

**Files:** `Assets/Scripts/FallenLog.cs`, `Assets/Scenes/SampleScene.unity`

### ~~Placeable infrastructure items (B key) are bare sprites, not prefabs~~ (Fixed)
Placeholder prefabs (`LamppostPlaceholder`, `BenchPlaceholder`, etc.) now include `BoxCollider2D` components. `ContentDb` `[SerializeField]` fields are wired to these prefabs, and `BuildModeController` instantiates them when placing. No bare-sprite instances remain in the scene.

**Files:** `Assets/Scripts/InfrastructureManager.cs`, `Assets/Scripts/ContentDb.cs`, `Assets/Prefabs/*Placeholder.prefab`

### ~~Errant DeliveryPoint in the scene~~ (Fixed)
No `DeliveryPoint` GameObject found in `SampleScene` — already removed or was never present. Delivery points are created dynamically by `SellManager` when the cart arrives.

**Files:** `Assets/Scenes/SampleScene.unity`

### FermentVat has no sprite
FermentVats currently render as a colored square (tinted via `vatRenderer.color` in `FermentVat.cs:84`) with no actual sprite assigned. The `CampfirePot` (interior tileset sprite from the kitchen/campfire set) has a pot visual that would work as a stand-in for the vat.

**Files:** `Assets/Scripts/FermentVat.cs`, `Assets/Prefabs/FermentVat.prefab`

### Homestead built stage uses wrong sprite
`Homestead.cs:100` swaps to `builtSprite` on completion, but the currently assigned sprite (`HomesteadBuilt.png`) looks bad. Should use `BuildingFacade.png` instead.

**Files:** `Assets/Scripts/Homestead.cs:13`, `Assets/Sprite/HomesteadBuilt.png`, `Assets/Sprite/BuildingFacade.png`

### Homestead built stage is half the expected size
When the Homestead reaches the final built stage, it appears at roughly half the size it should be. Likely a `localScale` or sprite import pixels-per-unit mismatch — the built sprite needs to be displayed at 2x the current scale to match the surrounding buildings and expected visual footprint.

**Files:** `Assets/Scripts/Homestead.cs`, `Assets/Scenes/SampleScene.unity`

### Homestead middle build stages lack dedicated sprites
First (Site) and final (Walls/built) stages have proper sprites, but Foundation and Frame stages just recolor the existing sprite via `_stageColors`. Needs dedicated construction-stage art when available.

**Files:** `Assets/Scripts/Homestead.cs:19-24`

### Resident.Create() uses a white placeholder sprite
`Resident.Create()` generates a white 4x4 texture tinted with `def.spriteColor` instead of using a real sprite. Since residents are spawned dynamically (via `ResidentManager`), this placeholder is what the player actually sees.

**Files:** `Assets/Scripts/Resident.cs:22-26`

### SellerInteractable.Create() uses a white placeholder sprite
Same issue — `SellerInteractable.Create()` builds a white 4x4 texture tinted by seller type. Tormod and the Traveling Cart both appear as colored squares when spawned dynamically via `SellManager`.

**Files:** `Assets/Scripts/SellerInteractable.cs:43`

### Dropped items are oversized and use the wrong sprite
`DroppedItem.Create()` reuses `item.icon` — the small GUI hotbar/inventory icon — as the in-world sprite, and only applies a `localScale` of `(0.5, 0.5, 1)`. GUI icons aren't imported with world-appropriate pixels-per-unit, so on the ground they render far larger than the item's collider (also `0.5x0.5`) suggests. Needs a dedicated in-world pickup sprite per item/category, sized correctly (see also the "World pickup sprites" backlog item).

**Files:** `Assets/Scripts/DroppedItem.cs:27-35`

### Crate is invisible on the ground and when carried
`Crate.Create()` has two paths — prefab (requires `ContentDb.CratePrefab` wired in Inspector) and fallback (tiny green square from `Texture2D.whiteTexture`). If the prefab field isn't assigned, the fallback is nearly invisible at 0.25 world units. When carried, `ShowCarrySprite()` copies the crate's sprite to the player's `carrySpriteRenderer`, but the Player prefab's CarrySprite child has no material assigned and no default sprite, so it renders invisible in URP even when a sprite is set. The vat now collects directly to inventory (`TryCollectBatch`), so crates are no longer spawned in normal gameplay — but the underlying visibility issues remain if crates are used in the future.

**Files:** `Assets/Scripts/Crate.cs`, `Assets/Scripts/Player/PlayerController.cs:252-265`, `Assets/Prefabs/Player.prefab`

## Design / Placeholder

### FermentVats in the Bakery are temporary
FermentVats are currently placed inside the Bakery scene as a temporary gameplay location. The intended design is that players purchase and place FermentVats inside their own homestead (or similar player-owned space), not find them pre-placed in town buildings.

**Files:** `Assets/Scripts/FermentVat.cs`, `Assets/Scripts/FermentManager.cs`
