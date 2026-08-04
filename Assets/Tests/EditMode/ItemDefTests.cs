using NUnit.Framework;

public class ItemDefTests
{
    [Test]
    public void Constructor_AssignsAllFields()
    {
        var item = new ItemDef("test_id", "Test Item", false, 42, true);

        Assert.AreEqual("test_id", item.id);
        Assert.AreEqual("Test Item", item.displayName);
        Assert.IsFalse(item.isIngredient);
        Assert.AreEqual(42, item.basePrice);
        Assert.IsTrue(item.isBottle);
    }

    [Test]
    public void Defaults_AreFalseOrZero()
    {
        var item = new ItemDef("id", "Name");

        Assert.IsTrue(item.isIngredient);
        Assert.AreEqual(0, item.basePrice);
        Assert.IsFalse(item.isBottle);
    }

    [Test]
    public void PlacementDefaults_AreFalseAndOneByOne()
    {
        var item = new ItemDef("id", "Name");

        Assert.IsFalse(item.isPlaceable);
        Assert.IsNull(item.placedPrefab);
        Assert.AreEqual(1, item.footprintWidth);
        Assert.AreEqual(1, item.footprintHeight);
    }

    [Test]
    public void Constructor_AssignsPlacementFields()
    {
        var item = new ItemDef("bench", "Bench", isPlaceable: true, footprintWidth: 2, footprintHeight: 1);

        Assert.IsTrue(item.isPlaceable);
        Assert.AreEqual(2, item.footprintWidth);
        Assert.AreEqual(1, item.footprintHeight);
    }
}
