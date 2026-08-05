using System.Collections.Generic;

public sealed class BuildBook
{
    private readonly List<BuildBookEntry> _entries = new();

    public IReadOnlyList<BuildBookEntry> Entries => _entries;

    public void Add(ItemDef item, int available)
    {
        if (item == null || available <= 0) return;

        var existing = Find(item);
        if (existing != null)
        {
            existing.Available += available;
            return;
        }

        _entries.Add(new BuildBookEntry(item, available));
    }

    public int Available(ItemDef item) => Find(item)?.Available ?? 0;

    public bool TryConsume(ItemDef item)
    {
        var entry = Find(item);
        if (entry == null || entry.Available <= 0) return false;

        entry.Available -= 1;
        return true;
    }

    private BuildBookEntry Find(ItemDef item)
    {
        foreach (var entry in _entries)
            if (entry.Item == item) return entry;

        return null;
    }
}
