using System.Collections;
using System.Linq;
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
        GameEvents.ActiveSlotChanged += (index) =>
            _recorder.Record("ActiveSlotChanged", index.ToString());
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
        Assert.IsTrue(_recorder.Order.Any(e => e.StartsWith("InventoryFull")));
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetActiveSlot_ValidIndex_UpdatesAndFiresEvent()
    {
        _inventory.SetActiveSlot(3);

        Assert.AreEqual(3, _inventory.ActiveSlotIndex);
        Assert.AreEqual(1, _recorder.Count);
        Assert.AreEqual("ActiveSlotChanged: 3", _recorder.Order[0]);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetActiveSlot_OutOfRange_NoOp()
    {
        _inventory.SetActiveSlot(-1);
        _inventory.SetActiveSlot(InventoryManager.HotbarSlotCount);

        Assert.AreEqual(0, _inventory.ActiveSlotIndex);
        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetActiveSlot_SameIndex_NoEventFires()
    {
        _inventory.SetActiveSlot(2);
        _recorder.Clear();

        _inventory.SetActiveSlot(2);

        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }
}
