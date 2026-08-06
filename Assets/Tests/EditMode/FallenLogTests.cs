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
    public void CompleteSwing_AddsWoodAndMarksHarvested()
    {
        _log.CompleteSwing();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
        Assert.IsTrue(_log.IsHarvested);
    }

    [Test]
    public void CompleteSwing_AfterHarvested_DoesNothing()
    {
        _log.CompleteSwing();
        _log.CompleteSwing();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void RequiredTool_IsHandAxe()
    {
        Assert.AreEqual(ContentDb.HandAxe, _log.RequiredTool);
    }

    [Test]
    public void SwingDuration_IsOneAndAHalfSeconds()
    {
        Assert.AreEqual(1.5f, _log.SwingDuration);
    }
}
