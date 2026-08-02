using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(0, 0, 560, 400);
    private int _selectedSlot = -1;

    private const int GridCols = 5;
    private const int GridRows = 4;
    private const int CellSize = 56;
    private const int CellGap = 4;
    private const int GridWidth = GridCols * CellSize + (GridCols - 1) * CellGap;
    private const int SidebarWidth = 220;
    private const int SidebarX = GridWidth + 20;

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

    private void OnInventoryOpened()
    {
        Open();
    }

    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount)
    {
    }

    private void OnMenuCloseRequested()
    {
        Close();
    }

    private void Open()
    {
        if (_visible) return;
        _visible = true;
        _selectedSlot = -1;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void Close()
    {
        if (!_visible) return;
        _visible = false;
        _selectedSlot = -1;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
        GameEvents.OnInventoryClosed();
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (_visible) Close();
            else Open();
        }
        if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUI.Window(3, _windowRect, DrawWindow, "Inventory");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (InventoryManager.Instance == null) return;

        DrawGrid();
        DrawSidebar();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }

    private void DrawGrid()
    {
        float startX = 10;
        float startY = 30;

        for (int row = 0; row < GridRows; row++)
        {
            for (int col = 0; col < GridCols; col++)
            {
                int idx = row * GridCols + col;
                float x = startX + col * (CellSize + CellGap);
                float y = startY + row * (CellSize + CellGap);
                var rect = new Rect(x, y, CellSize, CellSize);

                var slot = InventoryManager.Instance.Slots[idx];
                bool isSelected = idx == _selectedSlot;

                Color prevBg = GUI.backgroundColor;
                if (isSelected)
                    GUI.backgroundColor = new Color(1f, 0.9f, 0.4f);
                else if (slot.IsEmpty)
                    GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                else
                    GUI.backgroundColor = new Color(0.35f, 0.3f, 0.25f);

                GUI.Box(rect, "");
                GUI.backgroundColor = prevBg;

                if (!slot.IsEmpty)
                    DrawSlotContent(rect, slot);

                HandleSlotInput(rect, idx, slot);
            }
        }
    }

    private void DrawSlotContent(Rect rect, InventorySlot slot)
    {
        if (slot.Item.icon != null)
        {
            var sprite = slot.Item.icon;
            var tex = sprite.texture;
            var cr = sprite.textureRect;
            var uvRect = new Rect(cr.x / tex.width, cr.y / tex.height, cr.width / tex.width, cr.height / tex.height);
            GUI.DrawTextureWithTexCoords(rect, tex, uvRect);
        }
        else
        {
            Color prev = GUI.color;
            GUI.color = slot.Item.isBottle ? new Color(0.8f, 0.6f, 0.2f) : new Color(0.6f, 0.4f, 0.2f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prev;

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(rect, slot.Item.displayName[0].ToString(), labelStyle);
        }

        if (slot.Count > 1)
        {
            var countStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 12
            };
            var countRect = new Rect(rect.x + rect.width - 28, rect.y + rect.height - 18, 26, 16);
            GUI.Label(countRect, slot.Count.ToString(), countStyle);
        }
    }

    private void HandleSlotInput(Rect rect, int idx, InventorySlot slot)
    {
        var e = Event.current;
        if (rect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    _selectedSlot = slot.IsEmpty ? -1 : idx;
                    e.Use();
                }
                else if (e.button == 1 && !slot.IsEmpty)
                {
                    InventoryManager.Instance.TryDropFromSlot(idx, 1);
                    if (_selectedSlot == idx && slot.IsEmpty)
                        _selectedSlot = -1;
                    e.Use();
                }
            }
        }
    }

    private void DrawSidebar()
    {
        float x = SidebarX + 10;
        float y = 30;

        if (_selectedSlot < 0 || InventoryManager.Instance.Slots[_selectedSlot].IsEmpty)
        {
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x, y, SidebarWidth - 10, 200), "Select an item", style);
            return;
        }

        var slot = InventoryManager.Instance.Slots[_selectedSlot];
        var item = slot.Item;
        float curY = y;

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 24), item.displayName, titleStyle);
        curY += 28;

        var tag = item.isIngredient ? "Ingredient" : "Product";
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), tag);
        curY += 22;

        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), $"Base Price: {item.basePrice}g");
        curY += 22;

        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 20), $"Stack: {slot.Count}/{InventorySlot.MaxStack}");
        curY += 30;

        var hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Italic };
        GUI.Label(new Rect(x, curY, SidebarWidth - 10, 40), "Right-click slot\nto drop", hintStyle);
    }
}
