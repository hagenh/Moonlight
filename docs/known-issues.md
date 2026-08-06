# Known Issues

## Visual

### FallenLog instances have the wrong sprite
FallenLog scene instances are showing placeholder sprites from placeable infrastructure items (e.g. bench sprites) instead of the intended fallen-log sprite. The correct sprite exists in the `Grassland Spring@128x128` tileset (sub-sprite `_115`), but some scene instances were wired to wrong sprites. Additionally, these foraging nodes are placed as bare GameObjects in the scene rather than prefabs, making them harder to maintain and prone to this kind of wiring mistake. They should be converted to prefabs so sprite/collider setup is managed in one place.

**Files:** `Assets/Scripts/FallenLog.cs`, `Assets/Scenes/SampleScene.unity`

### Errant DeliveryPoint in the scene
A `DeliveryPoint` GameObject is present in `SampleScene` but should not be — delivery points are created dynamically by `SellManager` when the cart arrives. The static scene instance should be removed.

**Files:** `Assets/Scenes/SampleScene.unity`

### FermentVat has no sprite
FermentVats currently render as a colored square (tinted via `vatRenderer.color` in `FermentVat.cs:84`) with no actual sprite assigned. The `CampfirePot` (interior tileset sprite from the kitchen/campfire set) has a pot visual that would work as a stand-in for the vat.

**Files:** `Assets/Scripts/FermentVat.cs`, `Assets/Prefabs/FermentVat.prefab`

### Homestead built stage uses wrong sprite
`Homestead.cs:100` swaps to `builtSprite` on completion, but the currently assigned sprite (`HomesteadBuilt.png`) looks bad. Should use `BuildingFacade.png` instead.

**Files:** `Assets/Scripts/Homestead.cs:13`, `Assets/Sprite/HomesteadBuilt.png`, `Assets/Sprite/BuildingFacade.png`

### Homestead middle build stages lack dedicated sprites
First (Site) and final (Walls/built) stages have proper sprites, but Foundation and Frame stages just recolor the existing sprite via `_stageColors`. Needs dedicated construction-stage art when available.

**Files:** `Assets/Scripts/Homestead.cs:19-24`

### Resident.Create() uses a white placeholder sprite
`Resident.Create()` generates a white 4x4 texture tinted with `def.spriteColor` instead of using a real sprite. Since residents are spawned dynamically (via `ResidentManager`), this placeholder is what the player actually sees.

**Files:** `Assets/Scripts/Resident.cs:22-26`

### SellerInteractable.Create() uses a white placeholder sprite
Same issue — `SellerInteractable.Create()` builds a white 4x4 texture tinted by seller type. Tormod and the Traveling Cart both appear as colored squares when spawned dynamically via `SellManager`.

**Files:** `Assets/Scripts/SellerInteractable.cs:43`

## Design / Placeholder

### FermentVats in the Bakery are temporary
FermentVats are currently placed inside the Bakery scene as a temporary gameplay location. The intended design is that players purchase and place FermentVats inside their own homestead (or similar player-owned space), not find them pre-placed in town buildings.

**Files:** `Assets/Scripts/FermentVat.cs`, `Assets/Scripts/FermentManager.cs`
