using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class FallenLogTests
{
    private InventoryManager _inventory;
    private FallenLog _log;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestLog");
        _log = go.AddComponent<FallenLog>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsWoodToInventory()
    {
        _log.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _log.Interact();
        _log.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _log.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));

        GameEvents.OnDayEnded(1);

        _log.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Wood));
    }
}
