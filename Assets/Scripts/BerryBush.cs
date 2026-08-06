using UnityEngine;

public class BerryBush : MonoBehaviour, IInteractable
{
    [SerializeField] private int berryYield = 1;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _triggerCollider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool CanInteract => true;
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
        GameEvents.HourChanged += OnHourChanged;
    }

    private void OnDisable()
    {
        GameEvents.HourChanged -= OnHourChanged;
    }

    private void OnHourChanged(int hour, int day)
    {
        if (_harvested && hour >= 12)
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


}
