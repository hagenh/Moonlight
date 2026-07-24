using UnityEngine;

public class Crate : MonoBehaviour, IInteractable
{
    public ItemDef item;
    public int count;

    public InteractType InteractType => InteractType.Crate;

    public static Crate Create(ItemDef item, int count, Vector3 position)
    {
        if (ContentDb.Instance != null && ContentDb.Instance.CratePrefab != null)
        {
            var go = Instantiate(ContentDb.Instance.CratePrefab, position, Quaternion.identity);
            go.name = $"Crate_{item.id}";
            var crate = go.GetComponent<Crate>();
            if (crate != null)
            {
                crate.item = item;
                crate.count = count;
            }
            return crate;
        }

        var fallback = new GameObject($"Crate_{item.id}");
        fallback.transform.position = position;
        var sr = fallback.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        sr.color = new Color(0.4f, 0.7f, 0.3f);
        sr.sortingOrder = 5;
        var col = fallback.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);
        fallback.layer = LayerMask.NameToLayer("Interactable");
        var fallbackCrate = fallback.AddComponent<Crate>();
        fallbackCrate.item = item;
        fallbackCrate.count = count;
        return fallbackCrate;
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
