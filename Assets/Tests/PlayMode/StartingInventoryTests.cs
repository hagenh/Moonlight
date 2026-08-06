using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class StartingInventoryTests
{
    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator Day1_GrantsThreeBerry()
    {
        TestBootstrap.CreateSingleton<TimeManager>();
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator Day1_GrantsPickaxeAndHandAxe()
    {
        TestBootstrap.CreateSingleton<TimeManager>();
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(1, inventory.GetCount(ContentDb.Pickaxe));
        Assert.AreEqual(1, inventory.GetCount(ContentDb.HandAxe));
    }

    [UnityTest]
    public IEnumerator NotDay1_GrantsNoBerry()
    {
        var timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        timeManager.SetTime(2, 8, 0);
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator NoTimeManager_GrantsNoBerry_DoesNotThrow()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Berry));
    }
}
