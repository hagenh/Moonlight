using UnityEngine;

public class Debris : MonoBehaviour, IInteractable
{
    public Building sourceBuilding;

    public InteractType InteractType => InteractType.Debris;

    public static Debris Create(Building source, Vector3 position)
    {
        var go = new GameObject("Debris");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.6f, 0.45f, 0.3f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        if (source.BoardTrigger != null)
            go.layer = source.BoardTrigger.gameObject.layer;
        else
            go.layer = LayerMask.NameToLayer("Interactable");

        var debris = go.AddComponent<Debris>();
        debris.sourceBuilding = source;
        return debris;
    }

    public void Interact()
    {
        if (PlayerController.Instance == null) return;

        if (PlayerController.Instance.IsCarrying)
        {
            GameEvents.OnToastRequested("Already carrying debris");
            return;
        }

        PlayerController.Instance.PickUpDebris(this);
        gameObject.SetActive(false);
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }
}
