using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    private struct HotbarSlotRefs
    {
        public InventorySlotView view;
        public GameObject outline;
    }

    [SerializeField] private HotbarSlotRefs[] slots;

    private InputSystem_Actions _input;

    private void Awake()
    {
        _input = new InputSystem_Actions();
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

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = InventoryManager.Instance.Slots[i];
            slots[i].view.Render(slot, false);
            if (slots[i].outline != null)
                slots[i].outline.SetActive(i == InventoryManager.Instance.ActiveSlotIndex);
        }
    }
}
