using System.Collections.Generic;

public sealed class Inventory
{
    private readonly InventorySlot[] _slots;
    public const int SlotCount = 20;

    public Inventory()
    {
        _slots = new InventorySlot[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            _slots[i] = new InventorySlot();
    }

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public int GetCount(ItemDef def)
    {
        if (def == null) return 0;
        int total = 0;
        for (int i = 0; i < SlotCount; i++)
            if (_slots[i].Item == def)
                total += _slots[i].Count;
        return total;
    }

    public bool Has(ItemDef def, int count)
    {
        return GetCount(def) >= count;
    }

    public AddResult TryAdd(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return new AddResult(false, 0, 0);

        int remaining = count;

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].Item == def && !_slots[i].IsFull)
            {
                int space = InventorySlot.MaxStack - _slots[i].Count;
                int add = System.Math.Min(space, remaining);
                _slots[i].Count += add;
                remaining -= add;
            }
        }

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].IsEmpty)
            {
                int add = System.Math.Min(InventorySlot.MaxStack, remaining);
                _slots[i].Item = def;
                _slots[i].Count = add;
                remaining -= add;
            }
        }

        int added = count - remaining;
        return new AddResult(added > 0, added, remaining);
    }

    public bool TryRemove(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return false;

        if (GetCount(def) < count)
            return false;

        int remaining = count;

        for (int i = 0; i < SlotCount && remaining > 0; i++)
        {
            if (_slots[i].Item == def)
            {
                int remove = System.Math.Min(_slots[i].Count, remaining);
                _slots[i].Count -= remove;
                remaining -= remove;

                if (_slots[i].Count <= 0)
                {
                    _slots[i].Item = null;
                    _slots[i].Count = 0;
                }
            }
        }

        return remaining == 0;
    }

    public DropResult TryDropFromSlot(int slotIndex, int count)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
            return new DropResult(false, null, 0);

        var slot = _slots[slotIndex];
        if (slot.IsEmpty || count <= 0)
            return new DropResult(false, null, 0);

        int actual = System.Math.Min(count, slot.Count);
        ItemDef dropped = slot.Item;
        slot.Count -= actual;

        if (slot.Count <= 0)
        {
            slot.Item = null;
            slot.Count = 0;
        }

        return new DropResult(true, dropped, actual);
    }

    public int FirstEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
            if (_slots[i].IsEmpty)
                return i;
        return -1;
    }

    public Dictionary<ItemDef, int> GetAllItems()
    {
        var result = new Dictionary<ItemDef, int>();
        for (int i = 0; i < SlotCount; i++)
        {
            if (!_slots[i].IsEmpty)
            {
                if (result.ContainsKey(_slots[i].Item))
                    result[_slots[i].Item] += _slots[i].Count;
                else
                    result[_slots[i].Item] = _slots[i].Count;
            }
        }
        return result;
    }
}
