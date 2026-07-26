using System.Collections.Generic;

/// <summary>
/// The notes currently pinned in the book, and how many may be pinned at once.
///
/// The slot count is the game's only source of pressure on the request economy:
/// notes never expire, so a request the player will not fill occupies a slot
/// that no new note can use. Ignoring a request costs the demand you did not get
/// to see instead — never anything the player already had.
/// </summary>
public sealed class RequestBook
{
    private readonly List<StandRequest> _active = new();
    private int _slotCount;

    public RequestBook(int slotCount)
    {
        _slotCount = slotCount < 0 ? 0 : slotCount;
    }

    public int SlotCount => _slotCount;

    public IReadOnlyList<StandRequest> Active => _active;

    /// <summary>
    /// Never negative. A shrunk book reports zero free slots rather than a
    /// deficit, because the overhang is resolved by the player filling notes.
    /// </summary>
    public int FreeSlots
    {
        get
        {
            int free = _slotCount - _active.Count;
            return free < 0 ? 0 : free;
        }
    }

    public bool TryPost(StandRequest request)
    {
        if (request == null) return false;
        if (FreeSlots <= 0) return false;
        if (Find(request.Id) != null) return false;

        _active.Add(request);
        return true;
    }

    public StandRequest Take(string id)
    {
        var found = Find(id);
        if (found == null) return null;

        _active.Remove(found);
        return found;
    }

    /// <summary>
    /// Shrinking never discards a posted note. Guardrail 1 is unconditional, and
    /// a note already offered is something the player has.
    /// </summary>
    public void SetSlotCount(int slotCount)
    {
        _slotCount = slotCount < 0 ? 0 : slotCount;
    }

    private StandRequest Find(string id)
    {
        if (id == null) return null;

        for (int i = 0; i < _active.Count; i++)
            if (_active[i].Id == id) return _active[i];

        return null;
    }
}
