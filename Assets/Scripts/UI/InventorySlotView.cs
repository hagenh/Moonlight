using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image background;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private int _index;
    private InventoryUI _parent;

    public void Initialize(int index, InventoryUI parent)
    {
        _index = index;
        _parent = parent;
    }

    public void Render(InventorySlot slot, bool selected)
    {
        if (background != null)
            background.sprite = selected ? selectedSprite : normalSprite;

        if (slot.IsEmpty)
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
            }
            if (countText != null) countText.text = "";
            return;
        }

        if (iconImage != null)
        {
            if (slot.Item.icon != null)
            {
                iconImage.sprite = slot.Item.icon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = slot.Item.isBottle
                    ? new Color(0.8f, 0.6f, 0.2f)
                    : new Color(0.6f, 0.4f, 0.2f);
            }
        }

        if (countText != null)
        {
            countText.text = slot.Count > 1 ? slot.Count.ToString() : "";
            countText.alignment = TMPro.TextAlignmentOptions.BottomRight;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_parent == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            _parent.OnSlotLeftClick(_index);
        else if (eventData.button == PointerEventData.InputButton.Right)
            _parent.OnSlotRightClick(_index);
    }
}
