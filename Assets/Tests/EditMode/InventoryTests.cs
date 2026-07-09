using NUnit.Framework;

public class InventoryTests
{
    private Inventory _inventory;
    private ItemDef _item;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Inventory();
        _item = new ItemDef("grain", "Grain", true, 5);
    }

    [Test]
    public void TryAdd_Null_ReturnsFalse()
    {
        var r = _inventory.TryAdd(null, 5);
        Assert.IsFalse(r.Success);
    }

    [Test]
    public void TryAdd_ZeroOrNegative_ReturnsFalse()
    {
        var r1 = _inventory.TryAdd(_item, 0);
        var r2 = _inventory.TryAdd(_item, -3);
        Assert.IsFalse(r1.Success);
        Assert.IsFalse(r2.Success);
    }

    [Test]
    public void TryAdd_Stacks()
    {
        _inventory.TryAdd(_item, 5);
        var r = _inventory.TryAdd(_item, 3);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.OldCount);
        Assert.AreEqual(8, r.NewCount);
    }

    [Test]
    public void TryRemove_Insufficient_ReturnsFalse()
    {
        _inventory.TryAdd(_item, 3);
        var r = _inventory.TryRemove(_item, 5);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(3, r.OldCount);
        Assert.AreEqual(3, r.NewCount);
    }

    [Test]
    public void TryRemove_AutoRemovesZeroEntry()
    {
        _inventory.TryAdd(_item, 5);
        _inventory.TryRemove(_item, 5);
        Assert.IsFalse(_inventory.AllItems.ContainsKey(_item));
        Assert.AreEqual(0, _inventory.GetCount(_item));
    }

    [Test]
    public void TryRemove_Null_ReturnsFalse()
    {
        var r = _inventory.TryRemove(null, 5);
        Assert.IsFalse(r.Success);
    }

    [Test]
    public void GetCount_NullDef_ReturnsZero()
    {
        Assert.AreEqual(0, _inventory.GetCount(null));
    }

    [Test]
    public void Has_RespectsCount()
    {
        _inventory.TryAdd(_item, 3);
        Assert.IsTrue(_inventory.Has(_item, 3));
        Assert.IsFalse(_inventory.Has(_item, 4));
    }
}
