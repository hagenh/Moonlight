using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailType;
    [SerializeField] private TMP_Text detailPrice;
    [SerializeField] private TMP_Text detailStack;
    [SerializeField] private TMP_Text detailHint;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private GameObject emptyDetailPanel;
    [SerializeField] private Button closeButton;

    private const int SlotCount = 20;

    private readonly List<InventorySlotView> _slotViews = new();
    private int _selectedSlot = -1;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        BuildGrid();
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

    private bool IsOpen => root != null && root.activeSelf;

    private void OnInventoryOpened() => Open();
    private void OnMenuCloseRequested() => Close();
    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount) => Refresh();

    private void Open()
    {
        if (IsOpen) return;
        _selectedSlot = -1;
        Refresh();
        if (root != null) root.SetActive(true);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
    }

    private void Close()
    {
        if (!IsOpen) return;
        _selectedSlot = -1;
        if (root != null) root.SetActive(false);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
        GameEvents.OnInventoryClosed();
    }

    private void BuildGrid()
    {
        if (gridContainer == null || slotPrefab == null) return;

        for (int i = 0; i < SlotCount; i++)
        {
            var view = Instantiate(slotPrefab, gridContainer);
            view.Initialize(i, this);
            _slotViews.Add(view);
        }
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

    private void RefreshDetail()
    {
        bool hasSelection = _selectedSlot >= 0
            && InventoryManager.Instance != null
            && !InventoryManager.Instance.Slots[_selectedSlot].IsEmpty;

        if (detailPanel != null) detailPanel.SetActive(hasSelection);
        if (emptyDetailPanel != null) emptyDetailPanel.SetActive(!hasSelection);

        if (!hasSelection) return;

        var slot = InventoryManager.Instance.Slots[_selectedSlot];
        var item = slot.Item;

        if (detailName != null) detailName.text = item.displayName;
        if (detailType != null) detailType.text = item.isIngredient ? "Ingredient" : "Product";
        if (detailPrice != null) detailPrice.text = $"Base Price: {item.basePrice}g";
        if (detailStack != null) detailStack.text = $"Stack: {slot.Count}/{InventorySlot.MaxStack}";
        if (detailHint != null) detailHint.text = "Right-click to drop";
    }
}
