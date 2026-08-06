using UnityEngine;

public class FallenLog : MonoBehaviour, IInteractable, IForageable
{
    private const float SwingSeconds = 1.5f;

    private SpriteRenderer _spriteRenderer;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool CanInteract => true;
    public bool IsHarvested => _harvested;

    public float SwingDuration => SwingSeconds;
    public ItemDef RequiredTool => ContentDb.HandAxe;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void Interact() { }

    public void CompleteSwing()
    {
        if (_harvested) return;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.TryAdd(ContentDb.Wood, 1);
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
