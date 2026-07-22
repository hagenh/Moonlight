using NUnit.Framework;

public class EconomyRulesTests
{
    private ItemDef _moonshine;
    private ItemDef _grain;

    [SetUp]
    public void SetUp()
    {
        _moonshine = new ItemDef("moonshine", "Moonshine", false, 25, true);
        _grain = new ItemDef("grain", "Grain", true, 5);
    }

    [Test]
    public void GetSellPrice_RiskyBuyer_Is2x_Rounded()
    {
        int price = EconomyRules.GetSellPrice(_moonshine, SellerType.RiskyBuyer);
        Assert.AreEqual(50, price);
    }

    [Test]
    public void GetSellPrice_NonRisky_IsBasePrice()
    {
        Assert.AreEqual(25, EconomyRules.GetSellPrice(_moonshine, SellerType.Tormod));
        Assert.AreEqual(25, EconomyRules.GetSellPrice(_moonshine, SellerType.TravelingCart));
    }

    [Test]
    public void GetBuyPrice_EqualsBasePrice()
    {
        Assert.AreEqual(5, EconomyRules.GetBuyPrice(_grain));
        Assert.AreEqual(25, EconomyRules.GetBuyPrice(_moonshine));
    }

    [Test]
    public void IsCartDay_DayModuloThree()
    {
        Assert.IsFalse(EconomyRules.IsCartDay(3));
        Assert.IsFalse(EconomyRules.IsCartDay(6));
        Assert.IsFalse(EconomyRules.IsCartDay(9));
        Assert.IsTrue(EconomyRules.IsCartDay(1));
        Assert.IsTrue(EconomyRules.IsCartDay(2));
        Assert.IsTrue(EconomyRules.IsCartDay(4));
    }

    [Test]
    public void IsSellable_CartBuysBottles()
    {
        Assert.IsTrue(EconomyRules.IsSellable(_moonshine, SellerType.TravelingCart));
        Assert.IsFalse(EconomyRules.IsSellable(_grain, SellerType.TravelingCart));
    }

    [Test]
    public void IsSellable_OthersBuyNonIngredients()
    {
        Assert.IsTrue(EconomyRules.IsSellable(_moonshine, SellerType.Tormod));
        Assert.IsFalse(EconomyRules.IsSellable(_grain, SellerType.Tormod));
        Assert.IsTrue(EconomyRules.IsSellable(_moonshine, SellerType.RiskyBuyer));
        Assert.IsFalse(EconomyRules.IsSellable(_grain, SellerType.RiskyBuyer));
    }

    [Test]
    public void GetSuspicionTier_Boundaries()
    {
        Assert.AreEqual(EconomyRules.SuspicionTier.Clean, EconomyRules.GetSuspicionTier(0));
        Assert.AreEqual(EconomyRules.SuspicionTier.Clean, EconomyRules.GetSuspicionTier(20));
        Assert.AreEqual(EconomyRules.SuspicionTier.Noticed, EconomyRules.GetSuspicionTier(21));
        Assert.AreEqual(EconomyRules.SuspicionTier.Noticed, EconomyRules.GetSuspicionTier(40));
        Assert.AreEqual(EconomyRules.SuspicionTier.TalkedAbout, EconomyRules.GetSuspicionTier(41));
        Assert.AreEqual(EconomyRules.SuspicionTier.TalkedAbout, EconomyRules.GetSuspicionTier(60));
        Assert.AreEqual(EconomyRules.SuspicionTier.Burning, EconomyRules.GetSuspicionTier(61));
        Assert.AreEqual(EconomyRules.SuspicionTier.Burning, EconomyRules.GetSuspicionTier(100));
    }

    [Test]
    public void GetDeliveryPrice_Backwoods_Is1_5x()
    {
        Assert.AreEqual(38, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Backwoods, 0));
    }

    [Test]
    public void GetDeliveryPrice_Cart_Clean_Is1x()
    {
        Assert.AreEqual(25, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart, 0));
    }

    [Test]
    public void GetDeliveryPrice_Cart_Noticed_Is0_9x()
    {
        Assert.AreEqual(23, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart, 25));
    }

    [Test]
    public void GetDeliveryPrice_Cart_TalkedAbout_Refuses()
    {
        Assert.AreEqual(0, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart, 45));
    }

    [Test]
    public void GetDeliveryPrice_Cart_Burning_Refuses()
    {
        Assert.AreEqual(0, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart, 70));
    }

    [Test]
    public void GetGuardCount_ScalesWithSuspicion()
    {
        Assert.AreEqual(1, EconomyRules.GetGuardCountForSuspicion(0));
        Assert.AreEqual(2, EconomyRules.GetGuardCountForSuspicion(25));
        Assert.AreEqual(3, EconomyRules.GetGuardCountForSuspicion(45));
        Assert.AreEqual(4, EconomyRules.GetGuardCountForSuspicion(70));
    }
}
