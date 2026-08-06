using UnityEngine;

public class FallenLog : MonoBehaviour, IInteractable, IForageable
{
    private const int SwingsRequired = 3;
    private const float SwingSeconds = 3f;

    private SpriteRenderer _spriteRenderer;
    private bool _harvested;
    private int _swingsDone;

    public InteractType InteractType => InteractType.Forage;
    public bool CanInteract => true;
    public bool IsHarvested => _harvested;

    public float SwingDuration => SwingSeconds;
    public ItemDef RequiredTool => ContentDb.HandAxe;
    public int SwingsDone => _swingsDone;
    public int SwingsNeeded => SwingsRequired;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void Interact() { }

    public void CompleteSwing()
    {
        if (_harvested) return;

        _swingsDone++;
        if (_swingsDone < SwingsRequired) return;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.TryAdd(ContentDb.Wood, 1);
        SetHarvested(true);
    }

    private void SetHarvested(bool harvested)
    {
        _harvested = harvested;
        _swingsDone = 0;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !harvested;
        foreach (var c in GetComponents<Collider2D>())
            c.enabled = !harvested;
    }
}
