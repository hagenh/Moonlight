# Sprite & Art Tracker

Every game object that renders a sprite. Status: **Done** = real art assigned, **Placeholder** = procedural colored square, **Needs art** = needs a real sprite drawn or picked from tileset.

## Scene-Placed Interactables

| Object | Current Sprite | Status | Notes |
|--------|---------------|--------|-------|
| BerryBush (×9) | `Grassland Spring@128x128_115` | Done | Purple berry bush from Grasslands tileset |
| StonePile (×3) | `Grassland Spring@128x128_114` | Done | Rock pile from Grasslands tileset |
| FallenLog (×3) | Brown 16×16 procedural | Needs art | Pick a log sprite from tileset or draw |
| FermentVat (camp, ×1) | `Interior Color 2@128x128_250` | Done | Interior pot sprite |
| FermentVat (interior, ×3) | `Interior Color 2@128x128_250` | Done | Interior pot sprite |
| BuildSign | Tan 16×16 procedural | Needs art | Signpost/foundation marker sprite |
| Homestead (Building) | Colored square (facadeRenderer tint) | Needs art | Full building exterior sprite |
| Bed | Scene-assigned | Verify | Check if real sprite in scene |
| Building facades (Bakery, General Store, Road House, Mill, Boarding House, Constable) | Town tileset sprites | Done | Finished buildings have real facade sprites |
| DebrisPile | Scene-assigned | Verify | Check if real sprite in scene |
| DeliveryPoint | Scene-assigned | Verify | Check if real sprite in scene |
| ExitDoor | Scene-assigned | Verify | Check if real sprite in scene |
| Guard | Directional idle/walk sprites | Done | Uses DirectionalSpriteAnimator |
| Player | Placeholder | Needs art | Directional idle/walk animations |
| SellerInteractable (Tormod) | `Texture2D.whiteTexture` 4×4 | Needs art | NPC or stall sprite |
| SellerInteractable (Cart) | `Texture2D.whiteTexture` 4×4 | Needs art | Cart sprite |
| Crate | Two-sprite prefab (bottom+top tile) | Done | CratePrefab in ContentDb, carry sprite via CrateCarrySprite |
| Debris | `Texture2D.whiteTexture` 4×4 | Needs art | Rubble/debris sprite |

## Tileset Sources

| Tileset | Path | Usage |
|---------|------|-------|
| Grasslands | `Assets/Sprite/Grasslands_tileset/Grassland Spring@128x128.png` | BerryBush, StonePile, terrain |
| Interior | `Assets/Sprite/Interior/Interior Color 2@128x128.png` | FermentVat, interior objects |
| Town | `Assets/Sprite/2D Hand Painted - Town Tileset/128x128/Town Spring@128x128.png` | Buildings, town props |

## BuildSign Stage Sprites

When the BuildSign advances stages, the visual should change. Currently uses color tints on a single placeholder.

| Stage | Current Visual | Needed Sprite |
|-------|---------------|---------------|
| Site | Tan placeholder | Signpost or stake marker |
| Foundation | Gray placeholder | Stone foundation outline |
| Frame | Brown placeholder | Wood frame skeleton |
| Walls | Tan placeholder | Complete building exterior |

## Collider Status

All forage interactables now have **both** a trigger collider (for interaction) and a solid collider (for physics blocking). Runtime `Create()` methods include both colliders.

| Object | Trigger Collider | Solid Collider |
|--------|-----------------|----------------|
| BerryBush | 0.6×0.8 | 0.5×0.5 |
| StonePile | 0.8×0.6 | 0.5×0.4 |
| FallenLog | 1.0×0.5 | 0.6×0.3 |
| FermentVat | scene-assigned | 0.8×0.6 (offset -0.1y) |
| BuildSign | 0.8×1.0 | 0.8×1.0 |
| Crate | 0.6×0.6 | None (picked up on interact) |
| Debris | 0.6×0.6 | None (picked up on interact) |

## Known Texture2D.whiteTexture Bugs

`Crate.Create()` and `Debris.Create()` still use `Texture2D.whiteTexture` (4×4 px). These create invisible sprites at 16 PPU. Need same fix as BerryBush: use `new Texture2D(16,16)` with `SetPixels32`.
