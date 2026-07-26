using System.Collections.Generic;
using NUnit.Framework;

public class RequestBookRulesTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    private static readonly ItemDef Sweet = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);

    private static StandRequest Exact(ItemDef item, int units) =>
        new StandRequest("r1", RequestKind.Exact, new List<ItemDef> { item }, units, "A carter", "Four jars, if you have them.");

    private static StandRequest Descriptive(IEnumerable<ItemDef> items, int units) =>
        new StandRequest("r2", RequestKind.Descriptive, new List<ItemDef>(items), units, "Berta", "Something strong. It's for a wedding.");

    [Test]
    public void Payment_ExactRequest_AppliesExactMultiplier()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(108, RequestBookRules.Payment(request, Shine));
    }

    [Test]
    public void Payment_DescriptiveRequest_AppliesDescriptiveMultiplier()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.AreEqual(176, RequestBookRules.Payment(request, Sweet));
    }

    [Test]
    public void Payment_DescriptiveRequest_PricesWhatWasActuallyDelivered()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.AreEqual(66, RequestBookRules.Payment(request, Shine));
    }

    [Test]
    public void Payment_ItemNotAccepted_ReturnsZero()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(0, RequestBookRules.Payment(request, Sweet));
    }

    [Test]
    public void Payment_NullItem_ReturnsZero()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(0, RequestBookRules.Payment(request, null));
    }

    [Test]
    public void Payment_AlwaysBeatsShelfPrice()
    {
        var request = Exact(Shine, 4);
        int shelf = Shine.basePrice * 4;

        Assert.Greater(RequestBookRules.Payment(request, Shine), shelf);
    }

    [Test]
    public void Accepts_ExactRequest_OnlyTheNamedItem()
    {
        var request = Exact(Shine, 4);

        Assert.IsTrue(RequestBookRules.Accepts(request, Shine));
        Assert.IsFalse(RequestBookRules.Accepts(request, Sweet));
    }

    [Test]
    public void Accepts_DescriptiveRequest_AnyListedItem()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.IsTrue(RequestBookRules.Accepts(request, Shine));
        Assert.IsTrue(RequestBookRules.Accepts(request, Sweet));
    }

    [Test]
    public void Accepts_NullRequest_ReturnsFalse()
    {
        Assert.IsFalse(RequestBookRules.Accepts(null, Shine));
    }
}
