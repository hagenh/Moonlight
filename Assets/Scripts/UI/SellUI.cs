using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SellUI : MonoBehaviour
{
    private bool _visible;
    private SellerType _currentSeller;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 340, 420);

    private void OnEnable()
    {
        GameEvents.SellMenuRequested += OnSellMenuRequested;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.SellMenuRequested -= OnSellMenuRequested;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    private void OnSellMenuRequested(SellerType type)
    {
        _visible = true;
        _currentSeller = type;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void OnMenuCloseRequested()
    {
        Close();
    }

    private void Update()
    {
        if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void Close()
    {
        if (!_visible) return;
        _visible = false;
        if (SellManager.Instance != null)
            SellManager.Instance.CloseSellMenu();
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        string title = _currentSeller == SellerType.Tormod
            ? "Tormod — Buy Ingredients"
            : "Traveling Cart — Buy Ingredients";
        _windowRect = GUI.Window(2, _windowRect, DrawWindow, title);
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (SellManager.Instance == null) return;

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(340));

        var ingredients = new List<ItemDef>
        {
            ContentDb.Grain, ContentDb.Sugar, ContentDb.Yeast, ContentDb.Water,
            ContentDb.Timber, ContentDb.Nails
        };

        foreach (var item in ingredients)
        {
            int price = SellManager.Instance.GetBuyPrice(item);
            int have = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetCount(item) : 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{item.displayName}  ({price}g each)  have: {have}", GUILayout.Width(220));

            GUI.enabled = GameManager.Instance != null && GameManager.Instance.Cash >= price;
            if (GUILayout.Button("Buy 1", GUILayout.Width(60)))
                SellManager.Instance.ExecutePurchase(item, 1);
            if (GUILayout.Button("Buy 5", GUILayout.Width(60)))
                SellManager.Instance.ExecutePurchase(item, 5);
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }
}
