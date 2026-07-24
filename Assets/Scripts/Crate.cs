using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    public ItemDef item;
    public int count;

    public InteractType InteractType => InteractType.Crate;

    public Sprite bottomSprite;
    public Sprite topSprite;

    public static Crate Create(ItemDef item, int count, Vector3 position)
    {
        var go = new GameObject($"Crate_{item.id}");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var top = new GameObject("CrateTop");
        top.transform.SetParent(go.transform, false);
        top.transform.localPosition = new Vector3(0, 1f, 0);
        var topSr = top.AddComponent<SpriteRenderer>();
        topSr.sortingOrder = 5;

        if (ContentDb.Instance != null && ContentDb.Instance.CrateBottom != null)
        {
            sr.sprite = ContentDb.Instance.CrateBottom;
            topSr.sprite = ContentDb.Instance.CrateTop;
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var crate = go.AddComponent<Crate>();
        crate.item = item;
        crate.count = count;
        return crate;
    }

    public void Interact()
    {
        if (PlayerController.Instance == null) return;

        if (PlayerController.Instance.IsCarryingAnything)
        {
            GameEvents.OnToastRequested("Already carrying something");
            return;
        }

        PlayerController.Instance.PickUpCrate(this);
        gameObject.SetActive(false);
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }
}
