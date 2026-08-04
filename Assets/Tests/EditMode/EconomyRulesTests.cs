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
    public void GetSellPrice_IsBasePrice()
    {
        Assert.AreEqual(25, EconomyRules.GetSellPrice(_moonshine));
        Assert.AreEqual(5, EconomyRules.GetSellPrice(_grain));
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
    public void GetDeliveryPrice_Backwoods_Is1_5x()
    {
        Assert.AreEqual(38, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Backwoods));
    }

    [Test]
    public void GetDeliveryPrice_Cart_IsBasePrice()
    {
        Assert.AreEqual(25, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart));
    }

    [Test]
    public void IsSellable_CartBuysBerryShine()
    {
        var berryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
        Assert.IsTrue(EconomyRules.IsSellable(berryShine, SellerType.TravelingCart));
    }
}
