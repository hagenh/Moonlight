using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private readonly List<Building> _buildings = new();

    public IReadOnlyList<Building> Buildings => _buildings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (var building in FindObjectsByType<Building>(FindObjectsSortMode.None))
            Register(building);
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
    }

    public void Register(Building building)
    {
        if (!_buildings.Contains(building))
            _buildings.Add(building);
    }

    public void Unregister(Building building)
    {
        _buildings.Remove(building);
    }

    public bool TryPurchase(Building building)
    {
        if (building.State != BuildingState.Abandoned) return false;

        if (!GameManager.Instance.TrySpend(building.purchaseCost))
        {
            GameEvents.OnToastRequested($"Can't afford {building.buildingName} ({building.purchaseCost}g)");
            return false;
        }

        var old = building.State;
        building.SetState(BuildingState.Cleared);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Purchased {building.buildingName} (-{building.purchaseCost}g)");
        return true;
    }

    public bool TryRepair(Building building)
    {
        if (building.State != BuildingState.Cleared) return false;

        if (!GameManager.Instance.TrySpend(building.repairCost))
        {
            GameEvents.OnToastRequested($"Can't repair {building.buildingName} ({building.repairCost}g)");
            return false;
        }

        var old = building.State;
        building.SetState(BuildingState.Restored);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Repaired {building.buildingName} (-{building.repairCost}g)");
        return true;
    }

    public bool CollectIncome(Building building)
    {
        if (building.State != BuildingState.Restored) return false;
        if (building.UncollectedIncome <= 0)
        {
            GameEvents.OnToastRequested($"No income at {building.buildingName}");
            return false;
        }

        int amount = building.UncollectedIncome;
        building.ResetIncome();
        GameManager.Instance.AddCash(amount);
        GameEvents.OnToastRequested($"Collected {amount}g from {building.buildingName}");
        return true;
    }

    private void OnDayEnded(int day)
    {
        foreach (var building in _buildings)
        {
            if (building.State == BuildingState.Restored)
            {
                building.AddDailyIncome();
            }
        }
    }
}
