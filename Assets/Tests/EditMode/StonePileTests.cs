using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class StonePileTests
{
    private InventoryManager _inventory;
    private StonePile _pile;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestPile");
        _pile = go.AddComponent<StonePile>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsStoneToInventory()
    {
        _pile.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _pile.Interact();
        _pile.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _pile.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));

        GameEvents.OnDayEnded(1);

        _pile.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Stone));
    }
}
