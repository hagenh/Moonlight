using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ForageRespawnFlowTests
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
    public IEnumerator BerryBush_Interact_RespawnsAtMidday()
    {
        var bush = TestBootstrap.CreateGameObject("TestBush").AddComponent<BerryBush>();
        yield return null;

        bush.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));

        GameEvents.OnHourChanged(12, 1);

        bush.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator FallenLog_CompleteSwing_RespawnsAtMidday()
    {
        var log = TestBootstrap.CreateGameObject("TestLog").AddComponent<FallenLog>();
        yield return null;

        log.CompleteSwing();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));

        GameEvents.OnHourChanged(12, 1);

        log.CompleteSwing();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Wood));
    }

    [UnityTest]
    public IEnumerator StonePile_CompleteSwing_RespawnsAtMidday()
    {
        var pile = TestBootstrap.CreateGameObject("TestPile").AddComponent<StonePile>();
        yield return null;

        pile.CompleteSwing();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));

        GameEvents.OnHourChanged(12, 1);

        pile.CompleteSwing();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Stone));
    }

    [UnityTest]
    public IEnumerator StonePile_DoesNotRespawnBeforeMidday()
    {
        var pile = TestBootstrap.CreateGameObject("TestPile").AddComponent<StonePile>();
        yield return null;

        pile.CompleteSwing();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));

        GameEvents.OnHourChanged(10, 1);

        Assert.IsTrue(pile.IsHarvested);
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }
}
