# Inventory Hotbar — Design

**Backlog item:** `docs/backlog.md` → UI → `[M] Hotbar — bottom bar with item slots, press 1-9 to select active item/tool (like Minecraft / Stardew Valley)`

## Goal

Add a Minecraft/Stardew-style hotbar: a row of 9 slots always visible on the HUD, mirroring the first 9 slots of the player's existing 20-slot inventory. Number keys 1-9 select which of those 9 slots is "active," shown with a visual highlight.

**Explicitly out of scope:** actually *using* the active item. No interactable in the codebase (Building, FermentVat, Stand, DeliveryPoint) currently accepts an item handed to it directly — delivery/selling goes through separate `Crate` objects the player carries, not inventory slots. Wiring up a use-action is deferred to a new backlog item (see "Backlog addition" below); this feature only adds selection state and its visual.

## Architecture

Three independent pieces, in dependency order:

1. **Data + events** — `InventoryManager` gains active-slot state.
2. **Input** — a new `Hotbar` action bound to keys 1-9.
3. **UI** — a new `HotbarUI` component + scene wiring, consuming 1 and 2.

### 1. Data + events

`InventoryManager` (existing singleton, already owns the 20-slot `Inventory`) gains:

```csharp
public const int HotbarSlotCount = 9;
public int ActiveSlotIndex { get; private set; }

public void SetActiveSlot(int index)
{
    if (index < 0 || index >= HotbarSlotCount) return;
    if (index == ActiveSlotIndex) return;
    ActiveSlotIndex = index;
    GameEvents.OnActiveSlotChanged(index);
}
```

`GameEvents` gains:

```csharp
public static event System.Action<int> ActiveSlotChanged;
public static void OnActiveSlotChanged(int index) => ActiveSlotChanged?.Invoke(index);
```

`ActiveSlotIndex` defaults to `0`. No persistence — matches the rest of the inventory state (in-memory only, no save system exists yet).

### 2. Input

Add one new `Hotbar` action (type `Button`) to the `Player` map in `InputSystem_Actions.inputactions`, with 9 bindings: `<Keyboard>/1` through `<Keyboard>/9`, all on the same action. A single callback distinguishes which key fired via `ctx.control.name` (Unity keyboard digit controls are named `"1"`–`"9"`), so the generated `IPlayerActions` interface only grows by one method instead of nine.

This does not touch `PlayerController`. The existing (unused, no-op) `Previous`/`Next` actions already bound to keys 1/2 are left alone — they're dead stubs unrelated to this feature; both callbacks firing on the same keypress is harmless.

### 3. UI: `HotbarUI`

New script `Assets/Scripts/UI/HotbarUI.cs`, following the same self-contained-input pattern `InventoryUI` already uses (owns its own `InputSystem_Actions` instance rather than routing through `PlayerController`):

- Serialized fields: `Transform slotContainer`, `InventorySlotView slotTemplate` — wired in-editor/via MCP, no self-bootstrapped hierarchy beyond instantiating the 9 slot views (same pattern as `InventoryUI.BuildGrid()`).
- `Awake`: instantiate 9 `InventorySlotView` copies into `slotContainer`, calling `view.Initialize(i, null)` (passing `null` for the parent is already a safe no-op in `InventorySlotView.OnPointerClick` — hotbar slots aren't clickable, only keyboard-driven). Disable the template.
- `OnEnable`: subscribe to `GameEvents.InventoryChanged`, `GameEvents.ActiveSlotChanged`, and `_input.Player.Hotbar.performed`; enable the action. Refresh once immediately (so the bar is correct before the first event fires).
- `OnDisable`: mirror unsubscribe/disable.
- `OnHotbarKey(ctx)`: `int.Parse(ctx.control.name) - 1` → `InventoryManager.Instance.SetActiveSlot(index)`.
- `Refresh()`: for `i` in `0..8`, `slotViews[i].Render(InventoryManager.Instance.Slots[i], i == InventoryManager.Instance.ActiveSlotIndex)`.

No changes to `InventorySlotView` — it's reused as-is.

**Scope boundary:** the full `InventoryUI` screen's own click-to-select highlight (for viewing item details in the sidebar) is a separate, independent selection concept from the hotbar's active-slot highlight. They are not cross-wired — slot 0-8 can show different highlight states in each panel simultaneously.

## Scene wiring (via Unity MCP)

- New `Hotbar` panel as a child of the existing `HUDCanvas` (the always-on canvas already holding cash/rep/toast HUD elements), anchored bottom-center.
- `HorizontalLayoutGroup` container holding 9 slot instances stamped from a template `InventorySlotView`, skinned with the same Post-Baroque sprites already wired for the inventory grid (`UI_SlotsA_04` normal / `UI_SlotsA_01` selected). No new art assets.
- `HotbarUI`'s `slotContainer`/`slotTemplate` fields wired to these objects in-editor/via MCP.
- Always visible regardless of `PlayerController.IsMenuOpen` or any panel state (per requirement — matches Stardew/Minecraft convention). Iterate on layout/sizing by screenshot the same way the inventory skinning pass did.

## Testing

- `InventoryManager.SetActiveSlot` is a `MonoBehaviour` singleton method, so it follows the existing `PlayMode` integration-test convention (see `Assets/Tests/PlayMode/InventoryIntegrationTests.cs`, which uses `TestBootstrap.CreateSingleton<InventoryManager>()` + `GameEventsReset.ClearAll()`), not a bare EditMode test. Add cases to that file: range validation (`-1`, `9` are no-ops), no-op on setting the same index (no event fires), and `ActiveSlotChanged` firing with the new index on a real change.
- Manual Play Mode check: press 1-9, confirm the hotbar highlight moves and the full inventory screen's own selection is unaffected.
- All existing EditMode/PlayMode tests must stay green.

## Backlog addition

Add a new unchecked line to `docs/backlog.md` under UI:

```markdown
- [S] Hotbar use-action — pressing an action while an item is active in the hotbar should use/consume it (drop? tool use? TBD)
```
