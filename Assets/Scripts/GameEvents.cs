public static class GameEvents
{
    public static event System.Action<string> ToastRequested;
    public static event System.Action<Building, BuildingState, BuildingState> BuildingStateChanged;
    public static event System.Action<ResidentDef, Building> ResidentMovedIn;
    public static event System.Action<int> DayEnded;
    public static event System.Action<int> CashChanged;
    public static event System.Action<int, int> RepChanged;

    public static event System.Action<int, int> HourChanged;
    public static event System.Action<ItemDef, int, int> InventoryChanged;
    public static event System.Action<FermentVat, VatState, VatState> VatStateChanged;
    public static event System.Action<FermentVat, float> BatchProgressed;
    public static event System.Action<FermentVat> RecipeSelectionRequested;
    public static event System.Action<string> RecipeDiscovered;
    public static event System.Action<int> HomesteadBuildStageChanged;
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

    public static event System.Action<IForageable> ForageStarted;
    public static event System.Action<IForageable> ForageEnded;
    public static event System.Action<IForageable, float> ForageProgress;

    public static event System.Action<ResidentDef, int> ResidentTeleported;
    public static event System.Action<ResidentDef> ResidentVisible;
    public static event System.Action<ResidentDef> ResidentHidden;
    public static event System.Action<ResidentDef, string> DialogueRequested;
    public static event System.Action DialogueClosed;
    public static event System.Action<ResidentDef, Building> MoveInSequenceStarted;
    public static event System.Action<ResidentDef, Building> MoveInSequenceCompleted;

    public static event System.Action<DeliveryType, ItemDef, int, int> DeliveryMade;

    public static event System.Action<StandRequest> RequestPosted;
    public static event System.Action<StandRequest, int> RequestFilled;
    public static event System.Action<StandRequest> RequestDeclined;
    public static event System.Action<ItemDef, int, int> ShelfSold;
    public static event System.Action RequestBookRequested;
    public static event System.Action RecipeBookRequested;
    public static event System.Action InventoryOpened;
    public static event System.Action InventoryClosed;
    public static event System.Action<ItemDef, int> InventoryFull;
    public static event System.Action<int, ItemDef, int> ItemDropped;
    public static event System.Action<int> ActiveSlotChanged;

    public static void OnToastRequested(string message)
        => ToastRequested?.Invoke(message);

    public static void OnBuildingStateChanged(Building b, BuildingState oldState, BuildingState newState)
        => BuildingStateChanged?.Invoke(b, oldState, newState);

    public static void OnResidentMovedIn(ResidentDef r, Building b)
        => ResidentMovedIn?.Invoke(r, b);

    public static void OnDayEnded(int day)
        => DayEnded?.Invoke(day);

    public static void OnCashChanged(int newCash)
        => CashChanged?.Invoke(newCash);

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

    public static void OnRecipeDiscovered(string recipeId)
        => RecipeDiscovered?.Invoke(recipeId);

    public static void OnHomesteadBuildStageChanged(int newStage)
        => HomesteadBuildStageChanged?.Invoke(newStage);

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

    public static void OnForageStarted(IForageable target)
        => ForageStarted?.Invoke(target);

    public static void OnForageEnded(IForageable target)
        => ForageEnded?.Invoke(target);

    public static void OnForageProgress(IForageable target, float progress)
        => ForageProgress?.Invoke(target, progress);

    public static void OnResidentTeleported(ResidentDef def, int hour)
        => ResidentTeleported?.Invoke(def, hour);

    public static void OnResidentVisible(ResidentDef def)
        => ResidentVisible?.Invoke(def);

    public static void OnResidentHidden(ResidentDef def)
        => ResidentHidden?.Invoke(def);

    public static void OnDialogueRequested(ResidentDef def, string line)
        => DialogueRequested?.Invoke(def, line);

    public static void OnDialogueClosed()
        => DialogueClosed?.Invoke();

    public static void OnMoveInSequenceStarted(ResidentDef def, Building b)
        => MoveInSequenceStarted?.Invoke(def, b);

    public static void OnMoveInSequenceCompleted(ResidentDef def, Building b)
        => MoveInSequenceCompleted?.Invoke(def, b);

    public static void OnDeliveryMade(DeliveryType type, ItemDef item, int count, int price)
        => DeliveryMade?.Invoke(type, item, count, price);

    public static void OnRequestPosted(StandRequest request)
        => RequestPosted?.Invoke(request);

    public static void OnRequestFilled(StandRequest request, int payment)
        => RequestFilled?.Invoke(request, payment);

    public static void OnRequestDeclined(StandRequest request)
        => RequestDeclined?.Invoke(request);

    public static void OnShelfSold(ItemDef item, int count, int payment)
        => ShelfSold?.Invoke(item, count, payment);

    public static void OnRequestBookRequested()
        => RequestBookRequested?.Invoke();

    public static void OnRecipeBookRequested()
        => RecipeBookRequested?.Invoke();

    public static void OnInventoryOpened()
        => InventoryOpened?.Invoke();

    public static void OnInventoryClosed()
        => InventoryClosed?.Invoke();

    public static void OnInventoryFull(ItemDef def, int overflow)
        => InventoryFull?.Invoke(def, overflow);

    public static void OnItemDropped(int slotIndex, ItemDef def, int count)
        => ItemDropped?.Invoke(slotIndex, def, count);

    public static void OnActiveSlotChanged(int index)
        => ActiveSlotChanged?.Invoke(index);
}
