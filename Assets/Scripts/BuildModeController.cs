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
        _ghostRenderer.transform.localScale = FootprintScaleFor(item);
        _ghostRenderer.gameObject.SetActive(true);
    }

    private Vector3 FootprintScaleFor(ItemDef item)
    {
        if (item.icon == null || PlacementGrid.Instance == null) return Vector3.one;

        Vector3 iconSize = item.icon.bounds.size;
        if (iconSize.x <= 0f || iconSize.y <= 0f) return Vector3.one;

        Vector3 cellSize = PlacementGrid.Instance.CellSize;
        return new Vector3(
            item.footprintWidth * cellSize.x / iconSize.x,
            item.footprintHeight * cellSize.y / iconSize.y,
            1f);
    }

    private Vector3 FootprintCenterWorld(Vector3Int origin, int width, int height)
    {
        Vector3 cellSize = PlacementGrid.Instance.CellSize;
        Vector3 originCenter = PlacementGrid.Instance.CellCenterWorld(origin);
        return originCenter + new Vector3((width - 1) * cellSize.x / 2f, (height - 1) * cellSize.y / 2f, 0f);
    }

    private static bool IsPlayerMenuOpen() =>
        PlayerController.Instance != null && PlayerController.Instance.IsMenuOpen;

    public void Cancel()
    {
        IsActive = false;
        CurrentItem = null;
        _ghostRenderer.gameObject.SetActive(false);
    }

    public bool TryConfirmAt(Vector3Int cell)
    {
        if (!IsActive || CurrentItem == null) return false;
        if (IsPlayerMenuOpen()) return false;
        if (PlacementGrid.Instance == null || InfrastructureManager.Instance == null) return false;

        int width = CurrentItem.footprintWidth;
        int height = CurrentItem.footprintHeight;
        if (!PlacementGrid.Instance.IsAreaFree(cell, width, height)) return false;
        if (!InfrastructureManager.Instance.TryConsume(CurrentItem)) return false;

        Vector3 worldPos = FootprintCenterWorld(cell, width, height);
        GameObject instanceGo = CurrentItem.placedPrefab != null
            ? Instantiate(CurrentItem.placedPrefab, worldPos, Quaternion.identity)
            : new GameObject(CurrentItem.displayName);
        instanceGo.transform.position = worldPos;
        instanceGo.transform.localScale = FootprintScaleFor(CurrentItem);

        var marker = instanceGo.GetComponent<PlacedInfrastructure>();
        if (marker == null) marker = instanceGo.AddComponent<PlacedInfrastructure>();

        PlacementGrid.Instance.Reserve(cell, width, height, marker);

        Cancel();
        return true;
    }

    private void Update()
    {
        if (IsPlayerMenuOpen()) return;
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
        int width = CurrentItem.footprintWidth;
        int height = CurrentItem.footprintHeight;
        _ghostRenderer.transform.position = FootprintCenterWorld(cell, width, height);
        bool free = PlacementGrid.Instance.IsAreaFree(cell, width, height);
        _ghostRenderer.color = free ? validColor : invalidColor;
    }
}
