using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Inventory _inventory = new();

    public IReadOnlyDictionary<ItemDef, int> AllItems => _inventory.AllItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.Day == 1)
            TryAdd(ContentDb.Berry, 3);
    }

    public int GetCount(ItemDef def)
    {
        return _inventory.GetCount(def);
    }

    public bool Has(ItemDef def, int count)
    {
        return _inventory.Has(def, count);
    }

    public bool TryAdd(ItemDef def, int count)
    {
        var r = _inventory.TryAdd(def, count);
        if (!r.Success) return false;
        GameEvents.OnInventoryChanged(def, r.OldCount, r.NewCount);
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
        var r = _inventory.TryRemove(def, count);
        GameEvents.OnInventoryChanged(def, r.OldCount, r.NewCount);
        return true;
    }
}
