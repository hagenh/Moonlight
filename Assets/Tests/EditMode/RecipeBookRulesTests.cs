using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class RecipeBookRulesTests
{
    private static RecipeData[] TestRecipes() => new[]
    {
        new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)
            .AddIngredient(ContentDb.Berry, 3),
        new RecipeData("Basic Mash", 4, 3, ContentDb.BasicMoonshine, "Homestead"),
        new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine, "Bakery"),
        new RecipeData("Highland Mash", 8, 5, ContentDb.HighlandMoonshine, "Mill")
    };

    private static List<BookPage> CompileWithDiscovered(params string[] discovered)
    {
        var known = new HashSet<string>(discovered);
        return RecipeBookRules.CompilePages(TestRecipes(), r => known.Contains(r.recipeName));
    }

    [Test]
    public void NewGame_HasOneLegiblePage()
    {
        var pages = CompileWithDiscovered("Berry Shine");

        Assert.AreEqual(1, pages.Count(p => p.IsLegible));
        Assert.AreEqual("Berry Shine", pages.First(p => p.IsLegible).Recipe.recipeName);
    }

    [Test]
    public void NewGame_HasTheRestTorn()
    {
        var pages = CompileWithDiscovered("Berry Shine");

        Assert.AreEqual(3, pages.Count(p => !p.IsLegible));
    }

    [Test]
    public void TornPage_CarriesNoRecipe_SoItCannotLeakAName()
    {
        var pages = CompileWithDiscovered("Berry Shine");

        foreach (var page in pages.Where(p => !p.IsLegible))
            Assert.IsNull(page.Recipe);
    }

    [Test]
    public void DiscoveringARecipe_TurnsItsPageLegible()
    {
        var before = CompileWithDiscovered("Berry Shine");
        var after = CompileWithDiscovered("Berry Shine", "Sweet Batch");

        Assert.IsFalse(before[2].IsLegible);
        Assert.IsTrue(after[2].IsLegible);
        Assert.AreEqual("Sweet Batch", after[2].Recipe.recipeName);
    }

    [Test]
    public void PageNumbers_DoNotShift_WhenARecipeIsDiscovered()
    {
        var before = CompileWithDiscovered("Berry Shine");
        var after = CompileWithDiscovered("Berry Shine", "Sweet Batch");

        CollectionAssert.AreEqual(
            before.Select(p => p.PageNumber).ToArray(),
            after.Select(p => p.PageNumber).ToArray());
    }

    [Test]
    public void PageNumbers_StartAtOne_AndRunInOrder()
    {
        var pages = CompileWithDiscovered("Berry Shine");

        for (int i = 0; i < pages.Count; i++)
            Assert.AreEqual(i + 1, pages[i].PageNumber);
    }

    [Test]
    public void BurnedSection_IsPresentFromMinuteZero()
    {
        CollectionAssert.IsNotEmpty(RecipeBookRules.BurnedScraps);
    }

    [Test]
    public void BurnedSection_IsNotARecipe_SoNoUnlockCanEverRestoreIt()
    {
        var everyRecipeDiscovered = RecipeBookRules.CompilePages(TestRecipes(), _ => true);

        var recipeNames = everyRecipeDiscovered
            .Where(p => p.IsLegible)
            .Select(p => p.Recipe.recipeName)
            .ToList();

        foreach (var scrap in RecipeBookRules.BurnedScraps)
            CollectionAssert.DoesNotContain(recipeNames, scrap);

        Assert.AreEqual(TestRecipes().Length, everyRecipeDiscovered.Count,
            "The burned section must never appear as a page, even with everything discovered.");
    }

    [Test]
    public void CompilePages_ToleratesNullInput()
    {
        CollectionAssert.IsEmpty(RecipeBookRules.CompilePages(null, _ => true));
        CollectionAssert.IsEmpty(RecipeBookRules.CompilePages(TestRecipes(), null));
    }
}
