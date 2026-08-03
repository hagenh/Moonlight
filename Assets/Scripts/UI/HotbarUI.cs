using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotView slotTemplate;

    private readonly List<InventorySlotView> _slotViews = new();
    private InputSystem_Actions _input;

    private void Awake()
    {
        _input = new InputSystem_Actions();
        BuildSlots();
    }

    private void OnEnable()
    {
        GameEvents.InventoryChanged += OnInventoryChanged;
        GameEvents.ActiveSlotChanged += OnActiveSlotChanged;
        _input.Player.Hotbar.performed += OnHotbarKey;
        _input.Player.Hotbar.Enable();
        Refresh();
    }

    private void OnDisable()
    {
        GameEvents.InventoryChanged -= OnInventoryChanged;
        GameEvents.ActiveSlotChanged -= OnActiveSlotChanged;
        _input.Player.Hotbar.performed -= OnHotbarKey;
        _input.Player.Hotbar.Disable();
    }

    private void BuildSlots()
    {
        if (slotContainer == null || slotTemplate == null) return;

        for (int i = 0; i < InventoryManager.HotbarSlotCount; i++)
        {
            var view = Instantiate(slotTemplate, slotContainer);
            view.Initialize(i, null);
            _slotViews.Add(view);
        }

        slotTemplate.gameObject.SetActive(false);
    }

    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount) => Refresh();
    private void OnActiveSlotChanged(int index) => Refresh();

    private void OnHotbarKey(InputAction.CallbackContext ctx)
    {
        if (InventoryManager.Instance == null) return;
        if (!int.TryParse(ctx.control.name, out int pressed)) return;
        InventoryManager.Instance.SetActiveSlot(pressed - 1);
    }

    private void Refresh()
    {
        if (InventoryManager.Instance == null) return;

        for (int i = 0; i < _slotViews.Count && i < InventoryManager.HotbarSlotCount; i++)
        {
            var slot = InventoryManager.Instance.Slots[i];
            _slotViews[i].Render(slot, i == InventoryManager.Instance.ActiveSlotIndex);
        }
    }
}
