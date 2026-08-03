using System.Collections;
using TMPro;
using UnityEngine;

public class HotbarAnnouncementUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text label;

    private const float DisplayDuration = 3f;
    private Coroutine _hideRoutine;

    private void OnEnable()
    {
        GameEvents.ActiveSlotChanged += OnActiveSlotChanged;
        if (root != null) root.SetActive(false);
    }

    private void OnDisable()
    {
        GameEvents.ActiveSlotChanged -= OnActiveSlotChanged;
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
    }

    private void OnActiveSlotChanged(int index)
    {
        if (InventoryManager.Instance == null) return;
        var slot = InventoryManager.Instance.Slots[index];

        if (slot.IsEmpty)
        {
            if (root != null) root.SetActive(false);
            return;
        }

        if (label != null)
            label.text = slot.Count > 1 ? $"{slot.Item.displayName} x{slot.Count}" : slot.Item.displayName;
        if (root != null) root.SetActive(true);

        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(DisplayDuration);
        if (root != null) root.SetActive(false);
        _hideRoutine = null;
    }
}
