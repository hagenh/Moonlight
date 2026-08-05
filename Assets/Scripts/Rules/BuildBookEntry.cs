public sealed class BuildBookEntry
{
    public readonly ItemDef Item;
    public int Available;

    public BuildBookEntry(ItemDef item, int available)
    {
        Item = item;
        Available = available;
    }
}
