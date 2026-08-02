# Inventory UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the flat dictionary inventory with a slotted model (20 slots, max stack 30) and add a toggleable IMGUI inventory screen with grid, detail sidebar, and world-pickup drops.

**Architecture:** Rewrite `Inventory` (pure C# Rules/) from `Dictionary<ItemDef, int>` to `InventorySlot[]`. Add `InventoryUI` as an IMGUI panel toggled by the I key. Add `DroppedItem` as an `IInteractable` world pickup. All cross-system communication via `GameEvents`.

**Tech Stack:** Unity 6, C#, IMGUI (OnGUI), Unity Input System, TextMeshPro (GameHUD only)

## Global Constraints

- No ScriptableObjects for game data — all content hardcoded in `ContentDb`
- No dependency injection — use singletons and events
- No direct cross-manager calls — use `GameEvents` for inter-system communication
- Rules/ must be pure C# — no `UnityEngine` types except `Mathf`
- UI is IMGUI with `OnGUI` — not uGUI Canvas or UI Toolkit
- No comments in code unless explicitly requested
- Test naming: `Method_Condition_Expected`
- EditMode tests for pure logic (Rules/), PlayMode for integration
- `TestBootstrap.CreateSingleton<T>()` for test singletons, `GameEventsReset.ClearAll()` between tests
- `EventRecorder` for verifying event sequences

---

### Task 1: InventorySlot + AddResult + DropResult (pure C#)

**Files:**
- Create: `Assets/Scripts/Rules/InventorySlot.cs`
- Test: `Assets/Tests/EditMode/InventorySlotTests.cs`

**Interfaces:**
- Consumes: `ItemDef` (existing class)
- Produces: `InventorySlot` class with `Item`, `Count`, `MaxStack`, `IsEmpty`, `IsFull`; `AddResult` struct with `Success`, `Added`, `Overflow`; `DropResult` struct with `Def`, `Count`, `Success`

- [ ] **Step 1: Write failing tests for InventorySlot, AddResult, DropResult**

```csharp
using NUnit.Framework;

public class InventorySlotTests
{
    [Test]
    public void DefaultSlot_IsEmpty()
    {
        var slot = new InventorySlot();
        Assert.IsNull(slot.Item);
        Assert.AreEqual(0, slot.Count);
        Assert.IsTrue(slot.IsEmpty);
        Assert.IsFalse(slot.IsFull);
    }

    [Test]
    public void MaxStack_Is30()
    {
        Assert.AreEqual(30, InventorySlot.MaxStack);
    }

    [Test]
    public void SlotWithItem_NotEmpty()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var slot = new InventorySlot { Item = item, Count = 5 };
        Assert.IsFalse(slot.IsEmpty);
        Assert.IsFalse(slot.IsFull);
    }

    [Test]
    public void SlotAtMaxStack_IsFull()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var slot = new InventorySlot { Item = item, Count = 30 };
        Assert.IsTrue(slot.IsFull);
    }

    [Test]
    public void AddResult_Defaults()
    {
        var r = new AddResult();
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(0, r.Overflow);
    }

    [Test]
    public void DropResult_Defaults()
    {
        var r = new DropResult();
        Assert.IsFalse(r.Success);
        Assert.IsNull(r.Def);
        Assert.AreEqual(0, r.Count);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run via Unity MCP or: `Unity.exe -runTests -testPlatform EditMode -projectPath .`
Expected: FAIL — `InventorySlot`, `AddResult`, `DropResult` not defined

- [ ] **Step 3: Write InventorySlot, AddResult, DropResult implementation**

```csharp
public sealed class InventorySlot
{
    public ItemDef Item;
    public int Count;
    public const int MaxStack = 30;
    public bool IsEmpty => Item == null;
    public bool IsFull => Count >= MaxStack;
}

public readonly struct AddResult
{
    public readonly bool Success;
    public readonly int Added;
    public readonly int Overflow;

    public AddResult(bool success, int added, int overflow)
    {
        Success = success;
        Added = added;
        Overflow = overflow;
    }
}

public readonly struct DropResult
{
    public readonly bool Success;
    public readonly ItemDef Def;
    public readonly int Count;

    public DropResult(bool success, ItemDef def, int count)
    {
        Success = success;
        Def = def;
        Count = count;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All 6 tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/InventorySlot.cs Assets/Tests/EditMode/InventorySlotTests.cs
git commit -m "feat: add InventorySlot, AddResult, DropResult types"
```

---

### Task 2: Rewrite Inventory with slotted model

**Files:**
- Rewrite: `Assets/Scripts/Rules/Inventory.cs`
- Rewrite: `Assets/Tests/EditMode/InventoryTests.cs`

**Interfaces:**
- Consumes: `InventorySlot`, `AddResult`, `DropResult` (from Task 1), `ItemDef` (existing)
- Produces: `Inventory` class with `Slots`, `SlotCount`, `GetCount(ItemDef)`, `Has(ItemDef, int)`, `TryAdd(ItemDef, int)` → `AddResult`, `TryRemove(ItemDef, int)` → `bool`, `TryDropFromSlot(int, int)` → `DropResult`, `FirstEmptySlot()`, `GetAllItems()`

- [ ] **Step 1: Write failing tests for slotted Inventory**

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class InventoryTests
{
    private Inventory _inventory;
    private ItemDef _grain;
    private ItemDef _sugar;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Inventory();
        _grain = new ItemDef("grain", "Grain", true, 5);
        _sugar = new ItemDef("sugar", "Sugar", true, 5);
    }

    [Test]
    public void TryAdd_Null_ReturnsFailure()
    {
        var r = _inventory.TryAdd(null, 5);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
    }

    [Test]
    public void TryAdd_ZeroOrNegative_ReturnsFailure()
    {
        var r1 = _inventory.TryAdd(_grain, 0);
        var r2 = _inventory.TryAdd(_grain, -3);
        Assert.IsFalse(r1.Success);
        Assert.IsFalse(r2.Success);
    }

    [Test]
    public void TryAdd_FillsEmptySlot()
    {
        var r = _inventory.TryAdd(_grain, 5);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.Added);
        Assert.AreEqual(0, r.Overflow);
        Assert.AreEqual(5, _inventory.GetCount(_grain));
    }

    [Test]
    public void TryAdd_StacksOntoExistingPartialSlot()
    {
        _inventory.TryAdd(_grain, 25);
        var r = _inventory.TryAdd(_grain, 10);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.Added);
        Assert.AreEqual(5, r.Overflow);
        Assert.AreEqual(30, _inventory.GetCount(_grain));
    }

    [Test]
    public void TryAdd_FillsMultipleSlots()
    {
        var r = _inventory.TryAdd(_grain, 45);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(45, r.Added);
        Assert.AreEqual(0, r.Overflow);
        Assert.AreEqual(45, _inventory.GetCount(_grain));
        Assert.AreEqual(_grain, _inventory.Slots[0].Item);
        Assert.AreEqual(30, _inventory.Slots[0].Count);
        Assert.AreEqual(_grain, _inventory.Slots[1].Item);
        Assert.AreEqual(15, _inventory.Slots[1].Count);
    }

    [Test]
    public void TryAdd_OverflowWhenAllSlotsFull()
    {
        _inventory.TryAdd(_grain, 30);
        for (int i = 0; i < 19; i++)
            _inventory.TryAdd(_sugar, 30);

        var r = _inventory.TryAdd(_grain, 5);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(5, r.Overflow);
    }

    [Test]
    public void TryAdd_CompletelyFull_NoRoomAtAll()
    {
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(_grain, 30);

        var r = _inventory.TryAdd(_sugar, 1);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(1, r.Overflow);
    }

    [Test]
    public void TryRemove_Insufficient_ReturnsFalse()
    {
        _inventory.TryAdd(_grain, 3);
        bool result = _inventory.TryRemove(_grain, 5);
        Assert.IsFalse(result);
        Assert.AreEqual(3, _inventory.GetCount(_grain));
    }

    [Test]
    public void TryRemove_AcrossMultipleSlots()
    {
        _inventory.TryAdd(_grain, 45);
        bool result = _inventory.TryRemove(_grain, 35);
        Assert.IsTrue(result);
        Assert.AreEqual(10, _inventory.GetCount(_grain));
        Assert.AreEqual(10, _inventory.Slots[0].Count);
    }

    [Test]
    public void TryRemove_ToZero_ClearsSlot()
    {
        _inventory.TryAdd(_grain, 5);
        _inventory.TryRemove(_grain, 5);
        Assert.AreEqual(0, _inventory.GetCount(_grain));
        Assert.IsNull(_inventory.Slots[0].Item);
    }

    [Test]
    public void TryRemove_Null_ReturnsFalse()
    {
        bool result = _inventory.TryRemove(null, 5);
        Assert.IsFalse(result);
    }

    [Test]
    public void TryDropFromSlot_Valid_RemovesAndReturnsResult()
    {
        _inventory.TryAdd(_grain, 10);
        var r = _inventory.TryDropFromSlot(0, 3);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(_grain, r.Def);
        Assert.AreEqual(3, r.Count);
        Assert.AreEqual(7, _inventory.Slots[0].Count);
    }

    [Test]
    public void TryDropFromSlot_DropAll_ClearsSlot()
    {
        _inventory.TryAdd(_grain, 5);
        var r = _inventory.TryDropFromSlot(0, 5);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.Count);
        Assert.IsNull(_inventory.Slots[0].Item);
    }

    [Test]
    public void TryDropFromSlot_InvalidIndex_ReturnsFailure()
    {
        var r = _inventory.TryDropFromSlot(-1, 1);
        Assert.IsFalse(r.Success);
        var r2 = _inventory.TryDropFromSlot(20, 1);
        Assert.IsFalse(r2.Success);
    }

    [Test]
    public void TryDropFromSlot_EmptySlot_ReturnsFailure()
    {
        var r = _inventory.TryDropFromSlot(0, 1);
        Assert.IsFalse(r.Success);
    }

    [Test]
    public void GetCount_NullDef_ReturnsZero()
    {
        Assert.AreEqual(0, _inventory.GetCount(null));
    }

    [Test]
    public void Has_RespectsCount()
    {
        _inventory.TryAdd(_grain, 3);
        Assert.IsTrue(_inventory.Has(_grain, 3));
        Assert.IsFalse(_inventory.Has(_grain, 4));
    }

    [Test]
    public void FirstEmptySlot_ReturnsFirstEmpty()
    {
        Assert.AreEqual(0, _inventory.FirstEmptySlot());
        _inventory.TryAdd(_grain, 30);
        Assert.AreEqual(1, _inventory.FirstEmptySlot());
    }

    [Test]
    public void FirstEmptySlot_Full_ReturnsMinusOne()
    {
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(_grain, 30);
        Assert.AreEqual(-1, _inventory.FirstEmptySlot());
    }

    [Test]
    public void GetAllItems_ComputesFromSlots()
    {
        _inventory.TryAdd(_grain, 10);
        _inventory.TryAdd(_sugar, 5);
        var all = _inventory.GetAllItems();
        Assert.AreEqual(10, all[_grain]);
        Assert.AreEqual(5, all[_sugar]);
        Assert.AreEqual(2, all.Count);
    }

    [Test]
    public void SlotCount_Is20()
    {
        Assert.AreEqual(20, Inventory.SlotCount);
        Assert.AreEqual(20, _inventory.Slots.Count);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Expected: FAIL — old Inventory API doesn't match new test signatures

- [ ] **Step 3: Write slotted Inventory implementation**

```csharp
using System.Collections.Generic;

public sealed class Inventory
{
    private readonly InventorySlot[] _slots;
    public const int SlotCount = 20;

    public Inventory()
    {
        _slots = new InventorySlot[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            _slots[i] = new InventorySlot();
    }

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public int GetCount(ItemDef def)
    {
        if (def == null) return 0;
        int total = 0;
        for (int i = 0; i < SlotCount; i++)
            if (_slots[i].Item == def)
                total += _slots[i].Count;
        return total;
    }

    public bool Has(ItemDef def, int count)
    {
        return GetCount(def) >= count;
    }

    public AddResult TryAdd(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return new AddResult(false, 0, 0);

        int remaining = count;

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].Item == def && !_slots[i].IsFull)
            {
                int space = InventorySlot.MaxStack - _slots[i].Count;
                int add = System.Math.Min(space, remaining);
                _slots[i].Count += add;
                remaining -= add;
            }
        }

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty)
            {
                int add = System.Math.Min(InventorySlot.MaxStack, remaining);
                _slots[i].Item = def;
                _slots[i].Count = add;
                remaining -= add;
            }
        }

        int added = count - remaining;
        return new AddResult(added > 0, added, remaining);
    }

    public bool TryRemove(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return false;

        if (GetCount(def) < count)
            return false;

        int remaining = count;

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].Item == def)
            {
                int remove = System.Math.Min(_slots[i].Count, remaining);
                _slots[i].Count -= remove;
                remaining -= remove;

                if (_slots[i].Count <= 0)
                {
                    _slots[i].Item = null;
                    _slots[i].Count = 0;
                }
            }
        }

        return remaining == 0;
    }

    public DropResult TryDropFromSlot(int slotIndex, int count)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
            return new DropResult(false, null, 0);

        var slot = _slots[slotIndex];
        if (slot.IsEmpty || count <= 0)
            return new DropResult(false, null, 0);

        int actual = System.Math.Min(count, slot.Count);
        ItemDef dropped = slot.Item;
        slot.Count -= actual;

        if (slot.Count <= 0)
        {
            slot.Item = null;
            slot.Count = 0;
        }

        return new DropResult(true, dropped, actual);
    }

    public int FirstEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
            if (_slots[i].IsEmpty)
                return i;
        return -1;
    }

    public Dictionary<ItemDef, int> GetAllItems()
    {
        var result = new Dictionary<ItemDef, int>();
        for (int i = 0; i < SlotCount; i++)
        {
            if (!_slots[i].IsEmpty)
            {
                if (result.ContainsKey(_slots[i].Item))
                    result[_slots[i].Item] += _slots[i].Count;
                else
                    result[_slots[i].Item] = _slots[i].Count;
            }
        }
        return result;
    }
}
```

Note: `MutResult` is removed. The old `AllItems` property is replaced by `GetAllItems()` method.

- [ ] **Step 4: Run tests to verify they pass**

Expected: All tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/Inventory.cs Assets/Tests/EditMode/InventoryTests.cs
git commit -m "feat: rewrite Inventory with slotted model (20 slots, max stack 30)"
```

---

### Task 3: Update InventoryManager for slotted model

**Files:**
- Modify: `Assets/Scripts/InventoryManager.cs`
- Modify: `Assets/Tests/PlayMode/InventoryIntegrationTests.cs`

**Interfaces:**
- Consumes: `Inventory` (Task 2), `GameEvents` (existing)
- Produces: `InventoryManager` with `Slots`, `TryDropFromSlot`, `GetAllItems()`; fires `OnInventoryFull`, `OnItemDropped`

- [ ] **Step 1: Update GameEvents with new inventory events**

Add to `Assets/Scripts/GameEvents.cs`, after the existing `OnRecipeBookRequested` method:

```csharp
public static event System.Action InventoryOpened;
public static event System.Action InventoryClosed;
public static event System.Action<ItemDef, int> InventoryFull;
public static event System.Action<int, ItemDef, int> ItemDropped;

public static void OnInventoryOpened() => InventoryOpened?.Invoke();
public static void OnInventoryClosed() => InventoryClosed?.Invoke();
public static void OnInventoryFull(ItemDef def, int overflow) => InventoryFull?.Invoke(def, overflow);
public static void OnItemDropped(int slotIndex, ItemDef def, int count) => ItemDropped?.Invoke(slotIndex, def, count);
```

- [ ] **Step 2: Rewrite InventoryManager**

```csharp
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Inventory _inventory = new();

    public IReadOnlyList<InventorySlot> Slots => _inventory.Slots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.Day == 1)
            TryAdd(ContentDb.Berry, 3);
    }

    public int GetCount(ItemDef def)
    {
        return _inventory.GetCount(def);
    }

    public bool Has(ItemDef def, int count)
    {
        return _inventory.Has(def, count);
    }

    public bool TryAdd(ItemDef def, int count)
    {
        var r = _inventory.TryAdd(def, count);
        if (!r.Success) return false;

        int oldCount = GetCount(def) - r.Added;
        GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
        GameEvents.OnToastRequested($"+{r.Added} {def.displayName}");

        if (r.Overflow > 0)
            GameEvents.OnInventoryFull(def, r.Overflow);

        return true;
    }

    public bool TryRemove(ItemDef def, int count)
    {
        if (def == null || count <= 0) return false;
        int current = GetCount(def);
        if (current < count)
        {
            GameEvents.OnToastRequested($"Not enough {def.displayName}");
            return false;
        }
        int oldCount = current;
        _inventory.TryRemove(def, count);
        GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
        return true;
    }

    public DropResult TryDropFromSlot(int slotIndex, int count)
    {
        var r = _inventory.TryDropFromSlot(slotIndex, count);
        if (r.Success)
        {
            GameEvents.OnItemDropped(slotIndex, r.Def, r.Count);
            GameEvents.OnInventoryChanged(r.Def, GetCount(r.Def) + r.Count, GetCount(r.Def));
        }
        return r;
    }

    public Dictionary<ItemDef, int> GetAllItems()
    {
        return _inventory.GetAllItems();
    }
}
```

- [ ] **Step 3: Update InventoryIntegrationTests for new API**

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class InventoryIntegrationTests
{
    private InventoryManager _inventory;
    private EventRecorder _recorder;
    private ItemDef _grain;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _recorder = new EventRecorder();
        _grain = new ItemDef("grain", "Grain", true, 5);

        GameEvents.InventoryChanged += (def, oldCount, newCount) =>
            _recorder.Record("InventoryChanged", $"{oldCount}->{newCount}");
        GameEvents.ToastRequested += (msg) => _recorder.Record("Toast", msg);
        GameEvents.InventoryFull += (def, overflow) =>
            _recorder.Record("InventoryFull", $"{def.displayName}:{overflow}");
        GameEvents.ItemDropped += (idx, def, cnt) =>
            _recorder.Record("ItemDropped", $"{idx}:{def.displayName}:{cnt}");
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator TryAdd_FiresInventoryChangedAndToast()
    {
        bool result = _inventory.TryAdd(_grain, 5);

        Assert.IsTrue(result);
        Assert.AreEqual(5, _inventory.GetCount(_grain));
        Assert.AreEqual(2, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("InventoryChanged"));
        Assert.IsTrue(_recorder.Order[1].StartsWith("Toast"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryRemove_FiresInventoryChangedOnly()
    {
        _inventory.TryAdd(_grain, 5);
        _recorder.Clear();

        bool result = _inventory.TryRemove(_grain, 3);

        Assert.IsTrue(result);
        Assert.AreEqual(2, _inventory.GetCount(_grain));
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("InventoryChanged"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryRemove_Insufficient_FiresNotEnoughToast()
    {
        _inventory.TryAdd(_grain, 2);
        _recorder.Clear();

        bool result = _inventory.TryRemove(_grain, 5);

        Assert.IsFalse(result);
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].Contains("Not enough"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryRemove_ToZero_CountIsZero()
    {
        _inventory.TryAdd(_grain, 3);
        _recorder.Clear();

        _inventory.TryRemove(_grain, 3);

        Assert.AreEqual(0, _inventory.GetCount(_grain));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryAdd_Null_ReturnsFalse_NoEvents()
    {
        bool result = _inventory.TryAdd(null, 5);

        Assert.IsFalse(result);
        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryDropFromSlot_FiresItemDroppedAndInventoryChanged()
    {
        _inventory.TryAdd(_grain, 10);
        _recorder.Clear();

        var r = _inventory.TryDropFromSlot(0, 3);

        Assert.IsTrue(r.Success);
        Assert.AreEqual(_grain, r.Def);
        Assert.AreEqual(3, r.Count);
        Assert.AreEqual(2, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("ItemDropped"));
        Assert.IsTrue(_recorder.Order[1].StartsWith("InventoryChanged"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryAdd_Overflow_FiresInventoryFull()
    {
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(_grain, 30);
        _recorder.Clear();

        bool result = _inventory.TryAdd(_grain, 5);

        Assert.IsFalse(result);
        Assert.IsTrue(_recorder.Has("InventoryFull"));
        yield return null;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All integration tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/InventoryManager.cs Assets/Scripts/GameEvents.cs Assets/Tests/PlayMode/InventoryIntegrationTests.cs
git commit -m "feat: update InventoryManager for slotted model with drop and overflow events"
```

---

### Task 4: Migrate AllItems callers

**Files:**
- Modify: `Assets/Scripts/UI/GameHUD.cs` (line 150 — `AllItems` usage)
- Modify: `Assets/Scripts/DebugMenu.cs` (line 182 — `AllItems` usage)

**Interfaces:**
- Consumes: `InventoryManager.GetAllItems()`, `InventoryManager.Slots` (from Task 3)
- Produces: No new public APIs — just fixes compilation errors

- [ ] **Step 1: Update GameHUD.UpdateInventoryDisplay()**

Replace the `AllItems` iteration at `GameHUD.cs:146-153`:

```csharp
private void UpdateInventoryDisplay()
{
    if (inventoryText == null || InventoryManager.Instance == null) return;
    var sb = new System.Text.StringBuilder();
    foreach (var slot in InventoryManager.Instance.Slots)
    {
        if (!slot.IsEmpty)
            sb.AppendLine($"{slot.Item.displayName}: {slot.Count}");
    }
    inventoryText.text = sb.ToString();
}
```

- [ ] **Step 2: Update DebugMenu stock display**

Replace the `AllItems` iteration at `DebugMenu.cs:180-183`:

```csharp
if (InventoryManager.Instance != null)
{
    foreach (var kvp in InventoryManager.Instance.GetAllItems())
        GUILayout.Label($"  {kvp.Key.displayName}: {kvp.Value}");
}
```

- [ ] **Step 3: Run tests and verify compilation**

Expected: All existing tests still pass, no compilation errors

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/GameHUD.cs Assets/Scripts/DebugMenu.cs
git commit -m "fix: migrate AllItems callers to slotted inventory API"
```

---

### Task 5: Add icon field to ItemDef + DroppedItem enum value

**Files:**
- Modify: `Assets/Scripts/ItemDef.cs`
- Modify: `Assets/Scripts/IInteractable.cs`
- Modify: `Assets/Scripts/ContentDb.cs`

**Interfaces:**
- Consumes: existing `ItemDef`, `InteractType`
- Produces: `ItemDef.icon` (optional Sprite), `InteractType.DroppedItem`

- [ ] **Step 1: Add icon field to ItemDef**

Add `using UnityEngine;` at the top of `ItemDef.cs` and add the field:

```csharp
using UnityEngine;

public class ItemDef
{
    public string id;
    public string displayName;
    public bool isIngredient = true;
    public int basePrice;
    public bool isBottle;
    public Sprite icon;

    public ItemDef(string id, string displayName, bool isIngredient = true, int basePrice = 0, bool isBottle = false, Sprite icon = null)
    {
        this.id = id;
        this.displayName = displayName;
        this.isIngredient = isIngredient;
        this.basePrice = basePrice;
        this.isBottle = isBottle;
        this.icon = icon;
    }
}
```

- [ ] **Step 2: Add DroppedItem to InteractType enum**

Add to `IInteractable.cs` after `Stand`:

```csharp
public enum InteractType
{
    Building,
    FermentVat,
    Seller,
    Bed,
    Debris,
    DebrisPile,
    NPC,
    ExitDoor,
    Crate,
    DeliveryPoint,
    Forage,
    Stand,
    DroppedItem
}
```

- [ ] **Step 3: Verify compilation**

Expected: No errors — `icon` defaults to null, existing callers unchanged

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/ItemDef.cs Assets/Scripts/IInteractable.cs
git commit -m "feat: add icon field to ItemDef and DroppedItem to InteractType"
```

---

### Task 6: DroppedItem interactable

**Files:**
- Create: `Assets/Scripts/DroppedItem.cs`
- Create: `Assets/Tests/PlayMode/DroppedItemTests.cs`

**Interfaces:**
- Consumes: `IInteractable`, `InteractType.DroppedItem`, `InventoryManager`, `ItemDef.icon` (from Tasks 3, 5)
- Produces: `DroppedItem` class with `Create()` factory, `Interact()`, `Item`, `Count`

- [ ] **Step 1: Write DroppedItem implementation**

```csharp
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    public ItemDef Item { get; private set; }
    public int Count { get; private set; }

    public InteractType InteractType => InteractType.DroppedItem;
    public bool CanInteract => Item != null && Count > 0;

    public void Interact()
    {
        if (!CanInteract || InventoryManager.Instance == null) return;

        var r = InventoryManager.Instance.TryAdd(Item, Count);
        if (r)
        {
            Count = 0;
            Item = null;
            Destroy(gameObject);
        }
    }

    public static DroppedItem Create(ItemDef item, int count, Vector3 position)
    {
        var go = new GameObject($"DroppedItem_{item.id}");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        if (item.icon != null)
            sr.sprite = item.icon;
        sr.sortingOrder = -1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        var di = go.AddComponent<DroppedItem>();
        di.Item = item;
        di.Count = count;

        return di;
    }
}
```

Note: `TryAdd` returns bool from InventoryManager. Partial adds are handled — if `TryAdd` succeeds (at least 1 added), the DroppedItem is fully consumed. For partial pickup support, we'd need to check the `AddResult.Overflow` from the internal call, but since `InventoryManager.TryAdd` returns bool, the simplest V1 approach is: full pickup succeeds, full inventory fails. This can be enhanced later.

Actually, we need partial pickup. Let me adjust:

```csharp
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    public ItemDef Item { get; private set; }
    public int Count { get; private set; }

    public InteractType InteractType => InteractType.DroppedItem;
    public bool CanInteract => Item != null && Count > 0;

    public void Interact()
    {
        if (!CanInteract || InventoryManager.Instance == null) return;

        int oldCount = Count;
        var r = InventoryManager.Instance.TryAddPartial(Item, Count);
        Count -= r.Added;

        if (Count <= 0)
            Destroy(gameObject);
    }

    public static DroppedItem Create(ItemDef item, int count, Vector3 position)
    {
        var go = new GameObject($"DroppedItem_{item.id}");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        if (item.icon != null)
            sr.sprite = item.icon;
        sr.sortingOrder = -1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        var di = go.AddComponent<DroppedItem>();
        di.Item = item;
        di.Count = count;

        return di;
    }
}
```

Wait — this requires `InventoryManager.TryAddPartial` which returns `AddResult`. Let me add that to InventoryManager.

- [ ] **Step 2: Add TryAddPartial to InventoryManager**

Add this method to `InventoryManager.cs`:

```csharp
public AddResult TryAddPartial(ItemDef def, int count)
{
    var r = _inventory.TryAdd(def, count);
    if (r.Added > 0)
    {
        int oldCount = GetCount(def) - r.Added;
        GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
        GameEvents.OnToastRequested($"+{r.Added} {def.displayName}");
    }
    if (r.Overflow > 0)
        GameEvents.OnInventoryFull(def, r.Overflow);
    return r;
}
```

Also update `DroppedItem.Interact()` to use `TryAddPartial`:

```csharp
public void Interact()
{
    if (!CanInteract || InventoryManager.Instance == null) return;

    var r = InventoryManager.Instance.TryAddPartial(Item, Count);
    Count -= r.Added;

    if (Count <= 0)
        Destroy(gameObject);
}
```

- [ ] **Step 3: Write PlayMode tests for DroppedItem**

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DroppedItemTests
{
    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        TestBootstrap.CreateSingleton<InventoryManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator Create_ProducesValidGameObject()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 3, Vector3.zero);

        Assert.IsNotNull(di);
        Assert.AreEqual(item, di.Item);
        Assert.AreEqual(3, di.Count);
        Assert.IsTrue(di.CanInteract);
        yield return null;

        TestBootstrap.DestroyAll();
    }

    [UnityTest]
    public IEnumerator Interact_AddsToInventory()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 3, Vector3.zero);

        di.Interact();

        Assert.AreEqual(3, InventoryManager.Instance.GetCount(item));
        Assert.IsNull(di.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Interact_PartialPickup_KeepsDroppedItemAlive()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        for (int i = 0; i < 19; i++)
            InventoryManager.Instance.TryAdd(item, 30);
        InventoryManager.Instance.TryAdd(item, 28);

        var di = DroppedItem.Create(item, 5, Vector3.zero);
        di.Interact();

        Assert.AreEqual(2, di.Count);
        Assert.IsNotNull(di.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Interact_FullInventory_DoesNothing()
    {
        var grain = new ItemDef("grain", "Grain", true, 5);
        var sugar = new ItemDef("sugar", "Sugar", true, 5);
        for (int i = 0; i < 20; i++)
            InventoryManager.Instance.TryAdd(grain, 30);

        var di = DroppedItem.Create(sugar, 5, Vector3.zero);
        di.Interact();

        Assert.AreEqual(0, InventoryManager.Instance.GetCount(sugar));
        Assert.AreEqual(5, di.Count);
        Assert.IsTrue(di.CanInteract);
        yield return null;
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Expected: All DroppedItem tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/DroppedItem.cs Assets/Scripts/InventoryManager.cs Assets/Tests/PlayMode/DroppedItemTests.cs
git commit -m "feat: add DroppedItem interactable with partial pickup support"
```

---

### Task 7: Wire drop spawn into InventoryManager

**Files:**
- Modify: `Assets/Scripts/InventoryManager.cs` — add DroppedItem spawning on TryDropFromSlot

**Interfaces:**
- Consumes: `DroppedItem.Create()` (Task 6), `PlayerController.Instance` (existing)
- Produces: `TryDropFromSlot` now spawns a world DroppedItem automatically

- [ ] **Step 1: Update InventoryManager.TryDropFromSlot to spawn DroppedItem**

Replace the existing `TryDropFromSlot` method:

```csharp
public DropResult TryDropFromSlot(int slotIndex, int count)
{
    var r = _inventory.TryDropFromSlot(slotIndex, count);
    if (r.Success)
    {
        GameEvents.OnItemDropped(slotIndex, r.Def, r.Count);
        GameEvents.OnInventoryChanged(r.Def, GetCount(r.Def) + r.Count, GetCount(r.Def));

        Vector3 spawnPos = PlayerController.Instance != null
            ? PlayerController.Instance.transform.position + new Vector3(0.5f, 0f, 0f)
            : Vector3.zero;
        DroppedItem.Create(r.Def, r.Count, spawnPos);
    }
    return r;
}
```

- [ ] **Step 2: Verify existing tests still pass**

Expected: All tests PASS — existing tests don't check for spawned objects, just the return value and events

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/InventoryManager.cs
git commit -m "feat: spawn DroppedItem on TryDropFromSlot"
```

---

### Task 8: InventoryUI — IMGUI panel with grid and sidebar

**Files:**
- Create: `Assets/Scripts/UI/InventoryUI.cs`

**Interfaces:**
- Consumes: `InventoryManager.Slots`, `GameEvents.InventoryOpened/InventoryClosed/InventoryChanged/MenuCloseRequested`, `PlayerController.IsMenuOpen`, `ItemDef.icon` (Task 5), `InventoryManager.TryDropFromSlot` (Task 7)
- Produces: `InventoryUI` MonoBehaviour — the inventory screen

- [ ] **Step 1: Write InventoryUI implementation**

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(0, 0, 560, 400);
    private int _selectedSlot = -1;
    private InputSystem_Actions _input;

    private const int GridCols = 5;
    private const int GridRows = 4;
    private const int CellSize = 56;
    private const int CellGap = 4;
    private const int GridWidth = GridCols * CellSize + (GridCols - 1) * CellGap;
    private const int SidebarWidth = 220;
    private const int SidebarX = GridWidth + 20;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        GameEvents.InventoryOpened += OnInventoryOpened;
        GameEvents.InventoryChanged += OnInventoryChanged;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;

        _input.Menus.Enable();
        _input.Menus.RecipeBook.performed += OnInventoryKey;
    }

    private void OnDisable()
    {
        GameEvents.InventoryOpened -= OnInventoryOpened;
        GameEvents.InventoryChanged -= OnInventoryChanged;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;

        _input.Menus.RecipeBook.performed -= OnInventoryKey;
        _input.Menus.Disable();
    }

    private void OnInventoryKey(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (_visible) Close();
        else Open();
    }

    private void OnInventoryOpened()
    {
        Open();
    }

    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount)
    {
    }

    private void OnMenuCloseRequested()
    {
        Close();
    }

    private void Open()
    {
        if (_visible) return;
        _visible = true;
        _selectedSlot = -1;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void Close()
    {
        if (!_visible) return;
        _visible = false;
        _selectedSlot = -1;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
        GameEvents.OnInventoryClosed();
    }

    private void Update()
    {
        if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUI.Window(3, _windowRect, DrawWindow, "Inventory");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (InventoryManager.Instance == null) return;

        DrawGrid();
        DrawSidebar();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }

    private void DrawGrid()
    {
        float startX = 10;
        float startY = 30;

        for (int row = 0; row < GridRows; row++)
        {
            for (int col = 0; col < GridCols; col++)
            {
                int idx = row * GridCols + col;
                float x = startX + col * (CellSize + CellGap);
                float y = startY + row * (CellSize + CellGap);
                var rect = new Rect(x, y, CellSize, CellSize);

                var slot = InventoryManager.Instance.Slots[idx];
                bool isSelected = idx == _selectedSlot;

                Color prevBg = GUI.backgroundColor;
                if (isSelected)
                    GUI.backgroundColor = new Color(1f, 0.9f, 0.4f);
                else if (slot.IsEmpty)
                    GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                else
                    GUI.backgroundColor = new Color(0.35f, 0.3f, 0.25f);

                GUI.Box(rect, "");
                GUI.backgroundColor = prevBg;

                if (!slot.IsEmpty)
                {
                    DrawSlotContent(rect, slot);
                }

                HandleSlotInput(rect, idx, slot);
            }
        }
    }

    private void DrawSlotContent(Rect rect, InventorySlot slot)
    {
        if (slot.Item.icon != null)
        {
            var sprite = slot.Item.icon;
            var tex = sprite.texture;
            var cr = sprite.textureRect;
            var uvRect = new Rect(cr.x / tex.width, cr.y / tex.height, cr.width / tex.width, cr.height / tex.height);
            GUI.DrawTextureWithTexCoords(rect, tex, uvRect);
        }
        else
        {
            Color prev = GUI.color;
            GUI.color = slot.Item.isBottle ? new Color(0.8f, 0.6f, 0.2f) : new Color(0.6f, 0.4f, 0.2f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prev;

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(rect, slot.Item.displayName[0].ToString(), labelStyle);
        }

        if (slot.Count > 1)
        {
            var countStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 12
            };
            var countRect = new Rect(rect.x + rect.width - 28, rect.y + rect.height - 18, 26, 16);
            GUI.Label(countRect, slot.Count.ToString(), countStyle);
        }
    }

    private void HandleSlotInput(Rect rect, int idx, InventorySlot slot)
    {
        var e = Event.current;
        if (rect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    _selectedSlot = slot.IsEmpty ? -1 : idx;
                    e.Use();
                }
                else if (e.button == 1 && !slot.IsEmpty)
                {
                    InventoryManager.Instance.TryDropFromSlot(idx, 1);
                    if (_selectedSlot == idx && slot.Count <= 1)
                        _selectedSlot = -1;
                    e.Use();
                }
            }
        }
    }

    private void DrawSidebar()
    {
        float x = SidebarX + 10;
        float y = 30;

        if (_selectedSlot < 0 || InventoryManager.Instance.Slots[_selectedSlot].IsEmpty)
        {
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x, y, SidebarWidth - 10, 200), "Select an item", style);
            return;
        }

        var slot = InventoryManager.Instance.Slots[_selectedSlot];
        var item = slot.Item;
        float curY = y;

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 24), item.displayName, titleStyle);
        curY += 28;

        var tag = item.isIngredient ? "Ingredient" : "Product";
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), tag);
        curY += 22;

        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), $"Base Price: {item.basePrice}g");
        curY += 22;

        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), $"Stack: {slot.Count}/{InventorySlot.MaxStack}");
        curY += 30;

        var hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Italic };
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 40), "Right-click slot\nto drop", hintStyle);
    }
}
```

Note: The Input System asset doesn't have an "Inventory" action yet. For now, this uses `RecipeBook` action as a placeholder. Task 9 will add the proper "Inventory" action to the Input System asset. If the Input System asset can't be modified programmatically, the I key binding will need to be added manually in the Unity editor.

- [ ] **Step 2: Add InventoryUI to the scene**

This step requires Unity editor — add an empty GameObject with the `InventoryUI` component to the scene. For now, the code compiles and the component can be added manually.

- [ ] **Step 3: Verify compilation**

Expected: No compilation errors

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/InventoryUI.cs
git commit -m "feat: add InventoryUI IMGUI panel with grid, sidebar, and drop"
```

---

### Task 9: Add Inventory input action

**Files:**
- Modify: Input System asset (add Inventory action to Menus action map, bound to I key)

**Interfaces:**
- Consumes: Input System asset
- Produces: `Menus.Inventory` action bound to I key

This task requires the Unity Editor. Steps:

- [ ] **Step 1: In Unity Editor, open the Input System asset**

- [ ] **Step 2: Add "Inventory" action to the "Menus" action map**

- Binding: `<Keyboard>/i`
- Action type: Button

- [ ] **Step 3: Update InventoryUI to use the new action**

Replace `_input.Menus.RecipeBook` references in `InventoryUI.cs` with `_input.Menus.Inventory`:

In `OnEnable`:
```csharp
_input.Menus.Inventory.performed += OnInventoryKey;
```

In `OnDisable`:
```csharp
_input.Menus.Inventory.performed -= OnInventoryKey;
```

- [ ] **Step 4: Test in Play Mode**

Press I → inventory opens. Press I again or Escape → closes. Right-click a slot → drops 1 item.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/InventoryUI.cs
git commit -m "feat: wire Inventory input action (I key) to InventoryUI"
```

---

### Task 10: Update StartingInventoryTests

**Files:**
- Modify: `Assets/Tests/PlayMode/StartingInventoryTests.cs`

**Interfaces:**
- Consumes: `InventoryManager.GetCount()` (unchanged API from Task 3)
- Produces: Tests pass

- [ ] **Step 1: Read current StartingInventoryTests and verify compatibility**

The existing tests use `InventoryManager.Instance.GetCount(ContentDb.Berry)` and `InventoryManager.Instance.AllItems.ContainsKey(ContentDb.Berry)`. The `AllItems` property no longer exists — update to use `GetAllItems()`.

- [ ] **Step 2: Fix any AllItems references**

Replace `AllItems.ContainsKey(x)` with `GetAllItems().ContainsKey(x)` or `GetCount(x) > 0`.

- [ ] **Step 3: Run all tests**

Expected: All tests PASS

- [ ] **Step 4: Commit**

```bash
git add Assets/Tests/PlayMode/StartingInventoryTests.cs
git commit -m "fix: update StartingInventoryTests for slotted inventory API"
```

---

### Task 11: Full integration verification

**Files:**
- No new files — manual testing and final test run

- [ ] **Step 1: Run all EditMode tests**

Expected: All pass

- [ ] **Step 2: Run all PlayMode tests**

Expected: All pass

- [ ] **Step 3: Manual playtest checklist**

- [ ] Press I → inventory opens, player frozen
- [ ] See 5×4 grid with Berry×3 in first slot
- [ ] Left-click Berry slot → sidebar shows Berry details
- [ ] Right-click Berry slot → 1 Berry drops to ground near player
- [ ] Walk to DroppedItem → E to pick up
- [ ] Press I or Escape → inventory closes, player moves again
- [ ] Add items via Debug Menu → verify they fill slots correctly
- [ ] Fill all 20 slots → try to add more → "Inventory full" toast
- [ ] Verify existing systems still work: buy from cart, ferment, sell, repair buildings
