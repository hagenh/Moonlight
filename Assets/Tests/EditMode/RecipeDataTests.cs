using NUnit.Framework;

public class RecipeDataTests
{
    private ItemDef _grain;
    private ItemDef _water;

    [SetUp]
    public void SetUp()
    {
        _grain = new ItemDef("grain", "Grain", true, 5);
        _water = new ItemDef("water", "Water", true, 2);
    }

    [Test]
    public void Costs_AggregatesDuplicateIngredients()
    {
        var recipe = new RecipeData("Test", 4, 1, _grain)
            .AddIngredient(_grain, 2)
            .AddIngredient(_grain, 3)
            .AddIngredient(_water, 1);

        Assert.AreEqual(5, recipe.Costs[_grain]);
        Assert.AreEqual(1, recipe.Costs[_water]);
    }

    [Test]
    public void Costs_LazyInitCaches()
    {
        var recipe = new RecipeData("Test", 4, 1, _grain)
            .AddIngredient(_grain, 2);

        var first = recipe.Costs;
        var second = recipe.Costs;
        Assert.AreSame(first, second);
    }

    [Test]
    public void AddIngredient_NullsCachedCosts()
    {
        var recipe = new RecipeData("Test", 4, 1, _grain)
            .AddIngredient(_grain, 2);

        var first = recipe.Costs;
        recipe.AddIngredient(_water, 1);
        var second = recipe.Costs;

        Assert.AreNotSame(first, second);
        Assert.IsTrue(second.ContainsKey(_water));
    }

    [Test]
    public void Costs_EmptyRecipeIsEmpty()
    {
        var recipe = new RecipeData("Test", 4, 1, _grain);
        Assert.AreEqual(0, recipe.Costs.Count);
    }

    [Test]
    public void Ingredients_PreservesInsertionOrder()
    {
        var recipe = new RecipeData("Test", 4, 1, _grain)
            .AddIngredient(_grain, 2)
            .AddIngredient(_water, 1)
            .AddIngredient(_grain, 1);

        Assert.AreEqual(3, recipe.Ingredients.Count);
        Assert.AreEqual(_grain, recipe.Ingredients[0].Key);
        Assert.AreEqual(_water, recipe.Ingredients[1].Key);
        Assert.AreEqual(_grain, recipe.Ingredients[2].Key);
    }
}
