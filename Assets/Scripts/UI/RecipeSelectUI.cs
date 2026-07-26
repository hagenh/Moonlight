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
        _windowRect = GUI.Window(1, _windowRect, DrawWindow, "Recipe Book");
        GUI.color = prevBg;
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (InventoryManager.Instance == null) return;

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(320));

        var pages = RecipeBookRules.CompilePages(
            FermentManager.Instance.Recipes,
            FermentManager.Instance.IsRecipeDiscovered);

        RecipeData chosen = null;

        foreach (var page in pages)
        {
            if (page.IsLegible)
            {
                if (DrawLegiblePage(page.Recipe))
                    chosen = page.Recipe;
            }
            else
            {
                DrawTornPage(page);
            }
        }

        DrawBurnedSection();

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
        {
            Close();
            return;
        }

        // Acting on the choice is deferred until after EndScrollView — returning
        // from inside the group leaves IMGUI's layout stack unbalanced.
        if (chosen != null)
        {
            FermentManager.Instance.TryStartBatch(_targetVat, chosen);
            Close();
        }
    }

    /// <summary>Returns true when the player pressed this page's brew button.</summary>
    private bool DrawLegiblePage(RecipeData recipe)
    {
        bool unlocked = FermentManager.Instance.IsRecipeUnlocked(recipe);

        if (!unlocked)
        {
            GUI.enabled = false;
            GUILayout.Label(recipe.recipeName + " (Locked)");
            string hint = "";
            if (!string.IsNullOrEmpty(recipe.unlockedByBuildingId))
                hint = $"  Restore the {recipe.unlockedByBuildingId} to unlock";
            if (recipe.minReputation > 0)
                hint += $"  Requires Reputation {recipe.minReputation}+";
            if (!string.IsNullOrEmpty(hint))
                GUILayout.Label(hint);
            GUILayout.Space(4);
            GUI.enabled = true;
            return false;
        }

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
        bool pressed = GUILayout.Button(recipe.recipeName);
        GUI.enabled = true;

        foreach (var kvp in recipe.Costs)
        {
            int have = InventoryManager.Instance.GetCount(kvp.Key);
            GUILayout.Label($"  {kvp.Key.displayName} x{kvp.Value} (have {have})");
        }
        GUILayout.Label($"  Time: {recipe.fermentationHours}h -> {recipe.outputCount} {recipe.outputItem?.displayName ?? "???"}");
        GUILayout.Space(4);

        return pressed;
    }

    /// <summary>
    /// A torn page shows that something is missing, never what. The page carries
    /// no recipe reference, so there is nothing here that could leak a name.
    /// </summary>
    private void DrawTornPage(BookPage page)
    {
        GUI.enabled = false;
        GUILayout.Label($"{page.PageNumber}.  ~~~~~~~~~~~~~~~~");
        GUILayout.Label("     (torn out)");
        GUILayout.Space(4);
        GUI.enabled = true;
    }

    private void DrawBurnedSection()
    {
        GUILayout.Space(10);
        GUI.enabled = false;
        GUILayout.Label("--- the back of the book is burned through ---");
        foreach (var scrap in RecipeBookRules.BurnedScraps)
            GUILayout.Label($"  {scrap}");
        GUI.enabled = true;
    }
}
