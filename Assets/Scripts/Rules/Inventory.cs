using System.Collections.Generic;

public readonly struct MutResult
{
    public readonly bool Success;
    public readonly int OldCount;
    public readonly int NewCount;

    public MutResult(bool success, int oldCount, int newCount)
    {
        Success = success;
        OldCount = oldCount;
        NewCount = newCount;
    }
}

public sealed class Inventory
{
    private readonly Dictionary<ItemDef, int> _items = new();

    public IReadOnlyDictionary<ItemDef, int> AllItems => _items;

    public int GetCount(ItemDef def)
    {
        if (def == null) return 0;
        return _items.GetValueOrDefault(def, 0);
    }

    public bool Has(ItemDef def, int count)
    {
        return GetCount(def) >= count;
    }

    public MutResult TryAdd(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return new MutResult(false, 0, 0);

        int old = GetCount(def);
        _items[def] = old + count;
        return new MutResult(true, old, _items[def]);
    }

    public MutResult TryRemove(ItemDef def, int count)
    {
        if (def == null || count <= 0)
            return new MutResult(false, 0, 0);

        int current = GetCount(def);
        if (current < count)
            return new MutResult(false, current, current);

        int old = current;
        _items[def] = current - count;
        if (_items[def] <= 0) _items.Remove(def);
        int newVal = _items.GetValueOrDefault(def, 0);
        return new MutResult(true, old, newVal);
    }
}
