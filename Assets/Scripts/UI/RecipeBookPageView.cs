using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One page of the book. Holds no game state and asks no manager anything — it
/// renders exactly what it is handed, so the page states stay decidable in Rules.
/// </summary>
public class RecipeBookPageView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text pageNumberLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text footnoteLabel;
    [SerializeField] private Button brewButton;

    private readonly StringBuilder _builder = new();

    public Button BrewButton => brewButton;

    public void ShowBlank()
    {
        if (root != null) root.SetActive(false);
    }

    public void Render(BookPage page, PageStatus status, bool showBrew,
        System.Func<ItemDef, int> getCount)
    {
        if (root != null) root.SetActive(true);

        if (pageNumberLabel != null)
            pageNumberLabel.text = page.PageNumber > 0 ? page.PageNumber.ToString() : "";

        if (status.IsTorn)
        {
            RenderTorn();
            return;
        }

        var recipe = page.Recipe;
        if (titleLabel != null) titleLabel.text = recipe.recipeName;
        if (bodyLabel != null) bodyLabel.text = BuildBody(recipe, getCount);
        if (footnoteLabel != null) footnoteLabel.text = BuildFootnote(recipe, status);

        if (brewButton != null)
        {
            brewButton.gameObject.SetActive(showBrew);
            brewButton.interactable = status.CanBrew;
        }
    }

    private void RenderTorn()
    {
        if (titleLabel != null) titleLabel.text = "";
        if (bodyLabel != null) bodyLabel.text = "(torn out)";
        if (footnoteLabel != null) footnoteLabel.text = "";
        if (brewButton != null) brewButton.gameObject.SetActive(false);
    }

    private string BuildBody(RecipeData recipe, System.Func<ItemDef, int> getCount)
    {
        _builder.Clear();
        foreach (var cost in recipe.Costs)
        {
            int have = getCount != null ? getCount(cost.Key) : 0;
            _builder.AppendLine($"{cost.Key.displayName} x{cost.Value}   (have {have})");
        }
        return _builder.ToString();
    }

    private string BuildFootnote(RecipeData recipe, PageStatus status)
    {
        if (status.Reason == LockReason.RequiresBuilding)
            return $"Restore the {status.RequiredBuildingId} to read this.";
        if (status.Reason == LockReason.RequiresReputation)
            return $"Requires standing {status.RequiredReputation}+.";

        string output = recipe.outputItem != null ? recipe.outputItem.displayName : "???";
        return $"{recipe.fermentationHours}h  ->  {recipe.outputCount} {output}";
    }
}
