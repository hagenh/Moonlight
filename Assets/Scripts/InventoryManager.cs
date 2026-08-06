using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private readonly Inventory _inventory = new();

    public const int HotbarSlotCount = 9;

    public IReadOnlyList<InventorySlot> Slots => _inventory.Slots;
    public int ActiveSlotIndex { get; private set; }

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
        {
            TryAdd(ContentDb.Berry, 3);
            TryAdd(ContentDb.Pickaxe, 1);
            TryAdd(ContentDb.HandAxe, 1);
        }
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

        if (r.Added > 0)
        {
            int oldCount = GetCount(def) - r.Added;
            GameEvents.OnInventoryChanged(def, oldCount, GetCount(def));
            GameEvents.OnToastRequested($"+{r.Added} {def.displayName}");
        }

        if (r.Overflow > 0)
            GameEvents.OnInventoryFull(def, r.Overflow);

        return r.Success;
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

            Vector3 spawnPos = PlayerController.Instance != null
                ? PlayerController.Instance.transform.position + new Vector3(0.5f, 0f, 0f)
                : Vector3.zero;
            DroppedItem.Create(r.Def, r.Count, spawnPos);
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
        }

        if (r.Overflow > 0)
            GameEvents.OnInventoryFull(def, r.Overflow);

        return r;
    }

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= HotbarSlotCount) return;
        if (index == ActiveSlotIndex) return;
        ActiveSlotIndex = index;
        GameEvents.OnActiveSlotChanged(index);
    }

    public Dictionary<ItemDef, int> GetAllItems()
    {
        return _inventory.GetAllItems();
    }
}
