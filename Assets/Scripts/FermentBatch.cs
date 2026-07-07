using UnityEngine;

public class FermentBatch
{
    public RecipeData Recipe { get; }
    public float StartGameMinutes { get; }
    public float TotalFermentMinutes { get; }

    public float Progress
    {
        get
        {
            if (TotalFermentMinutes <= 0) return 1f;
            float elapsed = TimeManager.Instance.TotalGameMinutes - StartGameMinutes;
            return Mathf.Clamp01(elapsed / TotalFermentMinutes);
        }
    }

    public bool IsComplete => Progress >= 1f;

    public FermentBatch(RecipeData recipe)
    {
        Recipe = recipe;
        StartGameMinutes = TimeManager.Instance.TotalGameMinutes;
        TotalFermentMinutes = recipe.fermentationHours * 60f;
    }
}
