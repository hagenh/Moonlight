using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SellUI : MonoBehaviour
{
    private SellerType? _currentSeller;
    private bool _visible;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 340, 420);
    private bool _showBuyTab;

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
        _currentSeller = type;
        _visible = true;
        _showBuyTab = false;
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
        _currentSeller = null;
        if (SellManager.Instance != null)
            SellManager.Instance.CloseSellMenu();
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible || !_currentSeller.HasValue) return;
        _windowRect = GUI.Window(2, _windowRect, DrawWindow, GetTitle());
    }

    private string GetTitle()
    {
        return _currentSeller.Value switch
        {
            SellerType.Tormod => "Tormod's Offer",
            SellerType.TravelingCart => "Traveling Cart",
            SellerType.RiskyBuyer => "Shady Deal",
            _ => "Sell"
        };
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (_currentSeller == SellerType.RiskyBuyer)
        {
            GUI.color = Color.red;
            GUILayout.Label("WARNING: +15 Heat per sale!");
            if (GameManager.Instance != null && GameManager.Instance.Heat > 50)
                GUILayout.Label($"Heat is {GameManager.Instance.Heat} - 10% confiscation risk!");
            GUI.color = Color.white;
        }

        if (_currentSeller == SellerType.TravelingCart)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(!_showBuyTab, "Sell Bottles", GUILayout.Width(150))) _showBuyTab = false;
            if (GUILayout.Toggle(_showBuyTab, "Buy Ingredients", GUILayout.Width(150))) _showBuyTab = true;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        if (!_showBuyTab)
            DrawSellSection();
        else
            DrawBuySection();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }

    private void DrawSellSection()
    {
        if (InventoryManager.Instance == null) return;

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(280));

        bool anyItems = false;
        foreach (var kvp in new List<KeyValuePair<ItemDef, int>>(InventoryManager.Instance.AllItems))
        {
            if (!IsSellable(kvp.Key)) continue;
            anyItems = true;

            int price = SellManager.Instance != null
                ? SellManager.Instance.GetSellPrice(kvp.Key, _currentSeller!.Value)
                : kvp.Key.basePrice;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{kvp.Key.displayName} x{kvp.Value}  ({price}g each)", GUILayout.Width(200));

            GUI.enabled = kvp.Value >= 1;
            if (GUILayout.Button("Sell 1", GUILayout.Width(60)))
                SellManager.Instance?.ExecuteSale(kvp.Key, 1, _currentSeller!.Value);
            if (GUILayout.Button("Sell All", GUILayout.Width(60)))
                SellManager.Instance?.ExecuteSale(kvp.Key, kvp.Value, _currentSeller!.Value);
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        if (!anyItems)
            GUILayout.Label("Nothing to sell.");

        GUILayout.EndScrollView();
    }

    private void DrawBuySection()
    {
        if (SellManager.Instance == null) return;

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(280));

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
            GUILayout.Label($"{item.displayName}  ({price}g each)  have: {have}", GUILayout.Width(200));

            GUI.enabled = GameManager.Instance != null && GameManager.Instance.Cash >= price;
            if (GUILayout.Button("Buy 1", GUILayout.Width(60)))
                SellManager.Instance.ExecutePurchase(item, 1);
            if (GUILayout.Button("Buy 5", GUILayout.Width(60)))
                SellManager.Instance.ExecutePurchase(item, 5);
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private bool IsSellable(ItemDef item)
    {
        if (_currentSeller == SellerType.TravelingCart)
            return item.isBottle;
        return !item.isIngredient;
    }
}
