using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class EconomyFlowTests
{
    private GameManager _gameManager;
    private InventoryManager _inventory;
    private SellManager _sellManager;
    private ItemDef _grain;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _sellManager = TestBootstrap.CreateSingleton<SellManager>();

        _grain = new ItemDef("grain", "Grain", true, 5);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator ExecutePurchase_Cart_RemovesCashAddsItems()
    {
        int cashBefore = _gameManager.Cash;

        bool result = _sellManager.ExecutePurchase(_grain, 3);

        Assert.IsTrue(result);
        Assert.AreEqual(3, _inventory.GetCount(_grain));
        Assert.AreEqual(cashBefore - 5 * 3, _gameManager.Cash);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ExecutePurchase_InsufficientCash_ReturnsFalse()
    {
        int cashBefore = _gameManager.Cash;

        bool result = _sellManager.ExecutePurchase(_grain, 1000);

        Assert.IsFalse(result);
        Assert.AreEqual(cashBefore, _gameManager.Cash);
        Assert.AreEqual(0, _inventory.GetCount(_grain));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TormodSpawnsAtDusk()
    {
        var timeManager = TestBootstrap.CreateSingleton<TimeManager>();

        timeManager.SetTime(1, 17, 0);

        Assert.IsFalse(_sellManager.IsTormodInTown);

        timeManager.AdvanceHour();

        for (int i = 0; i < 3; i++)
            yield return null;

        Assert.IsTrue(_sellManager.IsTormodInTown);
    }
}
