using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class BerryBushTests
{
    private InventoryManager _inventory;
    private BerryBush _bush;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestBush");
        _bush = go.AddComponent<BerryBush>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsBerryToInventory()
    {
        _bush.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _bush.Interact();
        _bush.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _bush.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));

        GameEvents.OnDayEnded(1);

        _bush.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Berry));
    }
}
