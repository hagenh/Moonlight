using NUnit.Framework;

public class RenovationRulesTests
{
    [Test]
    public void CanPurchase_OnlyAbandoned()
    {
        Assert.IsTrue(RenovationRules.CanPurchase(BuildingState.Abandoned));
        Assert.IsFalse(RenovationRules.CanPurchase(BuildingState.Purchased));
        Assert.IsFalse(RenovationRules.CanPurchase(BuildingState.Cleared));
        Assert.IsFalse(RenovationRules.CanPurchase(BuildingState.Restored));
    }

    [Test]
    public void CanSmash_OnlyPurchasedAndNotBoardsSmashed()
    {
        Assert.IsTrue(RenovationRules.CanSmash(BuildingState.Purchased, false));
        Assert.IsFalse(RenovationRules.CanSmash(BuildingState.Purchased, true));
        Assert.IsFalse(RenovationRules.CanSmash(BuildingState.Abandoned, false));
        Assert.IsFalse(RenovationRules.CanSmash(BuildingState.Cleared, false));
    }

    [Test]
    public void IsSmashComplete_TrueWhenDoneAtOrAboveRequired()
    {
        Assert.IsFalse(RenovationRules.IsSmashComplete(2, 3));
        Assert.IsTrue(RenovationRules.IsSmashComplete(3, 3));
        Assert.IsTrue(RenovationRules.IsSmashComplete(4, 3));
    }

    [Test]
    public void CanHammer_TrueWhenClearedWithMaterialsAndRemaining()
    {
        Assert.IsTrue(RenovationRules.CanHammer(BuildingState.Cleared, 1, 3, true, true));
    }

    [Test]
    public void CanHammer_FalseWhenNoMaterials()
    {
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Cleared, 1, 3, false, true));
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Cleared, 1, 3, true, false));
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Cleared, 1, 3, false, false));
    }

    [Test]
    public void CanHammer_FalseWhenStateNotCleared()
    {
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Abandoned, 0, 3, true, true));
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Purchased, 0, 3, true, true));
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Restored, 0, 3, true, true));
    }

    [Test]
    public void CanHammer_FalseWhenAllRepairPointsDone()
    {
        Assert.IsFalse(RenovationRules.CanHammer(BuildingState.Cleared, 3, 3, true, true));
    }

    [Test]
    public void IsRepairComplete_TrueWhenDoneAtOrAboveTotal()
    {
        Assert.IsFalse(RenovationRules.IsRepairComplete(2, 3));
        Assert.IsTrue(RenovationRules.IsRepairComplete(3, 3));
        Assert.IsTrue(RenovationRules.IsRepairComplete(4, 3));
    }

    [Test]
    public void CanCollectIncome_OnlyWhenRestoredAndHasIncome()
    {
        Assert.IsTrue(RenovationRules.CanCollectIncome(BuildingState.Restored, 20));
        Assert.IsFalse(RenovationRules.CanCollectIncome(BuildingState.Restored, 0));
        Assert.IsFalse(RenovationRules.CanCollectIncome(BuildingState.Cleared, 20));
        Assert.IsFalse(RenovationRules.CanCollectIncome(BuildingState.Abandoned, 20));
    }

    [Test]
    public void IsDebrisCleared_TrueWhenZeroOrLess()
    {
        Assert.IsTrue(RenovationRules.IsDebrisCleared(0));
        Assert.IsTrue(RenovationRules.IsDebrisCleared(-1));
        Assert.IsFalse(RenovationRules.IsDebrisCleared(1));
        Assert.IsFalse(RenovationRules.IsDebrisCleared(3));
    }
}
