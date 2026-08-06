using System.Collections;
using Lamplight.TestSupport;
using Lamplight.TestSupport.Fakes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SfxManagerFlowTests
{
    private SfxManager _sfx;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _sfx = TestBootstrap.CreateSingleton<SfxManager>();
        _sfx.SetRng(new SeededRng(1));
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator CashChanged_PlaysCoinClip()
    {
        var clip = AudioClip.Create("coin", 1, 1, 44100, false);
        _sfx.coinClips = new[] { clip };
        yield return null;

        GameEvents.OnCashChanged(100);

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator InventoryChanged_CountIncreased_PlaysPickupClip()
    {
        var clip = AudioClip.Create("pickup", 1, 1, 44100, false);
        _sfx.pickupClips = new[] { clip };
        yield return null;

        GameEvents.OnInventoryChanged(ContentDb.Stone, 0, 1);

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator InventoryChanged_CountDecreased_DoesNotPlayPickupClip()
    {
        var clip = AudioClip.Create("pickup", 1, 1, 44100, false);
        _sfx.pickupClips = new[] { clip };
        yield return null;

        GameEvents.OnInventoryChanged(ContentDb.Stone, 5, 3);

        Assert.IsNull(_sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator ItemDropped_PlaysDropClip()
    {
        var clip = AudioClip.Create("drop", 1, 1, 44100, false);
        _sfx.dropClips = new[] { clip };
        yield return null;

        GameEvents.OnItemDropped(0, ContentDb.Stone, 1);

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator SmashHit_PlaysHammerClip()
    {
        var clip = AudioClip.Create("hammer", 1, 1, 44100, false);
        _sfx.hammerClips = new[] { clip };
        var building = TestBootstrap.CreateGameObject("TestBuilding").AddComponent<Building>();
        yield return null;

        GameEvents.OnSmashHit(building, 1, 3);

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator RequestBookRequested_PlaysBookOpenClip()
    {
        var clip = AudioClip.Create("book", 1, 1, 44100, false);
        _sfx.bookOpenClips = new[] { clip };
        yield return null;

        GameEvents.OnRequestBookRequested();

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator RecipeBookRequested_PlaysBookOpenClip()
    {
        var clip = AudioClip.Create("book", 1, 1, 44100, false);
        _sfx.bookOpenClips = new[] { clip };
        yield return null;

        GameEvents.OnRecipeBookRequested();

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator InventoryOpened_PlaysBagOpenClip()
    {
        var clip = AudioClip.Create("bagopen", 1, 1, 44100, false);
        _sfx.bagOpenClips = new[] { clip };
        yield return null;

        GameEvents.OnInventoryOpened();

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator InventoryClosed_PlaysBagCloseClip()
    {
        var clip = AudioClip.Create("bagclose", 1, 1, 44100, false);
        _sfx.bagCloseClips = new[] { clip };
        yield return null;

        GameEvents.OnInventoryClosed();

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator SellMenuRequested_PlaysSelectClip()
    {
        var clip = AudioClip.Create("select", 1, 1, 44100, false);
        _sfx.selectClips = new[] { clip };
        yield return null;

        GameEvents.OnSellMenuRequested(SellerType.TravelingCart);

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }

    [UnityTest]
    public IEnumerator MenuCloseRequested_PlaysButtonClip()
    {
        var clip = AudioClip.Create("button", 1, 1, 44100, false);
        _sfx.buttonClips = new[] { clip };
        yield return null;

        GameEvents.OnMenuCloseRequested();

        Assert.AreEqual(clip, _sfx.LastPlayedClip);
    }
}
