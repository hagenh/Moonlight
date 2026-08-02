using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DroppedItemTests
{
    private InventoryManager _inventory;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator Create_SetsItemAndCount()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 3, Vector3.zero);
        TestBootstrap.Track(di.gameObject);

        Assert.AreEqual(item, di.Item);
        Assert.AreEqual(3, di.Count);
        Assert.IsTrue(di.CanInteract);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Create_AddsSpriteRendererAndCollider()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 1, new Vector3(5, 3, 0));
        TestBootstrap.Track(di.gameObject);

        Assert.AreEqual(new Vector3(5, 3, 0), di.transform.position);
        Assert.IsNotNull(di.GetComponent<SpriteRenderer>());
        Assert.IsNotNull(di.GetComponent<BoxCollider2D>());
        Assert.IsTrue(di.GetComponent<BoxCollider2D>().isTrigger);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Interact_AddsToInventoryAndDestroysSelf()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 3, Vector3.zero);
        TestBootstrap.Track(di.gameObject);

        di.Interact();

        Assert.AreEqual(3, _inventory.GetCount(item));
        yield return null;
        Assert.IsTrue(di == null);
    }

    [UnityTest]
    public IEnumerator Interact_PartialPickup_KeepsRemainingCount()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        for (int i = 0; i < 19; i++)
            _inventory.TryAdd(item, 30);
        _inventory.TryAdd(item, 27);

        var di = DroppedItem.Create(item, 5, Vector3.zero);
        TestBootstrap.Track(di.gameObject);
        di.Interact();

        Assert.AreEqual(2, di.Count);
        Assert.IsTrue(di.CanInteract);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Interact_FullInventory_DoesNothing()
    {
        var grain = new ItemDef("grain", "Grain", true, 5);
        var sugar = new ItemDef("sugar", "Sugar", true, 5);
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(grain, 30);

        var di = DroppedItem.Create(sugar, 5, Vector3.zero);
        TestBootstrap.Track(di.gameObject);
        di.Interact();

        Assert.AreEqual(0, _inventory.GetCount(sugar));
        Assert.AreEqual(5, di.Count);
        Assert.IsTrue(di.CanInteract);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Interact_NoInventoryManager_DoesNothing()
    {
        TestBootstrap.DestroyAll();
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 3, Vector3.zero);
        TestBootstrap.Track(di.gameObject);

        di.Interact();

        Assert.AreEqual(3, di.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CanInteract_FalseWhenCountZero()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 0, Vector3.zero);
        TestBootstrap.Track(di.gameObject);

        Assert.IsFalse(di.CanInteract);
        yield return null;
    }

    [UnityTest]
    public IEnumerator InteractType_IsDroppedItem()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var di = DroppedItem.Create(item, 1, Vector3.zero);
        TestBootstrap.Track(di.gameObject);

        Assert.AreEqual(InteractType.DroppedItem, di.InteractType);
        yield return null;
    }
}
