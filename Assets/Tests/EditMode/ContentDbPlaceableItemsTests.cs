using NUnit.Framework;

public class ContentDbPlaceableItemsTests
{
    [Test]
    public void Lamppost_IsPlaceable_OneByOne()
    {
        Assert.IsTrue(ContentDb.Lamppost.isPlaceable);
        Assert.AreEqual(1, ContentDb.Lamppost.footprintWidth);
        Assert.AreEqual(1, ContentDb.Lamppost.footprintHeight);
    }

    [Test]
    public void PlankSidewalk_IsPlaceable_OneByOne()
    {
        Assert.IsTrue(ContentDb.PlankSidewalk.isPlaceable);
        Assert.AreEqual(1, ContentDb.PlankSidewalk.footprintWidth);
        Assert.AreEqual(1, ContentDb.PlankSidewalk.footprintHeight);
    }

    [Test]
    public void Bench_IsPlaceable_TwoByOne()
    {
        Assert.IsTrue(ContentDb.Bench.isPlaceable);
        Assert.AreEqual(2, ContentDb.Bench.footprintWidth);
        Assert.AreEqual(1, ContentDb.Bench.footprintHeight);
    }

    [Test]
    public void FlowerBox_IsPlaceable_OneByOne()
    {
        Assert.IsTrue(ContentDb.FlowerBox.isPlaceable);
        Assert.AreEqual(1, ContentDb.FlowerBox.footprintWidth);
        Assert.AreEqual(1, ContentDb.FlowerBox.footprintHeight);
    }

    [Test]
    public void Sign_IsPlaceable_OneByOne()
    {
        Assert.IsTrue(ContentDb.Sign.isPlaceable);
        Assert.AreEqual(1, ContentDb.Sign.footprintWidth);
        Assert.AreEqual(1, ContentDb.Sign.footprintHeight);
    }
}
