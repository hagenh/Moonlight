using UnityEngine;

public class BerryBush : MonoBehaviour, IInteractable
{
    [SerializeField] private int berryYield = 1;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool IsHarvested => _harvested;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
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
        if (_collider != null)
            _collider.enabled = !harvested;
    }

    public static BerryBush Create(Vector3 position)
    {
        var go = new GameObject("BerryBush");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.6f, 0.2f, 0.7f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var bush = go.AddComponent<BerryBush>();
        bush._spriteRenderer = sr;
        bush._collider = col;

        return bush;
    }
}
