using UnityEngine;

public class StonePile : MonoBehaviour, IInteractable
{
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

        InventoryManager.Instance.TryAdd(ContentDb.Stone, 1);
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
