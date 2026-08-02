using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour, IPointerClickHandler
{
    private Image _iconImage;
    private TMP_Text _countText;
    private Image _highlight;

    private int _index;
    private InventoryUI _parent;

    public void Initialize(int index, InventoryUI parent)
    {
        _index = index;
        _parent = parent;

        _iconImage = GetComponent<Image>();

        var countGo = new GameObject("Count");
        countGo.transform.SetParent(transform, false);
        var countRect = countGo.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1, 0);
        countRect.anchorMax = new Vector2(1, 0);
        countRect.pivot = new Vector2(1, 0);
        countRect.sizeDelta = new Vector2(30, 18);
        countRect.anchoredPosition = new Vector2(-2, 2);
        _countText = countGo.AddComponent<TextMeshProUGUI>();
        _countText.fontSize = 12;
        _countText.alignment = TextAlignmentOptions.BottomRight;
        _countText.color = Color.white;

        var highlightGo = new GameObject("Highlight");
        highlightGo.transform.SetParent(transform, false);
        var highlightRect = highlightGo.GetComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.sizeDelta = Vector2.zero;
        _highlight = highlightGo.AddComponent<Image>();
        _highlight.color = new Color(1f, 0.9f, 0.3f, 0.4f);
        _highlight.raycastTarget = false;
        highlightGo.SetActive(false);
    }

    public void Render(InventorySlot slot, bool selected)
    {
        if (_highlight != null) _highlight.gameObject.SetActive(selected);

        if (slot.IsEmpty)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }
            if (_countText != null) _countText.text = "";
            return;
        }

        if (_iconImage != null)
        {
            if (slot.Item.icon != null)
            {
                _iconImage.sprite = slot.Item.icon;
                _iconImage.color = Color.white;
            }
            else
            {
                _iconImage.sprite = null;
                _iconImage.color = slot.Item.isBottle
                    ? new Color(0.8f, 0.6f, 0.2f)
                    : new Color(0.6f, 0.4f, 0.2f);
            }
        }

        if (_countText != null)
            _countText.text = slot.Count > 1 ? slot.Count.ToString() : "";
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
