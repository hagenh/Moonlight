using UnityEngine;

/// <summary>
/// What a filled request pays, and what may fill it.
///
/// The premium over shelf price is the reason the book is the primary economy
/// rather than a side channel: passive shelf trade always works and always pays
/// 1.0×, so a request has to be worth the planning it costs.
/// </summary>
public static class RequestBookRules
{
    public const float ExactMultiplier = 1.8f;
    public const float DescriptiveMultiplier = 2.2f;

    public static bool Accepts(StandRequest request, ItemDef item)
    {
        if (request == null || item == null) return false;

        for (int i = 0; i < request.Accepts.Count; i++)
            if (request.Accepts[i] == item) return true;

        return false;
    }

    /// <summary>
    /// Prices what was actually delivered, not what was asked for. A descriptive
    /// request filled with the cheaper valid answer pays less — the player chose
    /// to spend less effort and is paid accordingly, with nothing taken away.
    /// </summary>
    public static int Payment(StandRequest request, ItemDef delivered)
    {
        if (!Accepts(request, delivered)) return 0;

        float multiplier = request.Kind == RequestKind.Descriptive
            ? DescriptiveMultiplier
            : ExactMultiplier;

        return Mathf.RoundToInt(delivered.basePrice * request.Units * multiplier);
    }
}
