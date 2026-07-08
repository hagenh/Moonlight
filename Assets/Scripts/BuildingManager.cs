using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private readonly List<Building> _buildings = new();
    private readonly List<Debris> _activeDebris = new();
    private readonly Dictionary<Building, List<Debris>> _buildingDebris = new();

    public IReadOnlyList<Building> Buildings => _buildings;
    public IReadOnlyList<Debris> ActiveDebris => _activeDebris;

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
        building.SetState(BuildingState.Purchased);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Purchased {building.buildingName} (-{building.purchaseCost}g)");
        return true;
    }

    public bool TrySmashHit(Building building)
    {
        if (building.State != BuildingState.Purchased) return false;
        if (building.BoardsSmashed) return false;

        building.IncrementSmashHits();
        int done = building.SmashHitsDone;
        int required = building.smashHitsRequired;

        GameEvents.OnSmashHit(building, done, required);

        if (done < required)
        {
            building.StartPunchScalePublic();
            GameEvents.OnToastRequested($"Smash! ({done}/{required})");
            return true;
        }

        building.SetBoardsSmashed();

        if (building.isFacadeOnly)
        {
            var old = building.State;
            building.SetState(BuildingState.Cleared);
            GameEvents.OnBuildingStateChanged(building, old, building.State);
            GameEvents.OnToastRequested($"Cleared {building.buildingName}!");
        }
        else
        {
            SpawnDebris(building);
            GameEvents.OnToastRequested("Boards cleared! Carry debris to the pile.");
        }

        return true;
    }

    public bool CanHammer(Building building)
    {
        if (building.State != BuildingState.Cleared) return false;
        if (building.RepairPointsDone >= building.totalRepairPoints) return false;
        if (!InventoryManager.Instance.Has(ContentDb.Timber, building.timberPerRepair))
            return false;
        if (!InventoryManager.Instance.Has(ContentDb.Nails, building.nailsPerRepair))
            return false;
        return true;
    }

    public bool TryHammerHit(Building building)
    {
        if (building.State != BuildingState.Cleared) return false;

        if (!InventoryManager.Instance.Has(ContentDb.Timber, building.timberPerRepair)
            || !InventoryManager.Instance.Has(ContentDb.Nails, building.nailsPerRepair))
        {
            GameEvents.OnToastRequested(
                $"Need {building.timberPerRepair} Timber & {building.nailsPerRepair} Nails");
            return false;
        }

        InventoryManager.Instance.TryRemove(ContentDb.Timber, building.timberPerRepair);
        InventoryManager.Instance.TryRemove(ContentDb.Nails, building.nailsPerRepair);

        building.OnRepairPointCompleted();

        int done = building.RepairPointsDone;
        int total = building.totalRepairPoints;

        GameEvents.OnRepairPointCompleted(building, done, total);

        if (done >= total)
        {
            var old = building.State;
            building.SetState(BuildingState.Restored);
            GameEvents.OnBuildingStateChanged(building, old, building.State);
            GameEvents.OnToastRequested($"Restored {building.buildingName}!");
        }
        else
        {
            building.StartPunchScalePublic();
            GameEvents.OnToastRequested($"Repaired ({done}/{total})");
        }

        return true;
    }

    public void OnDebrisCleared(Building building)
    {
        if (building.DebrisRemaining > 0) return;

        CleanupDebris(building);

        var old = building.State;
        building.SetState(BuildingState.Cleared);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Cleared {building.buildingName}!");
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

    private void SpawnDebris(Building building)
    {
        Vector3 basePos = building.BoardTrigger != null
            ? building.BoardTrigger.transform.position
            : building.transform.position;

        Vector3 outward = building.transform.position - basePos;
        outward.z = 0f;
        if (outward.magnitude > 0.01f)
            outward = outward.normalized;
        else
            outward = new Vector3(-1f, 0f, 0f);

        Collider2D col = building.GetComponent<Collider2D>();
        float edge = col != null
            ? col.bounds.extents.x
            : building.transform.lossyScale.x * 0.5f;
        Vector3 spawnOrigin = building.transform.position - outward * (edge + 1.5f);

        if (!_buildingDebris.ContainsKey(building))
            _buildingDebris[building] = new List<Debris>();

        for (int i = 0; i < building.debrisCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.4f, 0f),
                0f);
            var debris = Debris.Create(building, spawnOrigin + offset);
            _activeDebris.Add(debris);
            _buildingDebris[building].Add(debris);
        }

        building.SetDebrisRemaining(building.debrisCount);
    }

    public void CleanupDebris(Building building)
    {
        if (!_buildingDebris.TryGetValue(building, out var list)) return;
        foreach (var debris in list)
        {
            if (debris != null)
            {
                _activeDebris.Remove(debris);
                Destroy(debris.gameObject);
            }
        }
        list.Clear();
    }

    public void ForceCompleteSmash(Building building)
    {
        if (building.State != BuildingState.Purchased) return;
        building.SetBoardsSmashed();
        building.SetDebrisRemaining(0);
        CleanupDebris(building);
        var old = building.State;
        building.SetState(BuildingState.Cleared);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
    }

    public void ForceCompleteRepair(Building building)
    {
        if (building.State != BuildingState.Cleared) return;
        var old = building.State;
        building.SetState(BuildingState.Restored);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
    }

    public void ResetBuilding(Building building)
    {
        CleanupDebris(building);
        building.ResetRenovation();
        var old = building.State;
        building.SetState(BuildingState.Abandoned);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
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
