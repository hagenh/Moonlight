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
    public IEnumerator TryRemove_ToZero_RemovesEntry()
    {
        _inventory.TryAdd(_grain, 3);
        _recorder.Clear();

        _inventory.TryRemove(_grain, 3);

        Assert.AreEqual(0, _inventory.GetCount(_grain));
        Assert.IsFalse(_inventory.AllItems.ContainsKey(_grain));
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
}
