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
    public void CompleteSwing_BeforeThirdSwing_YieldsNothing()
    {
        _pile.CompleteSwing();
        _pile.CompleteSwing();

        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
        Assert.IsFalse(_pile.IsHarvested);
    }

    [Test]
    public void CompleteSwing_ThirdSwing_AddsStoneAndMarksHarvested()
    {
        _pile.CompleteSwing();
        _pile.CompleteSwing();
        _pile.CompleteSwing();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
        Assert.IsTrue(_pile.IsHarvested);
    }

    [Test]
    public void CompleteSwing_AfterHarvested_DoesNothing()
    {
        _pile.CompleteSwing();
        _pile.CompleteSwing();
        _pile.CompleteSwing();

        _pile.CompleteSwing();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void RequiredTool_IsPickaxe()
    {
        Assert.AreEqual(ContentDb.Pickaxe, _pile.RequiredTool);
    }

    [Test]
    public void SwingsNeeded_IsThree()
    {
        Assert.AreEqual(3, _pile.SwingsNeeded);
    }
}
