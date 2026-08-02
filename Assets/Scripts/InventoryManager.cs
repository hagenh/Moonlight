using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Inventory _inventory = new();

    public IReadOnlyList<InventorySlot> Slots => _inventory.Slots;

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

        int oldCount = GetCount(def) - r.Added;
        GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
        GameEvents.OnToastRequested($"+{r.Added} {def.displayName}");

        if (r.Overflow > 0)
            GameEvents.OnInventoryFull(def, r.Overflow);

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
        int oldCount = current;
        _inventory.TryRemove(def, count);
        GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
        return true;
    }

    public DropResult TryDropFromSlot(int slotIndex, int count)
    {
        var r = _inventory.TryDropFromSlot(slotIndex, count);
        if (r.Success)
        {
            GameEvents.OnItemDropped(slotIndex, r.Def, r.Count);
            GameEvents.OnInventoryChanged(r.Def, GetCount(r.Def) + r.Count, GetCount(r.Def));
        }
        return r;
    }

    public AddResult TryAddPartial(ItemDef def, int count)
    {
        var r = _inventory.TryAdd(def, count);
        if (r.Added > 0)
        {
            int oldCount = GetCount(def) - r.Added;
            GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
            GameEvents.OnToastRequested($"+{r.Added} {def.displayName}");

            if (r.Overflow > 0)
                GameEvents.OnInventoryFull(def, r.Overflow);
        }
        return r;
    }

    public Dictionary<ItemDef, int> GetAllItems()
    {
        return _inventory.GetAllItems();
    }
}
