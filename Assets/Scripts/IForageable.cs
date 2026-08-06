public interface IForageable
{
    bool IsHarvested { get; }
    float SwingDuration { get; }
    ItemDef RequiredTool { get; }
    void CompleteSwing();
}
