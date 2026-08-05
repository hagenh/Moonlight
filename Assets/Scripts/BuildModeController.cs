using UnityEngine;
using UnityEngine.InputSystem;

public class BuildModeController : MonoBehaviour
{
    public static BuildModeController Instance { get; private set; }

    [SerializeField] private Color validColor = new Color(0.4f, 1f, 0.4f, 0.6f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.4f, 0.4f, 0.6f);

    private SpriteRenderer _ghostRenderer;

    public bool IsActive { get; private set; }
    public ItemDef CurrentItem { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var ghostGo = new GameObject("BuildGhost");
        ghostGo.transform.SetParent(transform);
        _ghostRenderer = ghostGo.AddComponent<SpriteRenderer>();
        _ghostRenderer.sortingOrder = 100;
        ghostGo.SetActive(false);
    }

    public void Enter(ItemDef item)
    {
        if (item == null || !item.isPlaceable) return;

        CurrentItem = item;
        IsActive = true;
        _ghostRenderer.sprite = item.icon;
        _ghostRenderer.gameObject.SetActive(true);
    }

    public void Cancel()
    {
        IsActive = false;
        CurrentItem = null;
        _ghostRenderer.gameObject.SetActive(false);
    }

    public bool TryConfirmAt(Vector3Int cell)
    {
        if (!IsActive || CurrentItem == null) return false;
        if (PlacementGrid.Instance == null || InfrastructureManager.Instance == null) return false;

        int width = CurrentItem.footprintWidth;
        int height = CurrentItem.footprintHeight;
        if (!PlacementGrid.Instance.IsAreaFree(cell, width, height)) return false;
        if (!InfrastructureManager.Instance.TryConsume(CurrentItem)) return false;

        Vector3 worldPos = PlacementGrid.Instance.CellCenterWorld(cell);
        GameObject instanceGo = CurrentItem.placedPrefab != null
            ? Instantiate(CurrentItem.placedPrefab, worldPos, Quaternion.identity)
            : new GameObject(CurrentItem.displayName);
        instanceGo.transform.position = worldPos;

        var marker = instanceGo.GetComponent<PlacedInfrastructure>();
        if (marker == null) marker = instanceGo.AddComponent<PlacedInfrastructure>();

        PlacementGrid.Instance.Reserve(cell, width, height, marker);

        Cancel();
        return true;
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cancel();
            return;
        }
        if (Mouse.current == null || PlacementGrid.Instance == null) return;

        Vector3Int cell = PlacementGrid.Instance.WorldToCell(MouseWorldPosition());
        UpdateGhost(cell);

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cancel();
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryConfirmAt(cell);
    }

    private Vector3 MouseWorldPosition()
    {
        var cam = Camera.main;
        if (cam == null) return transform.position;

        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 screenPoint = new Vector3(screen.x, screen.y, -cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(screenPoint);
        world.z = 0f;
        return world;
    }

    private void UpdateGhost(Vector3Int cell)
    {
        _ghostRenderer.transform.position = PlacementGrid.Instance.CellCenterWorld(cell);
        bool free = PlacementGrid.Instance.IsAreaFree(cell, CurrentItem.footprintWidth, CurrentItem.footprintHeight);
        _ghostRenderer.color = free ? validColor : invalidColor;
    }
}
