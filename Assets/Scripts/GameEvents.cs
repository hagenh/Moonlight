public static class GameEvents
{
    public static event System.Action<string> ToastRequested;
    public static event System.Action<Building, BuildingState, BuildingState> BuildingStateChanged;
    public static event System.Action<object> FragmentFound;
    public static event System.Action<object, Building> ResidentMovedIn;
    public static event System.Action<int> DayEnded;
    public static event System.Action<int, int> HeatChanged;
    public static event System.Action<int, int> RepChanged;

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
}
