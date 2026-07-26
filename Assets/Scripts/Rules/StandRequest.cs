using System.Collections.Generic;

/// <summary>
/// Exact requests name one product. Descriptive requests — "something strong,
/// it's for a wedding" — name several the player may choose between, which is
/// what makes knowing your own recipes worth something.
/// </summary>
public enum RequestKind { Exact, Descriptive }

/// <summary>
/// One written order in the stand's book. Immutable: a note says what it says,
/// and the player either fills it, declines it, or leaves it sitting there.
///
/// There is no deadline field and there never will be. Requests do not expire —
/// the occupied slot is the whole of their cost. See GameDesign.md Part 3,
/// "Requests never expire — the slot is the cost".
/// </summary>
public sealed class StandRequest
{
    public readonly string Id;
    public readonly RequestKind Kind;
    public readonly IReadOnlyList<ItemDef> Accepts;
    public readonly int Units;
    public readonly string Signature;
    public readonly string Text;

    public StandRequest(string id, RequestKind kind, IReadOnlyList<ItemDef> accepts,
        int units, string signature, string text)
    {
        Id = id;
        Kind = kind;
        Accepts = accepts ?? new List<ItemDef>();
        Units = units;
        Signature = signature;
        Text = text;
    }
}
