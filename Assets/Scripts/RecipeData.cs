using System.Collections.Generic;

public class RecipeData
{
    public string recipeName;
    public int fermentationHours;
    public int outputCount;
    public ItemDef outputItem;
    public string unlockedByBuildingId;
    public int minReputation;
    public int suspicionPerDrop;

    private readonly List<KeyValuePair<ItemDef, int>> _ingredients = new();

    public IReadOnlyList<KeyValuePair<ItemDef, int>> Ingredients => _ingredients;

    private Dictionary<ItemDef, int> _costs;

    public IReadOnlyDictionary<ItemDef, int> Costs
    {
        get
        {
            if (_costs == null)
            {
                _costs = new Dictionary<ItemDef, int>();
                foreach (var ing in _ingredients)
                {
                    if (_costs.ContainsKey(ing.Key))
                        _costs[ing.Key] += ing.Value;
                    else
                        _costs[ing.Key] = ing.Value;
                }
            }
            return _costs;
        }
    }

    public RecipeData(string recipeName, int fermentationHours, int outputCount, ItemDef outputItem,
        string unlockedByBuildingId = null, int minReputation = 0, int suspicionPerDrop = 5)
    {
        this.recipeName = recipeName;
        this.fermentationHours = fermentationHours;
        this.outputCount = outputCount;
        this.outputItem = outputItem;
        this.unlockedByBuildingId = unlockedByBuildingId;
        this.minReputation = minReputation;
        this.suspicionPerDrop = suspicionPerDrop;
    }

    public RecipeData AddIngredient(ItemDef def, int amount)
    {
        _ingredients.Add(new KeyValuePair<ItemDef, int>(def, amount));
        _costs = null;
        return this;
    }
}
