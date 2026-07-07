using System.Collections.Generic;
using UnityEngine;

public class FermentManager : MonoBehaviour
{
    public static FermentManager Instance { get; private set; }

    private RecipeData[] _recipes;

    private readonly List<FermentVat> _vats = new();

    public IReadOnlyList<RecipeData> Recipes => _recipes;
    public IReadOnlyList<FermentVat> Vats => _vats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _recipes = new RecipeData[]
        {
            new RecipeData("Basic Mash", 4, 3, ContentDb.BasicMoonshine)
                .AddIngredient(ContentDb.Grain, 2)
                .AddIngredient(ContentDb.Water, 1)
                .AddIngredient(ContentDb.Yeast, 1),
            new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine)
                .AddIngredient(ContentDb.Grain, 1)
                .AddIngredient(ContentDb.Sugar, 2)
                .AddIngredient(ContentDb.Yeast, 1)
                .AddIngredient(ContentDb.Water, 1)
        };

        foreach (var vat in FindObjectsByType<FermentVat>(FindObjectsSortMode.None))
            Register(vat);
    }

    private void Update()
    {
        foreach (var vat in _vats)
        {
            if (vat.State != VatState.Fermenting || vat.CurrentBatch == null) continue;

            GameEvents.OnBatchProgressed(vat, vat.CurrentBatch.Progress);

            if (vat.CurrentBatch.IsComplete)
            {
                var oldState = vat.State;
                vat.MarkReady();
                GameEvents.OnVatStateChanged(vat, oldState, vat.State);
                GameEvents.OnToastRequested($"{vat.CurrentBatch.Recipe.recipeName} is ready!");
            }
        }
    }

    public void Register(FermentVat vat)
    {
        if (!_vats.Contains(vat)) _vats.Add(vat);
    }

    public void Unregister(FermentVat vat) => _vats.Remove(vat);

    public bool TryStartBatch(FermentVat vat, RecipeData recipe)
    {
        if (vat.State != VatState.Empty) return false;

        foreach (var kvp in recipe.Costs)
        {
            if (!InventoryManager.Instance.Has(kvp.Key, kvp.Value))
            {
                GameEvents.OnToastRequested($"Missing {kvp.Key.displayName} (need {kvp.Value})");
                return false;
            }
        }

        foreach (var kvp in recipe.Costs)
            InventoryManager.Instance.TryRemove(kvp.Key, kvp.Value);

        var batch = new FermentBatch(recipe);
        var oldState = vat.State;
        vat.SetBatch(batch);

        GameEvents.OnVatStateChanged(vat, oldState, vat.State);
        GameEvents.OnToastRequested($"Started {recipe.recipeName} ({recipe.fermentationHours}h)");
        return true;
    }

    public bool TryCollectBatch(FermentVat vat)
    {
        if (vat.State != VatState.Ready) return false;
        if (vat.CurrentBatch == null) return false;

        var oldState = vat.State;
        int bottles = vat.CurrentBatch.Recipe.outputCount;
        InventoryManager.Instance.TryAdd(vat.CurrentBatch.Recipe.outputItem, bottles);
        vat.ClearBatch();

        GameEvents.OnVatStateChanged(vat, oldState, vat.State);
        return true;
    }
}
