public static class RenovationRules
{
    public static bool CanPurchase(BuildingState state)
    {
        return state == BuildingState.Abandoned;
    }

    public static bool CanSmash(BuildingState state, bool boardsSmashed)
    {
        return state == BuildingState.Purchased && !boardsSmashed;
    }

    public static bool IsSmashComplete(int done, int required)
    {
        return done >= required;
    }

    public static bool ShouldTransitionToClearedAfterSmash(bool isFacadeOnly, int done, int required)
    {
        return isFacadeOnly && IsSmashComplete(done, required);
    }

    public static bool CanHammer(BuildingState state, int repairPointsDone, int totalRepairPoints, bool hasTimber, bool hasNails)
    {
        return state == BuildingState.Cleared
               && repairPointsDone < totalRepairPoints
               && hasTimber
               && hasNails;
    }

    public static bool IsRepairComplete(int repairPointsDone, int totalRepairPoints)
    {
        return repairPointsDone >= totalRepairPoints;
    }

    public static bool CanCollectIncome(BuildingState state, int uncollectedIncome)
    {
        return state == BuildingState.Restored && uncollectedIncome > 0;
    }

    public static bool IsDebrisCleared(int debrisRemaining)
    {
        return debrisRemaining <= 0;
    }
}
