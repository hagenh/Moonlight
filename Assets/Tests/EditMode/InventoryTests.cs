using System.Collections.Generic;
using NUnit.Framework;

public class InventoryTests
{
    private Inventory _inventory;
    private ItemDef _grain;
    private ItemDef _sugar;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Inventory();
        _grain = new ItemDef("grain", "Grain", true, 5);
        _sugar = new ItemDef("sugar", "Sugar", true, 5);
    }

    [Test]
    public void TryAdd_Null_ReturnsFailure()
    {
        var r = _inventory.TryAdd(null, 5);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
    }

    [Test]
    public void TryAdd_ZeroOrNegative_ReturnsFailure()
    {
        var r1 = _inventory.TryAdd(_grain, 0);
        var r2 = _inventory.TryAdd(_grain, -3);
        Assert.IsFalse(r1.Success);
        Assert.IsFalse(r2.Success);
    }

    [Test]
    public void TryAdd_FillsEmptySlot()
    {
        var r = _inventory.TryAdd(_grain, 5);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.Added);
        Assert.AreEqual(0, r.Overflow);
        Assert.AreEqual(5, _inventory.GetCount(_grain));
    }

    [Test]
    public void TryAdd_StacksOntoExistingPartialSlot()
    {
        _inventory.TryAdd(_grain, 25);
        var r = _inventory.TryAdd(_grain, 10);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(10, r.Added);
        Assert.AreEqual(0, r.Overflow);
        Assert.AreEqual(35, _inventory.GetCount(_grain));
        Assert.AreEqual(30, _inventory.Slots[0].Count);
        Assert.AreEqual(5, _inventory.Slots[1].Count);
    }

    [Test]
    public void TryAdd_FillsMultipleSlots()
    {
        var r = _inventory.TryAdd(_grain, 45);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(45, r.Added);
        Assert.AreEqual(0, r.Overflow);
        Assert.AreEqual(45, _inventory.GetCount(_grain));
        Assert.AreEqual(_grain, _inventory.Slots[0].Item);
        Assert.AreEqual(30, _inventory.Slots[0].Count);
        Assert.AreEqual(_grain, _inventory.Slots[1].Item);
        Assert.AreEqual(15, _inventory.Slots[1].Count);
    }

    [Test]
    public void TryAdd_OverflowWhenAllSlotsFull()
    {
        _inventory.TryAdd(_grain, 30);
        for (int i = 0; i < 19; i++)
            _inventory.TryAdd(_sugar, 30);

        var r = _inventory.TryAdd(_grain, 5);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(5, r.Overflow);
    }

    [Test]
    public void TryAdd_CompletelyFull_NoRoomAtAll()
    {
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(_grain, 30);

        var r = _inventory.TryAdd(_sugar, 1);
        Assert.IsFalse(r.Success);
        Assert.AreEqual(0, r.Added);
        Assert.AreEqual(1, r.Overflow);
    }

    [Test]
    public void TryRemove_Insufficient_ReturnsFalse()
    {
        _inventory.TryAdd(_grain, 3);
        bool result = _inventory.TryRemove(_grain, 5);
        Assert.IsFalse(result);
        Assert.AreEqual(3, _inventory.GetCount(_grain));
    }

    [Test]
    public void TryRemove_AcrossMultipleSlots()
    {
        _inventory.TryAdd(_grain, 45);
        bool result = _inventory.TryRemove(_grain, 35);
        Assert.IsTrue(result);
        Assert.AreEqual(10, _inventory.GetCount(_grain));
        Assert.AreEqual(10, _inventory.Slots[1].Count);
    }

    [Test]
    public void TryRemove_ToZero_ClearsSlot()
    {
        _inventory.TryAdd(_grain, 5);
        _inventory.TryRemove(_grain, 5);
        Assert.AreEqual(0, _inventory.GetCount(_grain));
        Assert.IsNull(_inventory.Slots[0].Item);
    }

    [Test]
    public void TryRemove_Null_ReturnsFalse()
    {
        bool result = _inventory.TryRemove(null, 5);
        Assert.IsFalse(result);
    }

    [Test]
    public void TryDropFromSlot_Valid_RemovesAndReturnsResult()
    {
        _inventory.TryAdd(_grain, 10);
        var r = _inventory.TryDropFromSlot(0, 3);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(_grain, r.Def);
        Assert.AreEqual(3, r.Count);
        Assert.AreEqual(7, _inventory.Slots[0].Count);
    }

    [Test]
    public void TryDropFromSlot_DropAll_ClearsSlot()
    {
        _inventory.TryAdd(_grain, 5);
        var r = _inventory.TryDropFromSlot(0, 5);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(5, r.Count);
        Assert.IsNull(_inventory.Slots[0].Item);
    }

    [Test]
    public void TryDropFromSlot_InvalidIndex_ReturnsFailure()
    {
        var r = _inventory.TryDropFromSlot(-1, 1);
        Assert.IsFalse(r.Success);
        var r2 = _inventory.TryDropFromSlot(20, 1);
        Assert.IsFalse(r2.Success);
    }

    [Test]
    public void TryDropFromSlot_EmptySlot_ReturnsFailure()
    {
        var r = _inventory.TryDropFromSlot(0, 1);
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
        _inventory.TryAdd(_grain, 3);
        Assert.IsTrue(_inventory.Has(_grain, 3));
        Assert.IsFalse(_inventory.Has(_grain, 4));
    }

    [Test]
    public void FirstEmptySlot_ReturnsFirstEmpty()
    {
        Assert.AreEqual(0, _inventory.FirstEmptySlot());
        _inventory.TryAdd(_grain, 30);
        Assert.AreEqual(1, _inventory.FirstEmptySlot());
    }

    [Test]
    public void FirstEmptySlot_Full_ReturnsMinusOne()
    {
        for (int i = 0; i < 20; i++)
            _inventory.TryAdd(_grain, 30);
        Assert.AreEqual(-1, _inventory.FirstEmptySlot());
    }

    [Test]
    public void GetAllItems_ComputesFromSlots()
    {
        _inventory.TryAdd(_grain, 10);
        _inventory.TryAdd(_sugar, 5);
        var all = _inventory.GetAllItems();
        Assert.AreEqual(10, all[_grain]);
        Assert.AreEqual(5, all[_sugar]);
        Assert.AreEqual(2, all.Count);
    }

    [Test]
    public void SlotCount_Is20()
    {
        Assert.AreEqual(20, Inventory.SlotCount);
        Assert.AreEqual(20, _inventory.Slots.Count);
    }
}
