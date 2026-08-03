# Inventory UI Finish — Design

**Goal:** Close out the Inventory UI backlog item: replace keyboard polling with the proper Input System action, and skin the panel with the Post-Baroque UI Kit.

**Context:** The uGUI inventory (20-slot grid, detail sidebar, right-click drop) is functional and wired in `SampleScene`. Two known issues remain from the 2026-08-02 plan: raw `Keyboard.current` polling in `InventoryUI.Update()`, and plain colored rectangles instead of real UI sprites. The `Inventory` action (Button, `<Keyboard>/i`) already exists in the **Player** action map of `InputSystem_Actions.inputactions` and the generated wrapper includes `Player.Inventory`.

## Part 1 — Input action

- `InventoryUI` creates its own `InputSystem_Actions` instance in `Awake` (same pattern as `RecipeBookUI`).
- `OnEnable`: enable `Player.Inventory` (the single action, not the whole map) and subscribe `performed` → toggle open/close. `OnDisable`: unsubscribe and disable.
- Delete `Update()` entirely:
  - I-key poll replaced by the action.
  - Escape poll is redundant — `PlayerController` fires `GameEvents.MenuCloseRequested` on Escape while `IsMenuOpen`, and `InventoryUI` already subscribes to it.
- The Player map stays enabled while menus are open (`IsMenuOpen` only gates movement logic), so I still closes the panel.
- No behavior change: I toggles, Escape closes, player frozen while open.

## Part 2 — Visual polish (Post-Baroque UI Kit)

Assets live under `Assets/Sprite/Post-Baroque UI Kit/`.

- **Window root:** ornate **Panels B** frame, dark navy fill variant, as a 9-sliced `Image`. Requires setting the sprite's border in import settings.
- **Slots:** brown **Slots A** sprite as each slot's background image, 9-sliced. Selection is shown by swapping the background to the gold/light slot variant instead of the current color tint (small `InventorySlotView` change: two `Sprite` fields, swap in `Render`).
- **Sidebar:** simpler **Panels A** navy variant as a sub-panel behind the detail text.
- **Close button:** sprite from `UI_Buttons`.
- Exact sub-sprite files (e.g. which `UI_PanelsB_XX.png` is the navy variant) chosen at implementation time by inspecting the individual PNGs.
- Import settings (Sprite mode, 9-slice borders) set via `.meta`/importer edits; scene wiring done via Unity MCP against `SampleScene`.

## Verification

- All existing EditMode and PlayMode tests pass (no logic changes beyond `InventoryUI`/`InventorySlotView`).
- Editor screenshot of the open inventory confirms the skinned look.
- Manual playtest: I opens (player frozen), click slot → sidebar detail, right-click → drop, I/Escape closes.

## Out of scope

- Hotbar, SfxManager (separate backlog items).
- Skinning other menus (RecipeBook, Sell, Dialogue) with the kit.
- Drag-and-drop slot rearranging.
