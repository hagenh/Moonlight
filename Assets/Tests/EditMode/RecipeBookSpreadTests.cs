using System.Collections.Generic;
using NUnit.Framework;

public class RecipeBookSpreadTests
{
    private static List<BookPage> Pages(int count)
    {
        var pages = new List<BookPage>();
        for (int i = 0; i < count; i++)
            pages.Add(new BookPage(i + 1, new RecipeData($"R{i + 1}", 3, 2, ContentDb.BerryShine)));
        return pages;
    }

    [Test]
    public void FourPages_ProduceTwoSpreadsPlusTheBurnedSection()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.AreEqual(3, spreads.Count);
    }

    [Test]
    public void EvenPageCount_EverySpreadHasBothPages()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.IsTrue(spreads[0].HasRight);
        Assert.IsTrue(spreads[1].HasRight);
    }

    [Test]
    public void OddPageCount_LastRecipeSpreadHasNoRightPage()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(5));

        Assert.AreEqual(4, spreads.Count);
        Assert.IsFalse(spreads[2].HasRight);
        Assert.AreEqual(5, spreads[2].Left.PageNumber);
    }

    [Test]
    public void BurnedSpread_IsAlwaysLast()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(5));

        Assert.IsTrue(spreads[spreads.Count - 1].IsBurnedSection);
        for (int i = 0; i < spreads.Count - 1; i++)
            Assert.IsFalse(spreads[i].IsBurnedSection);
    }

    [Test]
    public void BurnedSpread_IsPresentEvenWithNoRecipes()
    {
        var spreads = RecipeBookRules.CompileSpreads(new List<BookPage>());

        Assert.AreEqual(1, spreads.Count);
        Assert.IsTrue(spreads[0].IsBurnedSection);
    }

    [Test]
    public void CompileSpreads_ToleratesNull()
    {
        var spreads = RecipeBookRules.CompileSpreads(null);

        Assert.AreEqual(1, spreads.Count);
        Assert.IsTrue(spreads[0].IsBurnedSection);
    }

    [Test]
    public void PageNumbers_RunInOrderAcrossSpreads()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.AreEqual(1, spreads[0].Left.PageNumber);
        Assert.AreEqual(2, spreads[0].Right.PageNumber);
        Assert.AreEqual(3, spreads[1].Left.PageNumber);
        Assert.AreEqual(4, spreads[1].Right.PageNumber);
    }

    [Test]
    public void ATornPage_HoldsItsSlotRatherThanCollapsing()
    {
        var pages = new List<BookPage>
        {
            new BookPage(1, new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)),
            new BookPage(2, null),
            new BookPage(3, new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine))
        };

        var spreads = RecipeBookRules.CompileSpreads(pages);

        Assert.AreEqual(2, spreads[0].Right.PageNumber);
        Assert.IsFalse(spreads[0].Right.IsLegible);
        Assert.AreEqual(3, spreads[1].Left.PageNumber);
    }

    [Test]
    public void ClampSpreadIndex_BelowZero_ReturnsZero()
    {
        Assert.AreEqual(0, RecipeBookRules.ClampSpreadIndex(-3, 4));
    }

    [Test]
    public void ClampSpreadIndex_PastTheEnd_ReturnsLastSpread()
    {
        Assert.AreEqual(3, RecipeBookRules.ClampSpreadIndex(99, 4));
    }

    [Test]
    public void ClampSpreadIndex_InRange_IsUnchanged()
    {
        Assert.AreEqual(2, RecipeBookRules.ClampSpreadIndex(2, 4));
    }

    [Test]
    public void ClampSpreadIndex_EmptyBook_ReturnsZero()
    {
        Assert.AreEqual(0, RecipeBookRules.ClampSpreadIndex(5, 0));
    }
}
