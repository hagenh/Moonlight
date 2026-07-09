using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BuildingRenovationFlowTests
{
    private GameManager _gameManager;
    private InventoryManager _inventory;
    private BuildingManager _buildingManager;
    private Building _building;
    private TimeManager _timeManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _buildingManager = TestBootstrap.CreateSingleton<BuildingManager>();
        _timeManager = TestBootstrap.CreateSingleton<TimeManager>();

        _recorder = new EventRecorder();
        GameEvents.BuildingStateChanged += (b, oldS, newS) =>
            _recorder.Record("BuildingStateChanged", $"{oldS}->{newS}");
        GameEvents.SmashHit += (b, done, req) => _recorder.Record("SmashHit", $"{done}/{req}");
        GameEvents.RepairPointCompleted += (b, done, total) =>
            _recorder.Record("RepairPointCompleted", $"{done}/{total}");
        GameEvents.DayEnded += (day) => _recorder.Record("DayEnded", day);

        var buildingGo = TestBootstrap.CreateGameObject("TestBuilding");
        _building = buildingGo.AddComponent<Building>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator FullRenovationLoop_AbandonedToRestoredToIncome()
    {
        Assert.AreEqual(BuildingState.Abandoned, _building.State);
        int cashBefore = _gameManager.Cash;

        bool purchased = _buildingManager.TryPurchase(_building);
        Assert.IsTrue(purchased);
        Assert.AreEqual(BuildingState.Purchased, _building.State);
        Assert.AreEqual(cashBefore - _building.PurchaseCost, _gameManager.Cash);

        int smashRequired = _building.SmashHitsRequired;
        for (int i = 0; i < smashRequired; i++)
        {
            _buildingManager.TrySmashHit(_building);
        }

        if (_building.State != BuildingState.Cleared)
        {
            _buildingManager.ForceCompleteSmash(_building);
        }
        Assert.AreEqual(BuildingState.Cleared, _building.State);

        _inventory.TryAdd(ContentDb.Timber, _building.TotalRepairPoints * _building.TimberPerRepair);
        _inventory.TryAdd(ContentDb.Nails, _building.TotalRepairPoints * _building.NailsPerRepair);

        int timberBefore = _inventory.GetCount(ContentDb.Timber);
        int nailsBefore = _inventory.GetCount(ContentDb.Nails);

        int repairTotal = _building.TotalRepairPoints;
        for (int i = 0; i < repairTotal; i++)
        {
            _buildingManager.TryHammerHit(_building);
        }

        Assert.AreEqual(BuildingState.Restored, _building.State);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Timber));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));

        _recorder.Clear();
        _timeManager.AdvanceToDayEnd();

        bool foundDayEnded = false;
        foreach (var entry in _recorder.Order)
        {
            if (entry.StartsWith("DayEnded")) foundDayEnded = true;
        }
        Assert.IsTrue(foundDayEnded);

        int cashAfterRestored = _gameManager.Cash;
        bool collected = _buildingManager.CollectIncome(_building);
        Assert.IsTrue(collected);
        Assert.Greater(_gameManager.Cash, cashAfterRestored);

        yield return null;
    }

    [UnityTest]
    public IEnumerator TryPurchase_InsufficientCash_ReturnsFalse()
    {
        _gameManager.SetReputation(0);
        while (_gameManager.Cash > 0)
            _gameManager.TrySpend(_gameManager.Cash);

        bool result = _buildingManager.TryPurchase(_building);

        Assert.IsFalse(result);
        Assert.AreEqual(BuildingState.Abandoned, _building.State);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TrySmashHit_WrongState_ReturnsFalse()
    {
        bool result = _buildingManager.TrySmashHit(_building);

        Assert.IsFalse(result);
        Assert.AreEqual(BuildingState.Abandoned, _building.State);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CanHammer_FalseWhenNoMaterials()
    {
        _buildingManager.ForceCompleteSmash(_building);
        _building.SetState(BuildingState.Cleared);

        bool result = _buildingManager.CanHammer(_building);

        Assert.IsFalse(result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CollectIncome_NotRestored_ReturnsFalse()
    {
        bool result = _buildingManager.CollectIncome(_building);

        Assert.IsFalse(result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CollectIncome_RestoredButNoIncome_ReturnsFalse()
    {
        _buildingManager.ForceCompleteRepair(_building);

        bool result = _buildingManager.CollectIncome(_building);

        Assert.IsFalse(result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CollectIncome_AfterDayEnd_Succeeds()
    {
        _building.SetState(BuildingState.Cleared);
        _buildingManager.ForceCompleteRepair(_building);
        Assert.AreEqual(BuildingState.Restored, _building.State);

        _timeManager.AdvanceToDayEnd();

        int cashBefore = _gameManager.Cash;
        bool result = _buildingManager.CollectIncome(_building);

        Assert.IsTrue(result);
        Assert.Greater(_gameManager.Cash, cashBefore);
        yield return null;
    }
}
