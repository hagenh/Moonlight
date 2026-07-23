using UnityEngine;

public class BerryBush : MonoBehaviour, IInteractable
{
    [SerializeField] private int berryYield = 1;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _triggerCollider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool IsHarvested => _harvested;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
    }

    private void OnDayEnded(int day)
    {
        if (_harvested)
            SetHarvested(false);
    }

    public void Interact()
    {
        if (_harvested) return;
        if (InventoryManager.Instance == null) return;

        InventoryManager.Instance.TryAdd(ContentDb.Berry, berryYield);
        SetHarvested(true);
    }

    private void SetHarvested(bool harvested)
    {
        _harvested = harvested;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !harvested;
        foreach (var c in GetComponents<Collider2D>())
            c.enabled = !harvested;
    }

    public static BerryBush Create(Vector3 position)
    {
        var go = new GameObject("BerryBush");
        go.transform.position = position;

        var tex = new Texture2D(16, 16);
        var pixels = new Color32[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        sr.color = new Color(0.6f, 0.2f, 0.7f);
        sr.sortingOrder = 5;

        var solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(0.5f, 0.5f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var bush = go.AddComponent<BerryBush>();
        bush._spriteRenderer = sr;
        bush._triggerCollider = col;

        return bush;
    }
}
