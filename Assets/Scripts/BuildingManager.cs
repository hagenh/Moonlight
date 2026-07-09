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
        if (!RenovationRules.CanPurchase(building.State)) return false;

        if (!GameManager.Instance.TrySpend(building.PurchaseCost))
        {
            GameEvents.OnToastRequested($"Can't afford {building.BuildingName} ({building.PurchaseCost}g)");
            return false;
        }

        var old = building.State;
        building.SetState(BuildingState.Purchased);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Purchased {building.BuildingName} (-{building.PurchaseCost}g)");
        return true;
    }

    public bool TrySmashHit(Building building)
    {
        if (!RenovationRules.CanSmash(building.State, building.BoardsSmashed)) return false;

        building.IncrementSmashHits();
        int done = building.SmashHitsDone;
        int required = building.SmashHitsRequired;

        GameEvents.OnSmashHit(building, done, required);

        if (!RenovationRules.IsSmashComplete(done, required))
        {
            building.StartPunchScalePublic();
            GameEvents.OnToastRequested($"Smash! ({done}/{required})");
            return true;
        }

        building.SetBoardsSmashed();

        if (building.IsFacadeOnly)
        {
            var old = building.State;
            building.SetState(BuildingState.Cleared);
            GameEvents.OnBuildingStateChanged(building, old, building.State);
            GameEvents.OnToastRequested($"Cleared {building.BuildingName}!");
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
        bool hasTimber = InventoryManager.Instance.Has(ContentDb.Timber, building.TimberPerRepair);
        bool hasNails = InventoryManager.Instance.Has(ContentDb.Nails, building.NailsPerRepair);
        return RenovationRules.CanHammer(
            building.State, building.RepairPointsDone, building.TotalRepairPoints,
            hasTimber, hasNails);
    }

    public bool TryHammerHit(Building building)
    {
        if (building.State != BuildingState.Cleared) return false;

        bool hasTimber = InventoryManager.Instance.Has(ContentDb.Timber, building.TimberPerRepair);
        bool hasNails = InventoryManager.Instance.Has(ContentDb.Nails, building.NailsPerRepair);
        if (!hasTimber || !hasNails)
        {
            GameEvents.OnToastRequested(
                $"Need {building.TimberPerRepair} Timber & {building.NailsPerRepair} Nails");
            return false;
        }

        InventoryManager.Instance.TryRemove(ContentDb.Timber, building.TimberPerRepair);
        InventoryManager.Instance.TryRemove(ContentDb.Nails, building.NailsPerRepair);

        building.OnRepairPointCompleted();

        int done = building.RepairPointsDone;
        int total = building.TotalRepairPoints;

        GameEvents.OnRepairPointCompleted(building, done, total);

        if (RenovationRules.IsRepairComplete(done, total))
        {
            var old = building.State;
            building.SetState(BuildingState.Restored);
            GameEvents.OnBuildingStateChanged(building, old, building.State);
            GameEvents.OnToastRequested($"Restored {building.BuildingName}!");
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
        if (!RenovationRules.IsDebrisCleared(building.DebrisRemaining)) return;

        CleanupDebris(building);

        var old = building.State;
        building.SetState(BuildingState.Cleared);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Cleared {building.BuildingName}!");
    }

    public bool CollectIncome(Building building)
    {
        if (!RenovationRules.CanCollectIncome(building.State, building.UncollectedIncome))
        {
            if (building.State == BuildingState.Restored)
                GameEvents.OnToastRequested($"No income at {building.BuildingName}");
            return false;
        }

        int amount = building.UncollectedIncome;
        building.ResetIncome();
        GameManager.Instance.AddCash(amount);
        GameEvents.OnToastRequested($"Collected {amount}g from {building.BuildingName}");
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

        for (int i = 0; i < building.DebrisCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.4f, 0f),
                0f);
            var debris = Debris.Create(building, spawnOrigin + offset);
            _activeDebris.Add(debris);
            _buildingDebris[building].Add(debris);
        }

        building.SetDebrisRemaining(building.DebrisCount);
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
