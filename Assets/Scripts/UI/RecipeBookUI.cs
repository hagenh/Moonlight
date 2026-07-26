using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The grandfather's recipe book. Not the stand's request book — see
/// <see cref="RequestBookUI"/>, which is a different thing entirely.
///
/// Read mode opens from anywhere and shows no brew buttons, so the player can
/// meet the burned back section long before they own a vat. Brew mode opens from
/// a vat and is the only mode that can act.
/// </summary>
public class RecipeBookUI : MonoBehaviour
{
    private enum Mode { Read, Brew }

    [SerializeField] private GameObject root;
    [SerializeField] private RecipeBookPageView leftPage;
    [SerializeField] private RecipeBookPageView rightPage;
    [SerializeField] private GameObject burnedPanel;
    [SerializeField] private TMP_Text burnedLabel;
    [SerializeField] private TMP_Text spreadLabel;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private InputSystem_Actions _input;
    private FermentVat _targetVat;
    private Mode _mode = Mode.Read;
    private int _spreadIndex;
    private List<BookSpread> _spreads = new();

    private void Awake()
    {
        _input = new InputSystem_Actions();
        if (root != null) root.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(PreviousSpread);
        if (nextButton != null) nextButton.onClick.AddListener(NextSpread);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (leftPage != null && leftPage.BrewButton != null)
            leftPage.BrewButton.onClick.AddListener(() => BrewFrom(true));
        if (rightPage != null && rightPage.BrewButton != null)
            rightPage.BrewButton.onClick.AddListener(() => BrewFrom(false));
    }

    private void OnEnable()
    {
        GameEvents.RecipeBookRequested += OpenForReading;
        GameEvents.RecipeSelectionRequested += OpenForBrewing;
        GameEvents.MenuCloseRequested += Close;

        _input.Menus.Enable();
        _input.Menus.RecipeBook.performed += OnRecipeBookKey;
    }

    private void OnDisable()
    {
        GameEvents.RecipeBookRequested -= OpenForReading;
        GameEvents.RecipeSelectionRequested -= OpenForBrewing;
        GameEvents.MenuCloseRequested -= Close;

        _input.Menus.RecipeBook.performed -= OnRecipeBookKey;
        _input.Menus.Disable();
    }

    private void OnRecipeBookKey(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (IsOpen) Close();
        else OpenForReading();
    }

    private bool IsOpen => root != null && root.activeSelf;

    private void OpenForReading()
    {
        _targetVat = null;
        Open(Mode.Read);
    }

    private void OpenForBrewing(FermentVat vat)
    {
        _targetVat = vat;
        Open(Mode.Brew);
    }

    private void Open(Mode mode)
    {
        if (FermentManager.Instance == null) return;

        _mode = mode;
        _spreadIndex = 0;
        Rebuild();

        if (root != null) root.SetActive(true);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
    }

    private void Close()
    {
        if (!IsOpen) return;

        root.SetActive(false);
        _targetVat = null;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void Rebuild()
    {
        var pages = RecipeBookRules.CompilePages(
            FermentManager.Instance.Recipes,
            FermentManager.Instance.IsRecipeDiscovered);

        _spreads = RecipeBookRules.CompileSpreads(pages);
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex, _spreads.Count);
        RenderCurrentSpread();
    }

    private void PreviousSpread()
    {
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex - 1, _spreads.Count);
        RenderCurrentSpread();
    }

    private void NextSpread()
    {
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex + 1, _spreads.Count);
        RenderCurrentSpread();
    }

    private void RenderCurrentSpread()
    {
        if (_spreads.Count == 0) return;

        var spread = _spreads[_spreadIndex];

        if (spreadLabel != null)
            spreadLabel.text = $"Spread {_spreadIndex + 1} of {_spreads.Count}";
        if (prevButton != null) prevButton.interactable = _spreadIndex > 0;
        if (nextButton != null) nextButton.interactable = _spreadIndex < _spreads.Count - 1;

        if (burnedPanel != null) burnedPanel.SetActive(spread.IsBurnedSection);

        if (spread.IsBurnedSection)
        {
            if (leftPage != null) leftPage.ShowBlank();
            if (rightPage != null) rightPage.ShowBlank();
            if (burnedLabel != null) burnedLabel.text = BuildBurnedText();
            return;
        }

        bool showBrew = _mode == Mode.Brew;

        if (leftPage != null)
            leftPage.Render(spread.Left, StatusFor(spread.Left), showBrew, GetCount);

        if (rightPage != null)
        {
            if (spread.HasRight)
                rightPage.Render(spread.Right, StatusFor(spread.Right), showBrew, GetCount);
            else
                rightPage.ShowBlank();
        }
    }

    private static string BuildBurnedText()
    {
        var builder = new StringBuilder();
        foreach (var scrap in RecipeBookRules.BurnedScraps)
            builder.AppendLine(scrap);
        return builder.ToString();
    }

    private PageStatus StatusFor(BookPage page) =>
        RecipeBookRules.StatusOf(page, FermentManager.Instance.IsRecipeUnlocked, GetCount);

    private static int GetCount(ItemDef item) =>
        InventoryManager.Instance != null ? InventoryManager.Instance.GetCount(item) : 0;

    private void BrewFrom(bool left)
    {
        if (_mode != Mode.Brew || _targetVat == null) return;
        if (_spreads.Count == 0) return;

        var spread = _spreads[_spreadIndex];
        if (spread.IsBurnedSection) return;

        var page = left ? spread.Left : spread.Right;
        if (!left && !spread.HasRight) return;
        if (!StatusFor(page).CanBrew) return;

        FermentManager.Instance.TryStartBatch(_targetVat, page.Recipe);
        Close();
    }
}
