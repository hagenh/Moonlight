using System.Collections;
using System.Collections.Generic;
using Lamplight.TestSupport;
using Lamplight.TestSupport.Fakes;
using NUnit.Framework;
using UnityEngine.TestTools;

public class StandFlowTests
{
    private InventoryManager _inventory;
    private GameManager _game;
    private StandManager _stand;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _game = TestBootstrap.CreateSingleton<GameManager>();
        _stand = TestBootstrap.CreateSingleton<StandManager>();
        _stand.SetRng(new SeededRng(1));
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    private static StandRequest Note(string id, ItemDef item, int units) =>
        new StandRequest(id, RequestKind.Exact, new List<ItemDef> { item }, units, "A carter", "Two jars.");

    [UnityTest]
    public IEnumerator Book_StartsWithThreeSlots()
    {
        Assert.AreEqual(3, _stand.Book.SlotCount);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Awake_SeedsOrdersImmediately()
    {
        Assert.AreEqual(2, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DayEnded_PostsTwoNotes()
    {
        GameEvents.OnDayEnded(1);

        Assert.AreEqual(2, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DayEnded_NeverOverfillsTheBook()
    {
        GameEvents.OnDayEnded(1);
        GameEvents.OnDayEnded(2);
        GameEvents.OnDayEnded(3);

        Assert.AreEqual(3, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WithEnoughStock_PaysThePremiumAndClearsTheSlot()
    {
        var note = Note("a", ContentDb.BerryShine, 4);
        _stand.Book.TryPost(note);
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;

        bool filled = _stand.TryFill("a", ContentDb.BerryShine);

        Assert.IsTrue(filled);
        Assert.AreEqual(cashBefore + 108, _game.Cash);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(0, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WithoutEnoughStock_TakesNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 4));
        _inventory.TryAdd(ContentDb.BerryShine, 2);
        int cashBefore = _game.Cash;

        bool filled = _stand.TryFill("a", ContentDb.BerryShine);

        Assert.IsFalse(filled);
        Assert.AreEqual(cashBefore, _game.Cash);
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(1, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WrongProduct_TakesNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 2));
        _inventory.TryAdd(ContentDb.SweetMoonshine, 10);

        Assert.IsFalse(_stand.TryFill("a", ContentDb.SweetMoonshine));
        Assert.AreEqual(10, _inventory.GetCount(ContentDb.SweetMoonshine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Decline_FreesTheSlotAndCostsNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 4));
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;

        bool declined = _stand.Decline("a");

        Assert.IsTrue(declined);
        Assert.AreEqual(0, _stand.Book.Active.Count);
        Assert.AreEqual(cashBefore, _game.Cash);
        Assert.AreEqual(4, _inventory.GetCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Shelf_SellsStockAtBasePriceOnDayEnd()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 3);
        _stand.StockShelf(ContentDb.BerryShine, 3);
        int cashBefore = _game.Cash;

        GameEvents.OnDayEnded(1);

        Assert.AreEqual(cashBefore + 45, _game.Cash);
        Assert.AreEqual(0, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Shelf_PaysLessThanTheSameGoodsAgainstARequest()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        _stand.StockShelf(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;
        GameEvents.OnDayEnded(1);
        int shelfEarnings = _game.Cash - cashBefore;

        Assert.Less(shelfEarnings, 108);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StockShelf_MovesGoodsOutOfInventory()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 5);

        _stand.StockShelf(ContentDb.BerryShine, 2);

        Assert.AreEqual(3, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(2, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator StockShelf_MoreThanHeld_StocksNothing()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 1);

        _stand.StockShelf(ContentDb.BerryShine, 5);

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(0, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }
}
