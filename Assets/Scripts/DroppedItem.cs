using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    public ItemDef Item { get; private set; }
    public int Count { get; private set; }

    public InteractType InteractType => InteractType.DroppedItem;
    public bool CanInteract => Item != null && Count > 0;

    public void Interact()
    {
        if (!CanInteract || InventoryManager.Instance == null) return;

        var r = InventoryManager.Instance.TryAddPartial(Item, Count);
        Count -= r.Added;

        if (Count <= 0)
            Destroy(gameObject);
    }

    public static DroppedItem Create(ItemDef item, int count, Vector3 position)
    {
        var go = new GameObject($"DroppedItem_{item.id}");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        if (item.icon != null)
            sr.sprite = item.icon;
        sr.sortingOrder = -1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        var di = go.AddComponent<DroppedItem>();
        di.Item = item;
        di.Count = count;

        return di;
    }
}
