using System.Collections.Generic;

/// <summary>
/// What arrives in the book overnight.
///
/// Notes are written while the player sleeps and read in the morning, which is
/// the mechanical half of "day is when you act, night is when the day answers
/// back". Generation only ever draws on recipes the player can already make, so
/// the book never asks for something unreachable — descriptive requests point at
/// the next unlock, and that is a later plan's job, not this one's.
/// </summary>
public static class RequestArrivalRules
{
    public const int MinBatches = 1;
    public const int MaxBatches = 3;

    /// <summary>
    /// One request in this many is descriptive. GameDesign.md says only "a
    /// minority are descriptive" and does not fix the fraction; this is the
    /// tuning knob, not a settled design number.
    /// </summary>
    public const int DescriptiveInN = 4;

    private static readonly string[] Signatures =
    {
        "A carter", "A traveller", "Someone passing", "A woman from the valley road",
        "No name given", "A man who did not stay"
    };

    private static readonly string[] ExactTexts =
    {
        "Leave them under the bench. I'll settle up.",
        "The same again, if you have it.",
        "I'll be back this way inside the week.",
        "For my brother. He asked where I got the last."
    };

    private static readonly string[] DescriptiveTexts =
    {
        "Something strong. It's for a wedding.",
        "Whatever you'd drink yourself.",
        "Something to keep the cold out.",
        "Your best. It's an apology."
    };

    public static int NotesPerNight(int slotCount) => slotCount >= 5 ? 3 : 2;

    public static StandRequest Generate(IReadOnlyList<RecipeData> available, IRng rng, string id)
    {
        if (available == null || available.Count == 0 || rng == null) return null;

        bool descriptive = available.Count > 1 && rng.Range(0, DescriptiveInN) == 0;
        int batches = rng.Range(MinBatches, MaxBatches + 1);

        if (descriptive)
        {
            var accepts = new List<ItemDef>();
            for (int i = 0; i < available.Count; i++)
                if (available[i]?.outputItem != null) accepts.Add(available[i].outputItem);

            if (accepts.Count < 2) return ExactRequest(available, rng, id, batches);

            int units = batches * available[0].outputCount;

            return new StandRequest(id, RequestKind.Descriptive, accepts, units,
                Pick(Signatures, rng), Pick(DescriptiveTexts, rng));
        }

        return ExactRequest(available, rng, id, batches);
    }

    private static StandRequest ExactRequest(IReadOnlyList<RecipeData> available, IRng rng, string id, int batches)
    {
        var recipe = available[rng.Range(0, available.Count)];
        int units = batches * recipe.outputCount;

        return new StandRequest(id, RequestKind.Exact,
            new List<ItemDef> { recipe.outputItem }, units,
            Pick(Signatures, rng), Pick(ExactTexts, rng));
    }

    private static string Pick(string[] pool, IRng rng) => pool[rng.Range(0, pool.Length)];
}
