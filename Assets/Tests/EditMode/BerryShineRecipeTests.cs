using NUnit.Framework;

public class BerryShineRecipeTests
{
    private ItemDef _berry;
    private ItemDef _berryShine;

    [SetUp]
    public void SetUp()
    {
        _berry = new ItemDef("berry", "Berry", true, 2);
        _berryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    }

    [Test]
    public void BerryShineRecipe_Requires3Berry()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(3, recipe.Costs[_berry]);
        Assert.AreEqual(1, recipe.Costs.Count);
    }

    [Test]
    public void BerryShineRecipe_Yields2()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(2, recipe.outputCount);
    }

    [Test]
    public void BerryShineRecipe_3HourFerment()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(3, recipe.fermentationHours);
    }

    [Test]
    public void BerryShineRecipe_NoBuildingGate()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.IsNull(recipe.unlockedByBuildingId);
    }
}
