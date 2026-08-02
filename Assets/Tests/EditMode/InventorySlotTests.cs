using NUnit.Framework;

public class InventorySlotTests
{
    [Test]
    public void DefaultSlot_IsEmpty()
    {
        var slot = new InventorySlot();
        Assert.IsNull(slot.Item);
        Assert.AreEqual(0, slot.Count);
        Assert.IsTrue(slot.IsEmpty);
        Assert.IsFalse(slot.IsFull);
    }

    [Test]
    public void MaxStack_Is30()
    {
        Assert.AreEqual(30, InventorySlot.MaxStack);
    }

    [Test]
    public void SlotWithItem_NotEmpty()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var slot = new InventorySlot { Item = item, Count = 5 };
        Assert.IsFalse(slot.IsEmpty);
        Assert.IsFalse(slot.IsFull);
    }

    [Test]
    public void SlotAtMaxStack_IsFull()
    {
        var item = new ItemDef("grain", "Grain", true, 5);
        var slot = new InventorySlot { Item = item, Count = 30 };
        Assert.IsTrue(slot.IsFull);
    }

    [Test]
    public void AddResult_Defaults()
    {
        var r = new AddResult();
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(0, r.Overflow);
    }

    [Test]
    public void DropResult_Defaults()
    {
        var r = new DropResult();
        Assert.IsFalse(r.Success);
        Assert.IsNull(r.Def);
        Assert.AreEqual(0, r.Count);
    }
}
