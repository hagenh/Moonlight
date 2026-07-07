public static class GameEvents
{
    public static event System.Action<string> ToastRequested;
    public static event System.Action<Building, BuildingState, BuildingState> BuildingStateChanged;
    public static event System.Action<object> FragmentFound;
    public static event System.Action<object, Building> ResidentMovedIn;
    public static event System.Action<int> DayEnded;
    public static event System.Action<int, int> HeatChanged;
    public static event System.Action<int, int> RepChanged;

    public static event System.Action<int, int> HourChanged;
    public static event System.Action<ItemDef, int, int> InventoryChanged;
    public static event System.Action<FermentVat, VatState, VatState> VatStateChanged;
    public static event System.Action<FermentVat, float> BatchProgressed;
    public static event System.Action<FermentVat> RecipeSelectionRequested;
    public static event System.Action MenuCloseRequested;
    public static event System.Action<SellerType> SellerArrived;
    public static event System.Action<SellerType> SellerLeft;
    public static event System.Action<SellerType> SellMenuRequested;
    public static event System.Action<int> CurfewReached;
    public static event System.Action<int> SleepInitiated;
    public static event System.Action<int> SleepCompleted;

    public static event System.Action<Building, int, int> SmashHit;
    public static event System.Action<Building> DebrisDeposited;
    public static event System.Action<Building> HammerStarted;
    public static event System.Action<Building> HammerEnded;
    public static event System.Action<Building, float> HammerProgress;
    public static event System.Action<Building, int, int> RepairPointCompleted;

    public static void OnToastRequested(string message)
        => ToastRequested?.Invoke(message);

    public static void OnBuildingStateChanged(Building b, BuildingState oldState, BuildingState newState)
        => BuildingStateChanged?.Invoke(b, oldState, newState);

    public static void OnFragmentFound(object f)
        => FragmentFound?.Invoke(f);

    public static void OnResidentMovedIn(object r, Building b)
        => ResidentMovedIn?.Invoke(r, b);

    public static void OnDayEnded(int day)
        => DayEnded?.Invoke(day);

    public static void OnHeatChanged(int newHeat, int oldHeat)
        => HeatChanged?.Invoke(newHeat, oldHeat);

    public static void OnRepChanged(int newRep, int oldRep)
        => RepChanged?.Invoke(newRep, oldRep);

    public static void OnHourChanged(int hour, int day)
        => HourChanged?.Invoke(hour, day);

    public static void OnInventoryChanged(ItemDef def, int oldCount, int newCount)
        => InventoryChanged?.Invoke(def, oldCount, newCount);

    public static void OnVatStateChanged(FermentVat vat, VatState oldState, VatState newState)
        => VatStateChanged?.Invoke(vat, oldState, newState);

    public static void OnBatchProgressed(FermentVat vat, float progress)
        => BatchProgressed?.Invoke(vat, progress);

    public static void OnRecipeSelectionRequested(FermentVat vat)
        => RecipeSelectionRequested?.Invoke(vat);

    public static void OnMenuCloseRequested()
        => MenuCloseRequested?.Invoke();

    public static void OnSellerArrived(SellerType type)
        => SellerArrived?.Invoke(type);

    public static void OnSellerLeft(SellerType type)
        => SellerLeft?.Invoke(type);

    public static void OnSellMenuRequested(SellerType type)
        => SellMenuRequested?.Invoke(type);

    public static void OnCurfewReached(int day)
        => CurfewReached?.Invoke(day);

    public static void OnSleepInitiated(int day)
        => SleepInitiated?.Invoke(day);

    public static void OnSleepCompleted(int newDay)
        => SleepCompleted?.Invoke(newDay);

    public static void OnSmashHit(Building b, int done, int required)
        => SmashHit?.Invoke(b, done, required);

    public static void OnDebrisDeposited(Building b)
        => DebrisDeposited?.Invoke(b);

    public static void OnHammerStarted(Building b)
        => HammerStarted?.Invoke(b);

    public static void OnHammerEnded(Building b)
        => HammerEnded?.Invoke(b);

    public static void OnHammerProgress(Building b, float progress)
        => HammerProgress?.Invoke(b, progress);

    public static void OnRepairPointCompleted(Building b, int done, int total)
        => RepairPointCompleted?.Invoke(b, done, total);
}
