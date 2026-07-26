using System.Collections.Generic;
using NUnit.Framework;

public class RecipeBookPageStatusTests
{
    private static RecipeData Simple() =>
        new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)
            .AddIngredient(ContentDb.Berry, 3);

    private static RecipeData GatedByBuilding() =>
        new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine, "Bakery")
            .AddIngredient(ContentDb.Sugar, 2);

    private static RecipeData GatedByReputation() =>
        new RecipeData("Aged Reserve", 12, 3, ContentDb.AgedReserve, null, 50)
            .AddIngredient(ContentDb.Grain, 2);

    private static RecipeData GatedByBoth() =>
        new RecipeData("Highland Mash", 8, 5, ContentDb.HighlandMoonshine, "Mill", 20)
            .AddIngredient(ContentDb.Grain, 4);

    private static PageStatus Status(RecipeData recipe, bool unlocked, int stock) =>
        RecipeBookRules.StatusOf(new BookPage(1, recipe), _ => unlocked, _ => stock);

    [Test]
    public void TornPage_IsTornAndCannotBrew()
    {
        var status = RecipeBookRules.StatusOf(new BookPage(2, null), _ => true, _ => 99);

        Assert.IsTrue(status.IsTorn);
        Assert.IsFalse(status.CanBrew);
    }

    [Test]
    public void UnlockedAndStocked_CanBrew()
    {
        var status = Status(Simple(), unlocked: true, stock: 10);

        Assert.IsTrue(status.IsUnlocked);
        Assert.IsTrue(status.CanAfford);
        Assert.IsTrue(status.CanBrew);
        Assert.AreEqual(LockReason.None, status.Reason);
    }

    [Test]
    public void UnlockedButShortOfIngredients_CannotBrew()
    {
        var status = Status(Simple(), unlocked: true, stock: 1);

        Assert.IsTrue(status.IsUnlocked);
        Assert.IsFalse(status.CanAfford);
        Assert.IsFalse(status.CanBrew);
    }

    [Test]
    public void LockedByBuilding_ReportsTheBuilding()
    {
        var status = Status(GatedByBuilding(), unlocked: false, stock: 99);

        Assert.IsFalse(status.IsUnlocked);
        Assert.IsFalse(status.CanBrew);
        Assert.AreEqual(LockReason.RequiresBuilding, status.Reason);
        Assert.AreEqual("Bakery", status.RequiredBuildingId);
    }

    [Test]
    public void LockedByReputation_ReportsTheThreshold()
    {
        var status = Status(GatedByReputation(), unlocked: false, stock: 99);

        Assert.AreEqual(LockReason.RequiresReputation, status.Reason);
        Assert.AreEqual(50, status.RequiredReputation);
    }

    [Test]
    public void LockedByBoth_PrefersTheBuilding_ButKeepsBothRequirements()
    {
        var status = Status(GatedByBoth(), unlocked: false, stock: 99);

        Assert.AreEqual(LockReason.RequiresBuilding, status.Reason);
        Assert.AreEqual("Mill", status.RequiredBuildingId);
        Assert.AreEqual(20, status.RequiredReputation);
    }

    [Test]
    public void ALockedPage_StillReportsAffordabilityHonestly()
    {
        var status = Status(GatedByBuilding(), unlocked: false, stock: 0);

        Assert.IsFalse(status.CanAfford);
    }

    [Test]
    public void StatusOf_ToleratesNullDelegates()
    {
        var status = RecipeBookRules.StatusOf(new BookPage(1, Simple()), null, null);

        Assert.IsFalse(status.CanBrew);
    }
}
