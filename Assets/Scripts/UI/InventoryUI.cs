using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    private const int GridCols = 5;
    private const int GridRows = 4;
    private const int SlotCount = 20;
    private const float CellSize = 56f;
    private const float CellGap = 6f;
    private const float PanelWidth = 520f;
    private const float PanelHeight = 340f;

    private GameObject _root;
    private readonly List<InventorySlotView> _slotViews = new();
    private TMP_Text _detailName;
    private TMP_Text _detailType;
    private TMP_Text _detailPrice;
    private TMP_Text _detailStack;
    private TMP_Text _detailHint;
    private GameObject _detailPanel;
    private GameObject _emptyDetailPanel;
    private int _selectedSlot = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _root.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.InventoryOpened += OnInventoryOpened;
        GameEvents.InventoryChanged += OnInventoryChanged;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.InventoryOpened -= OnInventoryOpened;
        GameEvents.InventoryChanged -= OnInventoryChanged;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (IsOpen) Close();
            else Open();
        }
        if (IsOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private bool IsOpen => _root != null && _root.activeSelf;

    private void OnInventoryOpened() => Open();
    private void OnMenuCloseRequested() => Close();
    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount) => Refresh();

    private void Open()
    {
        if (IsOpen) return;
        _selectedSlot = -1;
        Refresh();
        _root.SetActive(true);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
    }

    private void Close()
    {
        if (!IsOpen) return;
        _selectedSlot = -1;
        _root.SetActive(false);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
        GameEvents.OnInventoryClosed();
    }

    public void OnSlotLeftClick(int index)
    {
        if (InventoryManager.Instance == null) return;
        var slot = InventoryManager.Instance.Slots[index];
        _selectedSlot = slot.IsEmpty ? -1 : index;
        Refresh();
    }

    public void OnSlotRightClick(int index)
    {
        if (InventoryManager.Instance == null) return;
        var slot = InventoryManager.Instance.Slots[index];
        if (slot.IsEmpty) return;

        InventoryManager.Instance.TryDropFromSlot(index, 1);
        if (_selectedSlot == index && slot.IsEmpty)
            _selectedSlot = -1;
        Refresh();
    }

    private void Refresh()
    {
        if (InventoryManager.Instance == null) return;

        for (int i = 0; i < _slotViews.Count && i < SlotCount; i++)
        {
            var slot = InventoryManager.Instance.Slots[i];
            _slotViews[i].Render(slot, i == _selectedSlot);
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        bool hasSelection = _selectedSlot >= 0
            && InventoryManager.Instance != null
            && !InventoryManager.Instance.Slots[_selectedSlot].IsEmpty;

        if (_detailPanel != null) _detailPanel.SetActive(hasSelection);
        if (_emptyDetailPanel != null) _emptyDetailPanel.SetActive(!hasSelection);

        if (!hasSelection) return;

        var slot = InventoryManager.Instance.Slots[_selectedSlot];
        var item = slot.Item;

        if (_detailName != null) _detailName.text = item.displayName;
        if (_detailType != null) _detailType.text = item.isIngredient ? "Ingredient" : "Product";
        if (_detailPrice != null) _detailPrice.text = $"Base Price: {item.basePrice}g";
        if (_detailStack != null) _detailStack.text = $"Stack: {slot.Count}/{InventorySlot.MaxStack}";
        if (_detailHint != null) _detailHint.text = "Right-click to drop";
    }

    private void BuildUI()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        _root = CreateChild("InventoryPanel", transform);
        var rootRect = _root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        var rootBg = _root.AddComponent<Image>();
        rootBg.color = new Color(0.15f, 0.13f, 0.1f, 0.95f);
        rootBg.raycastTarget = true;

        var titleGo = CreateChild("Title", _root.transform);
        var titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 36);
        titleRect.anchoredPosition = Vector2.zero;
        var titleText = titleGo.AddComponent<TMP_Text>();
        titleText = titleGo.AddComponent<TextMeshProUGUI>();
        DestroyImmediate(titleText);
        var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Inventory";
        titleTmp.fontSize = 20;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;

        var closeBtnGo = CreateChild("CloseButton", _root.transform);
        var closeRect = closeBtnGo.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(28, 28);
        closeRect.anchoredPosition = new Vector2(-6, -6);
        var closeBg = closeBtnGo.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.2f, 0.2f);
        var closeBtn = closeBtnGo.AddComponent<Button>();
        closeBtn.onClick.AddListener(Close);
        var closeLabel = closeBtnGo.AddComponent<TextMeshProUGUI>();
        closeLabel.text = "X";
        closeLabel.fontSize = 16;
        closeLabel.alignment = TextAlignmentOptions.Center;
        closeLabel.color = Color.white;

        BuildGrid(_root.transform);
        BuildDetailSidebar(_root.transform);
    }

    private void BuildGrid(Transform parent)
    {
        var gridGo = CreateChild("Grid", parent);
        var gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 0);
        gridRect.anchorMax = new Vector2(1, 1);
        gridRect.pivot = new Vector2(0, 1);
        gridRect.sizeDelta = new Vector2(-220, -44);
        gridRect.anchoredPosition = new Vector2(12, -38);
        var gridLayout = gridGo.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(CellSize, CellSize);
        gridLayout.spacing = new Vector2(CellGap, CellGap);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GridCols;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < SlotCount; i++)
        {
            var slotGo = CreateChild($"Slot_{i}", gridGo.transform);
            var slotBg = slotGo.AddComponent<Image>();
            slotBg.color = new Color(0.25f, 0.22f, 0.18f);
            slotBg.raycastTarget = true;

            var slotView = slotGo.AddComponent<InventorySlotView>();
            slotView.Initialize(i, this);
            _slotViews.Add(slotView);
        }
    }

    private void BuildDetailSidebar(Transform parent)
    {
        var sidebarGo = CreateChild("Sidebar", parent);
        var sidebarRect = sidebarGo.GetComponent<RectTransform>();
        sidebarRect.anchorMin = new Vector2(1, 0);
        sidebarRect.anchorMax = new Vector2(1, 1);
        sidebarRect.pivot = new Vector2(1, 1);
        sidebarRect.sizeDelta = new Vector2(200, -44);
        sidebarRect.anchoredPosition = new Vector2(-8, -38);
        var sidebarBg = sidebarGo.AddComponent<Image>();
        sidebarBg.color = new Color(0.12f, 0.1f, 0.08f, 0.6f);

        _emptyDetailPanel = CreateChild("EmptyDetail", sidebarGo.transform);
        var emptyRect = _emptyDetailPanel.GetComponent<RectTransform>();
        emptyRect.anchorMin = Vector2.zero;
        emptyRect.anchorMax = Vector2.one;
        emptyRect.sizeDelta = Vector2.zero;
        var emptyLabel = _emptyDetailPanel.AddComponent<TextMeshProUGUI>();
        emptyLabel.text = "Select an item";
        emptyLabel.fontSize = 14;
        emptyLabel.alignment = TextAlignmentOptions.Center;
        emptyLabel.color = new Color(0.6f, 0.6f, 0.6f);

        _detailPanel = CreateChild("Detail", sidebarGo.transform);
        var detailRect = _detailPanel.GetComponent<RectTransform>();
        detailRect.anchorMin = Vector2.zero;
        detailRect.anchorMax = Vector2.one;
        detailRect.sizeDelta = Vector2.zero;
        var detailLayout = _detailPanel.AddComponent<VerticalLayoutGroup>();
        detailLayout.padding = new RectOffset(12, 12, 12, 12);
        detailLayout.spacing = 8;
        detailLayout.childAlignment = TextAnchor.UpperLeft;
        detailLayout.childControlWidth = true;
        detailLayout.childControlHeight = false;

        _detailName = AddDetailLabel(_detailPanel.transform, 18, FontStyles.Bold, Color.white);
        _detailType = AddDetailLabel(_detailPanel.transform, 14, FontStyles.Normal, new Color(0.8f, 0.75f, 0.6f));
        _detailPrice = AddDetailLabel(_detailPanel.transform, 14, FontStyles.Normal, new Color(0.9f, 0.85f, 0.5f));
        _detailStack = AddDetailLabel(_detailPanel.transform, 14, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));
        _detailHint = AddDetailLabel(_detailPanel.transform, 12, FontStyles.Italic, new Color(0.5f, 0.5f, 0.5f));

        _detailPanel.SetActive(false);
    }

    private TMP_Text AddDetailLabel(Transform parent, int fontSize, FontStyles style, Color color)
    {
        var go = CreateChild("Label", parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = fontSize + 8;
        return tmp;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }
}
