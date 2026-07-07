using UnityEngine;
using UnityEngine.InputSystem;

public class RecipeSelectUI : MonoBehaviour
{
    private FermentVat _targetVat;
    private bool _visible;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 320, 400);

    private void OnEnable()
    {
        GameEvents.RecipeSelectionRequested += OnRecipeSelectionRequested;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.RecipeSelectionRequested -= OnRecipeSelectionRequested;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    private void OnRecipeSelectionRequested(FermentVat vat)
    {
        _targetVat = vat;
        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2 + 100;
        _windowRect.y = (Screen.height - _windowRect.height) / 2 - 50;
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
        _targetVat = null;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible || FermentManager.Instance == null) return;

        Color prevBg = GUI.color;
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.95f);
        _windowRect = GUI.Window(1, _windowRect, DrawWindow, "Choose Recipe");
        GUI.color = prevBg;
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(320));

        foreach (var recipe in FermentManager.Instance.Recipes)
        {
            bool canAfford = true;
            foreach (var kvp in recipe.Costs)
            {
                if (!InventoryManager.Instance.Has(kvp.Key, kvp.Value))
                {
                    canAfford = false;
                    break;
                }
            }

            GUI.enabled = canAfford;
            if (GUILayout.Button(recipe.recipeName))
            {
                FermentManager.Instance.TryStartBatch(_targetVat, recipe);
                Close();
                return;
            }
            GUI.enabled = true;

            foreach (var kvp in recipe.Costs)
            {
                int have = InventoryManager.Instance.GetCount(kvp.Key);
                GUILayout.Label($"  {kvp.Key.displayName} x{kvp.Value} (have {have})");
            }
            GUILayout.Label($"  Time: {recipe.fermentationHours}h -> {recipe.outputCount} {recipe.outputItem?.displayName ?? "???"}");
            GUILayout.Space(4);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
        {
            Close();
        }
    }
}
