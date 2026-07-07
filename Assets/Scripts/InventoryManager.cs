using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Dictionary<ItemDef, int> _items = new();

    public IReadOnlyDictionary<ItemDef, int> AllItems => _items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetCount(ItemDef def)
    {
        if (def == null) return 0;
        return _items.GetValueOrDefault(def, 0);
    }

    public bool Has(ItemDef def, int count)
    {
        return GetCount(def) >= count;
    }

    public bool TryAdd(ItemDef def, int count)
    {
        if (def == null || count <= 0) return false;
        int old = GetCount(def);
        _items[def] = old + count;
        GameEvents.OnInventoryChanged(def, old, _items[def]);
        GameEvents.OnToastRequested($"+{count} {def.displayName}");
        return true;
    }

    public bool TryRemove(ItemDef def, int count)
    {
        if (def == null || count <= 0) return false;
        int current = GetCount(def);
        if (current < count)
        {
            GameEvents.OnToastRequested($"Not enough {def.displayName}");
            return false;
        }
        int old = current;
        _items[def] = current - count;
        if (_items[def] <= 0) _items.Remove(def);
        int newVal = _items.GetValueOrDefault(def, 0);
        GameEvents.OnInventoryChanged(def, old, newVal);
        return true;
    }
}
