using System.Collections;
using Lamplight.TestSupport;
using Lamplight.TestSupport.Fakes;
using NUnit.Framework;
using UnityEngine.TestTools;

public class EconomyFlowTests
{
    private GameManager _gameManager;
    private InventoryManager _inventory;
    private SellManager _sellManager;
    private ItemDef _moonshine;
    private ItemDef _grain;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _sellManager = TestBootstrap.CreateSingleton<SellManager>();

        _moonshine = new ItemDef("basic_moonshine", "Basic Moonshine", false, 25, true);
        _grain = new ItemDef("grain", "Grain", true, 5);

        _sellManager.SetRng(new StubRng(0.99f));
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator ExecuteSale_Tormod_AddsCashRemovesItems()
    {
        _inventory.TryAdd(_moonshine, 3);
        int cashBefore = _gameManager.Cash;

        bool result = _sellManager.ExecuteSale(_moonshine, 2, SellerType.Tormod);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _inventory.GetCount(_moonshine));
        Assert.AreEqual(cashBefore + 25 * 2, _gameManager.Cash);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ExecuteSale_RiskyBuyer_DoublesPriceAndAddsHeat()
    {
        _inventory.TryAdd(_moonshine, 1);
        _sellManager.SetRng(new StubRng(0.99f));
        int cashBefore = _gameManager.Cash;
        int heatBefore = _gameManager.Heat;

        bool result = _sellManager.ExecuteSale(_moonshine, 1, SellerType.RiskyBuyer);

        Assert.IsTrue(result);
        Assert.AreEqual(0, _inventory.GetCount(_moonshine));
        Assert.AreEqual(cashBefore + 50, _gameManager.Cash);
        Assert.AreEqual(heatBefore + 15, _gameManager.Heat);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ExecuteSale_RiskyBuyer_ConfiscationAtHeatAbove50()
    {
        _inventory.TryAdd(_moonshine, 2);
        _gameManager.SetHeat(60);
        _sellManager.SetRng(new StubRng(0.05f));
        int cashBefore = _gameManager.Cash;
        int heatBefore = _gameManager.Heat;

        bool result = _sellManager.ExecuteSale(_moonshine, 2, SellerType.RiskyBuyer);

        Assert.IsTrue(result);
        Assert.AreEqual(0, _inventory.GetCount(_moonshine));
        Assert.AreEqual(cashBefore, _gameManager.Cash);
        Assert.AreEqual(heatBefore + 15, _gameManager.Heat);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ExecuteSale_RiskyBuyer_NoConfiscationAtHeat50()
    {
        _inventory.TryAdd(_moonshine, 1);
        _gameManager.SetHeat(50);
        _sellManager.SetRng(new StubRng(0.0f));
        int cashBefore = _gameManager.Cash;

        bool result = _sellManager.ExecuteSale(_moonshine, 1, SellerType.RiskyBuyer);

        Assert.IsTrue(result);
        Assert.AreEqual(cashBefore + 50, _gameManager.Cash);
        yield return null;
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
    public IEnumerator GetSellPrice_Tormod_IsBasePrice()
    {
        Assert.AreEqual(25, _sellManager.GetSellPrice(_moonshine, SellerType.Tormod));
        yield return null;
    }

    [UnityTest]
    public IEnumerator GetSellPrice_RiskyBuyer_Is2x()
    {
        Assert.AreEqual(50, _sellManager.GetSellPrice(_moonshine, SellerType.RiskyBuyer));
        yield return null;
    }
}
