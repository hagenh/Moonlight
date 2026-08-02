public sealed class InventorySlot
{
    public ItemDef Item;
    public int Count;
    public const int MaxStack = 30;
    public bool IsEmpty => Item == null;
    public bool IsFull => Count >= MaxStack;
}

public readonly struct AddResult
{
    public readonly bool Success;
    public readonly int Added;
    public readonly int Overflow;

    public AddResult(bool success, int added, int overflow)
    {
        Success = success;
        Added = added;
        Overflow = overflow;
    }
}

public readonly struct DropResult
{
    public readonly bool Success;
    public readonly ItemDef Def;
    public readonly int Count;

    public DropResult(bool success, ItemDef def, int count)
    {
        Success = success;
        Def = def;
        Count = count;
    }
}
