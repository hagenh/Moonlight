# Inventory UI Design

## Overview

Add a toggleable inventory screen (press I) with a 5x4 grid of stack-limited slots, a detail sidebar, and world-pickup drops. Replace the current `Dictionary<ItemDef, int>` inventory model with a slotted model that enforces real capacity and stack limits.

## Domain Model

### InventorySlot

New sealed class in `Assets/Scripts/Rules/InventorySlot.cs`:

```
InventorySlot
  ItemDef Item        // null = empty slot; set by Inventory internally
  int Count           // 0 if empty; set by Inventory internally
  const int MaxStack = 30
  bool IsEmpty => Item == null
  bool IsFull => Count >= MaxStack
```

InventorySlot is mutable — the `Inventory` class manages mutations to Item and Count. External consumers read via `IReadOnlyList<InventorySlot>` and should not mutate slots directly.

### Inventory (Rewritten)

`Assets/Scripts/Rules/Inventory.cs` — replace `Dictionary<ItemDef, int>` with fixed-size slot array:

```
Inventory
  InventorySlot[] _slots  (20 slots, indexed 0-19)
  const int SlotCount = 20
  IReadOnlyList<InventorySlot> Slots => _slots

  int GetCount(ItemDef)              // sums across all slots with that item
  bool Has(ItemDef, int)             // checks total count
  AddResult TryAdd(ItemDef, int)     // fills partial stacks first, then empty slots
  bool TryRemove(ItemDef, int)       // removes from first matching slots (backward compat)
  bool TryDropFromSlot(int, int)     // removes from a specific slot by index
  int FirstEmptySlot()               // returns -1 if full
  Dictionary<ItemDef, int> GetAllItems()  // computed from slots
```

**AddResult** struct:
```
AddResult
  bool Success          // true if at least 1 was added
  int Added             // how many were actually added
  int Overflow          // how many couldn't fit (0 if all fit)
```

**TryAdd behavior:**
1. Find slots with the same ItemDef that aren't full — fill those first
2. If still remaining, find empty slots and fill them
3. Returns AddResult with how many were added and how many overflowed

**TryRemove behavior:**
1. Iterate slots from first to last
2. Remove from matching slots until count is satisfied
3. Empty slots (count reaches 0) have their ItemDef set to null
4. Returns false only if there aren't enough items total

**TryDropFromSlot behavior:**
1. Validate slotIndex is in range and slot is not empty
2. Clamp count to actual slot count
3. Record the ItemDef and actual count being dropped before mutation
4. Remove items, set slot empty if count reaches 0
5. Return a `DropResult(ItemDef def, int count, bool success)` struct so the caller knows what was dropped

**DropResult** struct:
```
DropResult
  ItemDef Def          // the item that was in the slot (null if failed)
  int Count            // how many were actually dropped
  bool Success         // true if anything was dropped
```

**MutResult** is replaced by **AddResult** for TryAdd and **DropResult** for TryDropFromSlot. TryRemove returns bool.

## Manager & Events

### InventoryManager Changes

`Assets/Scripts/InventoryManager.cs`:

- Internal `Inventory` swapped to slotted version
- Expose `IReadOnlyList<InventorySlot> Slots` for UI binding
- `TryAdd(def, count)` — calls `_inventory.TryAdd()`. Fires `OnInventoryChanged` for what was added. If overflow > 0, fires `OnInventoryFull(def, overflow)`. Returns true if at least 1 added.
- `TryRemove(def, count)` — unchanged API, delegates to new internal model
- New: `TryDropFromSlot(int slotIndex, int count)` — delegates to `_inventory.TryDropFromSlot()`, fires `OnItemDropped(slotIndex, def, count)` and `OnInventoryChanged`, returns `DropResult`
- `AllItems` property replaced by `GetAllItems()` method that computes dictionary from slots
- `GetCount(def)` and `Has(def, count)` preserved as convenience methods

### GameEvents Additions

```csharp
public static event System.Action InventoryOpened;
public static void OnInventoryOpened() => InventoryOpened?.Invoke();

public static event System.Action InventoryClosed;
public static void OnInventoryClosed() => InventoryClosed?.Invoke();

public static event System.Action<ItemDef, int> InventoryFull;
public static void OnInventoryFull(ItemDef def, int overflow) => InventoryFull?.Invoke(def, overflow);

public static event System.Action<int, ItemDef, int> ItemDropped;
public static void OnItemDropped(int slotIndex, ItemDef def, int count) => ItemDropped?.Invoke(slotIndex, def, count);
```

### AllItems Migration

Callers that currently use `AllItems` as `IReadOnlyDictionary<ItemDef, int>`:
- `DebugMenu.cs` — switch to `GetAllItems()`
- `GameHUD.cs` — switch to iterating `Slots` (shows slot-based counts)
- `InventoryManager` property — replace with `GetAllItems()` method

## InventoryUI

### Layout

`Assets/Scripts/UI/InventoryUI.cs` — IMGUI panel following existing pattern.

```
+--------------------------------------------------+
|  Inventory                              [X]      |
+----------------------------+---------------------+
|                            |                     |
|  [Grain×12] [Sugar×5]     |   Grain             |
|  [Yeast×8]  [Water×2]     |   Type: Ingredient  |
|  [    ]     [    ]         |   Price: 5g         |
|  [    ]     [    ]         |   Stack: 12/30      |
|  [    ]     [    ]         |                     |
|  [    ]     [    ]         |   Right-click slot  |
|  [    ]     [    ]         |   to drop           |
|  [    ]     [    ]         |                     |
+----------------------------+---------------------+
```

- Window: ~560×400, centered on screen
- Grid area: ~300px wide, 5 columns × 4 rows, 56×56 cells, 4px gap
- Detail sidebar: ~240px wide

### Grid Cell Rendering

Each cell:
- Empty slot: subtle bordered square (dark background, thin border)
- Occupied slot: item sprite (or fallback) + count badge (bottom-right, small font)
- Selected slot: highlighted border (bright color)
- Left-click: select slot, show details in sidebar
- Right-click: drop 1 item from that slot

### Sprite Rendering in IMGUI

- `ItemDef` gets an optional `Sprite icon` field (set in `ContentDb` registration)
- In `OnGUI`, convert sprite to `Texture2D` via `sprite.texture` + crop rect, draw with `GUI.DrawTexture`
- Fallback if no sprite: colored rectangle based on category (ingredients = brown, moonshine = amber) + first letter of `displayName` centered in the cell

### Detail Sidebar

When a slot is selected:
- Larger item icon (or fallback)
- Display name (bold)
- "Ingredient" or "Product" tag
- Base price in gold
- Stack count (e.g., "12/30")
- Hint text: "Right-click slot to drop"

When no slot selected:
- "Select an item" placeholder text

### Lifecycle (follows IMGUI pattern)

- `OnEnable`: subscribe to `GameEvents.InventoryOpened`, `GameEvents.InventoryChanged`, `GameEvents.MenuCloseRequested`
- `OnDisable`: unsubscribe (mirror of OnEnable)
- Open handler: set `_visible = true`, `PlayerController.Instance.IsMenuOpen = true`, center window, reset selection
- `Update()`: if visible and (Escape or I pressed), call `Close()`
- `Close()`: set `_visible = false`, `PlayerController.Instance.IsMenuOpen = false`, fire `GameEvents.OnInventoryClosed()`
- `OnGUI()`: if `!_visible` return; `GUI.Window(id, rect, DrawWindow, "Inventory")`
- `DrawWindow`: `GUI.DragWindow()` at top, then grid area + sidebar using `GUILayout`
- On `InventoryChanged` event: refresh displayed data (slots may have changed)

### Input Binding

- Add "Inventory" action to the `Menus` action map in the Input System asset, bound to **I** key
- `InventoryUI` subscribes to `Menus.Inventory` to toggle open/close
- Toggle behavior: if closed → open, if open → close

## DroppedItem

### Implementation

`Assets/Scripts/DroppedItem.cs` — implements `IInteractable`:

```
DroppedItem : MonoBehaviour, IInteractable
  ItemDef Item
  int Count
  InteractType InteractType => InteractType.DroppedItem
  bool CanInteract => true
  void Interact()
  static DroppedItem Create(ItemDef, int count, Vector3 position)
```

Requires adding `DroppedItem` to the `InteractType` enum in `IInteractable.cs`.

### Interact Logic

1. Call `InventoryManager.Instance.TryAdd(Item, Count)`
2. If all added → destroy the DroppedItem GameObject
3. If partial add (overflow > 0) → reduce `Count` by how many were added, keep DroppedItem alive
4. If nothing added → do nothing (inventory full)

### Create Factory

Static `Create(ItemDef, int count, Vector3 position)` method that programmatically builds:
- GameObject with name "DroppedItem_{item.id}"
- SpriteRenderer using item's sprite (or default fallback sprite)
- BoxCollider2D for interaction detection
- DroppedItem component with Item and Count set
- Position set to provided world position

### Drop Flow

1. Player right-clicks a slot in InventoryUI
2. InventoryUI calls `InventoryManager.Instance.TryDropFromSlot(slotIndex, 1)`
3. InventoryManager removes 1 from the slot, fires `OnItemDropped(slotIndex, item, 1)` and `OnInventoryChanged`
4. InventoryManager spawns `DroppedItem.Create(item, 1, playerPosition + offset)` internally (the manager owns the full drop lifecycle)
5. DroppedItem appears in the world near the player

### Visual

- Uses the item's sprite from `ItemDef.icon`
- Fallback: a small default "bag" or "bottle" sprite
- SpriteRenderer `sortingOrder` set so it appears on the ground plane
- Small BoxCollider2D sized for interaction (not physics)

## GameHUD Update

`Assets/Scripts/UI/GameHUD.cs`:

- Switch from iterating `AllItems` dictionary to iterating `InventoryManager.Instance.Slots`
- Show only non-empty slots: "DisplayName: Count"
- This aligns the HUD display with the slot-based model

## Test Strategy

### EditMode Tests

| Test Class | Coverage |
|-----------|----------|
| `InventorySlotTests` | MaxStack constant, IsEmpty, IsFull, default state |
| `InventoryTests` (rewritten) | TryAdd fills partial stacks first, TryAdd overflow, TryAdd no empty slots, TryRemove across multiple slots, TryDropFromSlot valid/invalid, GetCount sums correctly, Has checks total, FirstEmptySlot, GetAllItems computed correctly, capacity limit (20 slots), slot cleanup on empty |

### PlayMode Tests

| Test Class | Coverage |
|-----------|----------|
| `InventoryIntegrationTests` (updated) | InventoryChanged fired on add/remove/drop, InventoryFull fired on overflow, ItemDropped fired with correct slot/item/count, TryDropFromSlot returns correct data |
| `InventoryUITests` | Opens on InventoryOpened event, closes on Escape, closes on I key, right-click triggers drop flow |
| `DroppedItemTests` | Create factory produces valid GameObject, Interact adds to inventory, Interact partial pickup, Interact full inventory does nothing |

### Existing Test Migration

- `InventoryTests.cs` (7 tests) — rewrite against new slotted `Inventory` API, ensure same semantics (null guard, zero guard, stacking, auto-remove, Has)
- `InventoryIntegrationTests.cs` (5 tests) — should mostly pass as-is since `InventoryManager` API stays the same; update `AllItems` references to `GetAllItems()`
- `StartingInventoryTests.cs` (3 tests) — should pass unchanged

## File Changes Summary

| File | Change |
|------|--------|
| `Assets/Scripts/Rules/InventorySlot.cs` | **New** — InventorySlot sealed class |
| `Assets/Scripts/Rules/Inventory.cs` | **Rewrite** — slotted model, new AddResult, remove MutResult |
| `Assets/Scripts/InventoryManager.cs` | **Update** — slotted inventory, new TryDropFromSlot, GetAllItems(), Slots property |
| `Assets/Scripts/ItemDef.cs` | **Update** — add optional `Sprite icon` field |
| `Assets/Scripts/ContentDb.cs` | **Update** — assign sprites to item definitions |
| `Assets/Scripts/GameEvents.cs` | **Update** — add InventoryOpened, InventoryClosed, InventoryFull, ItemDropped events |
| `Assets/Scripts/IInteractable.cs` | **Update** — add `DroppedItem` to InteractType enum |
| `Assets/Scripts/UI/InventoryUI.cs` | **New** — IMGUI inventory panel |
| `Assets/Scripts/DroppedItem.cs` | **New** — IInteractable world pickup |
| `Assets/Scripts/UI/GameHUD.cs` | **Update** — iterate Slots instead of AllItems |
| `Assets/Scripts/DebugMenu.cs` | **Update** — use GetAllItems() |
| Input System asset | **Update** — add Inventory action to Menus map, bound to I |
| `Assets/Tests/EditMode/InventorySlotTests.cs` | **New** |
| `Assets/Tests/EditMode/InventoryTests.cs` | **Rewrite** |
| `Assets/Tests/PlayMode/InventoryIntegrationTests.cs` | **Update** |
| `Assets/Tests/PlayMode/InventoryUITests.cs` | **New** |
| `Assets/Tests/PlayMode/DroppedItemTests.cs` | **New** |
