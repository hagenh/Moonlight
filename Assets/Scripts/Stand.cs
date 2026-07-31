using UnityEngine;

public class Stand : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Stand;
    public bool CanInteract => true;

    public void Interact()
    {
        GameEvents.OnRequestBookRequested();
    }

    public static Stand Create(Vector3 position)
    {
        var go = new GameObject("Stand");
        go.transform.position = position;

        var tex = new Texture2D(16, 16);
        var pixels = new Color32[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        sr.color = new Color(0.75f, 0.6f, 0.35f);
        sr.sortingOrder = 5;

        var solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(1.2f, 0.6f);

        var trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.6f, 1.2f);

        go.layer = LayerMask.NameToLayer("Interactable");

        return go.AddComponent<Stand>();
    }
}
