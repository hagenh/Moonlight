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
    public IEnumerator BerryBush_Interact_AfterDayEnded_YieldsAgain()
    {
        var bush = TestBootstrap.CreateGameObject("TestBush").AddComponent<BerryBush>();
        yield return null;

        bush.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));

        GameEvents.OnDayEnded(1);
        Assert.IsFalse(bush.IsHarvested);

        bush.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator FallenLog_Interact_AfterDayEnded_YieldsAgain()
    {
        var log = TestBootstrap.CreateGameObject("TestLog").AddComponent<FallenLog>();
        yield return null;

        log.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));

        GameEvents.OnDayEnded(1);

        log.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Wood));
    }

    [UnityTest]
    public IEnumerator StonePile_Interact_AfterDayEnded_YieldsAgain()
    {
        var pile = TestBootstrap.CreateGameObject("TestPile").AddComponent<StonePile>();
        yield return null;

        pile.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));

        GameEvents.OnDayEnded(1);

        pile.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Stone));
    }
}
