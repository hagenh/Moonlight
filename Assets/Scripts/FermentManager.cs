using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FermentManager : MonoBehaviour
{
    public static FermentManager Instance { get; private set; }

    private RecipeData[] _recipes;

    private readonly List<FermentVat> _vats = new();
    private readonly Dictionary<FermentVat, int> _lastProgressPercent = new();
    private readonly HashSet<string> _discoveredRecipes = new() { "Berry Shine" };

    public IReadOnlyList<RecipeData> Recipes => _recipes;
    public IReadOnlyList<FermentVat> Vats => _vats;

    public IEnumerable<RecipeData> UnlockedRecipes => _recipes.Where(IsRecipeUnlocked);

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
            new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)
                .AddIngredient(ContentDb.Berry, 3),
            new RecipeData("Basic Mash", 4, 3, ContentDb.BasicMoonshine, "Homestead")
                .AddIngredient(ContentDb.Grain, 2)
                .AddIngredient(ContentDb.Water, 1)
                .AddIngredient(ContentDb.Yeast, 1),
            new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine, "Bakery")
                .AddIngredient(ContentDb.Grain, 1)
                .AddIngredient(ContentDb.Sugar, 2)
                .AddIngredient(ContentDb.Yeast, 1)
                .AddIngredient(ContentDb.Water, 1),
            new RecipeData("Highland Mash", 8, 5, ContentDb.HighlandMoonshine, "Mill")
                .AddIngredient(ContentDb.Grain, 4)
                .AddIngredient(ContentDb.Yeast, 1)
                .AddIngredient(ContentDb.Water, 2),
            new RecipeData("Aged Reserve", 12, 3, ContentDb.AgedReserve, null, 50)
                .AddIngredient(ContentDb.Grain, 2)
                .AddIngredient(ContentDb.Sugar, 2)
                .AddIngredient(ContentDb.Yeast, 2)
                .AddIngredient(ContentDb.Water, 2)
        };

        foreach (var vat in FindObjectsByType<FermentVat>(FindObjectsSortMode.None))
            Register(vat);
    }

    private void OnEnable()
    {
        GameEvents.RecipeDiscovered += OnRecipeDiscovered;
        GameEvents.BuildingStateChanged += OnBuildingRestored;
    }

    private void OnDisable()
    {
        GameEvents.RecipeDiscovered -= OnRecipeDiscovered;
        GameEvents.BuildingStateChanged -= OnBuildingRestored;
    }

    private void OnRecipeDiscovered(string recipeId)
    {
        _discoveredRecipes.Add(recipeId);
    }

    private void OnBuildingRestored(Building b, BuildingState oldState, BuildingState newState)
    {
        if (newState != BuildingState.Restored) return;
        foreach (var recipe in _recipes)
            if (recipe.unlockedByBuildingId == b.BuildingName)
                _discoveredRecipes.Add(recipe.recipeName);
    }

    private void Update()
    {
        foreach (var vat in _vats)
        {
            if (vat.State != VatState.Fermenting || vat.CurrentBatch == null) continue;

            float progress = vat.CurrentBatch.Progress;
            int pct = Mathf.FloorToInt(progress * 100f);
            if (!_lastProgressPercent.TryGetValue(vat, out int last) || last != pct)
            {
                _lastProgressPercent[vat] = pct;
                GameEvents.OnBatchProgressed(vat, progress);
            }

            if (vat.CurrentBatch.IsComplete)
            {
                _lastProgressPercent.Remove(vat);
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

    public void Unregister(FermentVat vat)
    {
        _vats.Remove(vat);
        _lastProgressPercent.Remove(vat);
    }

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

        var batch = new FermentBatch(recipe, () => TimeManager.Instance.TotalGameMinutes);
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

    public bool TryCollectBatchAsCrate(FermentVat vat)
    {
        if (vat.State != VatState.Ready || vat.CurrentBatch == null) return false;

        var recipe = vat.CurrentBatch.Recipe;
        Vector3 spawnPos = FindClearPositionNear(vat.transform.position);
        Crate.Create(recipe.outputItem, recipe.outputCount, spawnPos);
        var oldState = vat.State;
        vat.ClearBatch();

        GameEvents.OnVatStateChanged(vat, oldState, vat.State);
        GameEvents.OnToastRequested($"Collected {recipe.outputCount} {recipe.outputItem.displayName} in a crate");
        return true;
    }

    private Vector3 FindClearPositionNear(Vector3 origin)
    {
        Vector2[] offsets = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1),
            Vector2.up * 1.5f, Vector2.down * 1.5f, Vector2.left * 1.5f, Vector2.right * 1.5f,
        };

        foreach (var offset in offsets)
        {
            Vector2 candidate = (Vector2)origin + offset;
            var hit = Physics2D.OverlapBox(candidate, new Vector2(0.6f, 0.6f), 0f);
            if (hit == null || hit.isTrigger)
                return candidate;
        }

        return origin + Vector3.up * 0.5f;
    }

    public bool IsRecipeUnlocked(RecipeData recipe)
    {
        if (recipe.minReputation > 0 && GameManager.Instance != null
            && GameManager.Instance.Reputation < recipe.minReputation) return false;

        if (recipe.unlockedByBuildingId != null && BuildingManager.Instance != null)
        {
            var b = BuildingManager.Instance.Buildings.FirstOrDefault(x => x.BuildingName == recipe.unlockedByBuildingId);
            if (b == null || b.State != BuildingState.Restored) return false;
        }

        return true;
    }

    public bool IsRecipeDiscovered(RecipeData recipe) => _discoveredRecipes.Contains(recipe.recipeName);
}
