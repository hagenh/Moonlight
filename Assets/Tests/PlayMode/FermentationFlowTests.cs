using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FermentationFlowTests
{
    private TimeManager _timeManager;
    private InventoryManager _inventory;
    private FermentManager _fermentManager;
    private FermentVat _vat;
    private RecipeData _recipe;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var vatGo = TestBootstrap.CreateGameObject("TestVat");
        _vat = vatGo.AddComponent<FermentVat>();

        _fermentManager = TestBootstrap.CreateSingleton<FermentManager>();

        _recipe = new RecipeData("Test Mash", 1, 2, ContentDb.BasicMoonshine)
            .AddIngredient(ContentDb.Grain, 1)
            .AddIngredient(ContentDb.Water, 1)
            .AddIngredient(ContentDb.Yeast, 1);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator TryStartBatch_EmptyVat_ConsumesIngredients_SetsFermenting()
    {
        _inventory.TryAdd(ContentDb.Grain, 1);
        _inventory.TryAdd(ContentDb.Water, 1);
        _inventory.TryAdd(ContentDb.Yeast, 1);

        bool result = _fermentManager.TryStartBatch(_vat, _recipe);

        Assert.IsTrue(result);
        Assert.AreEqual(VatState.Fermenting, _vat.State);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Grain));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Water));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Yeast));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryStartBatch_MissingIngredient_ReturnsFalse()
    {
        _inventory.TryAdd(ContentDb.Grain, 1);

        bool result = _fermentManager.TryStartBatch(_vat, _recipe);

        Assert.IsFalse(result);
        Assert.AreEqual(VatState.Empty, _vat.State);
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Grain));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryStartBatch_NonEmptyVat_ReturnsFalse()
    {
        _inventory.TryAdd(ContentDb.Grain, 2);
        _inventory.TryAdd(ContentDb.Water, 2);
        _inventory.TryAdd(ContentDb.Yeast, 2);

        _fermentManager.TryStartBatch(_vat, _recipe);

        bool result = _fermentManager.TryStartBatch(_vat, _recipe);

        Assert.IsFalse(result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator FullFermentationCycle_CompletesAndCollects()
    {
        _inventory.TryAdd(ContentDb.Grain, 1);
        _inventory.TryAdd(ContentDb.Water, 1);
        _inventory.TryAdd(ContentDb.Yeast, 1);

        _fermentManager.TryStartBatch(_vat, _recipe);
        Assert.AreEqual(VatState.Fermenting, _vat.State);

        _timeManager.SetTime(
            _timeManager.Day + 1,
            _timeManager.Hour,
            _timeManager.Minute);

        for (int i = 0; i < 5; i++)
            yield return null;

        Assert.AreEqual(VatState.Ready, _vat.State);

        bool collected = _fermentManager.TryCollectBatch(_vat);
        Assert.IsTrue(collected);
        Assert.AreEqual(VatState.Empty, _vat.State);
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.BasicMoonshine));
    }

    [UnityTest]
    public IEnumerator TryCollectBatch_NotReady_ReturnsFalse()
    {
        _inventory.TryAdd(ContentDb.Grain, 1);
        _inventory.TryAdd(ContentDb.Water, 1);
        _inventory.TryAdd(ContentDb.Yeast, 1);

        _fermentManager.TryStartBatch(_vat, _recipe);

        bool result = _fermentManager.TryCollectBatch(_vat);
        Assert.IsFalse(result);
        Assert.AreEqual(VatState.Fermenting, _vat.State);
        yield return null;
    }
}
