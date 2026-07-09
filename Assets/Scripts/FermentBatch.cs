using System;
using UnityEngine;

public class FermentBatch
{
    public RecipeData Recipe { get; }
    public float StartGameMinutes { get; }
    public float TotalFermentMinutes { get; }

    private readonly Func<float> _getCurrentMinutes;

    public float Progress
    {
        get
        {
            if (TotalFermentMinutes <= 0) return 1f;
            float elapsed = _getCurrentMinutes() - StartGameMinutes;
            return Mathf.Clamp01(elapsed / TotalFermentMinutes);
        }
    }

    public bool IsComplete => Progress >= 1f;

    public FermentBatch(RecipeData recipe, Func<float> getCurrentGameMinutes)
    {
        Recipe = recipe;
        _getCurrentMinutes = getCurrentGameMinutes;
        StartGameMinutes = _getCurrentMinutes();
        TotalFermentMinutes = recipe.fermentationHours * 60f;
    }
}
