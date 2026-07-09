using System.Collections;
using System.Reflection;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TryEnterHammerStateTests
{
    private GameManager _gameManager;
    private InventoryManager _inventory;
    private BuildingManager _buildingManager;
    private Building _building;
    private PlayerController _player;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _buildingManager = TestBootstrap.CreateSingleton<BuildingManager>();

        var buildingGo = TestBootstrap.CreateGameObject("TestBuilding");
        _building = buildingGo.AddComponent<Building>();

        _player = TestBootstrap.CreateSingleton<PlayerController>();

        _recorder = new EventRecorder();
        GameEvents.HammerStarted += (b) => _recorder.Record("HammerStarted");
        GameEvents.HammerEnded += (b) => _recorder.Record("HammerEnded");
        GameEvents.ToastRequested += (msg) => _recorder.Record("ToastRequested", msg);
        GameEvents.BuildingStateChanged += (b, oldS, newS) =>
            _recorder.Record("BuildingStateChanged", $"{oldS}->{newS}");
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator EntersHammerState_WhenClearedAndHasMaterials()
    {
        _building.SetState(BuildingState.Cleared);
        _inventory.TryAdd(ContentDb.Timber, _building.TimberPerRepair);
        _inventory.TryAdd(ContentDb.Nails, _building.NailsPerRepair);
        _recorder.Clear();

        _player.CurrentInteractable = _building;
        InvokeInteract();

        Assert.AreEqual(1, _recorder.Count);
        Assert.AreEqual("HammerStarted", _recorder.Order[0]);
        yield return null;
    }

    [UnityTest]
    public IEnumerator FiresNeedMaterialsToast_WhenClearedButNoMaterials()
    {
        _building.SetState(BuildingState.Cleared);

        _player.CurrentInteractable = _building;
        InvokeInteract();

        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("ToastRequested"));
        Assert.IsTrue(_recorder.Order[0].Contains("Timber"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesNotEnterHammerState_WhenBuildingNotCleared()
    {
        _building.SetState(BuildingState.Abandoned);
        _inventory.TryAdd(ContentDb.Timber, 10);
        _inventory.TryAdd(ContentDb.Nails, 10);

        _player.CurrentInteractable = _building;
        InvokeInteract();

        bool hammerStarted = false;
        foreach (var entry in _recorder.Order)
        {
            if (entry.StartsWith("HammerStarted")) hammerStarted = true;
        }
        Assert.IsFalse(hammerStarted);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesNothing_WhenInteractableIsNull()
    {
        InvokeInteract();

        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }

    private void InvokeInteract()
    {
        var field = typeof(PlayerController).GetField("currentState",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var state = (PlayerState)field.GetValue(_player);
        state.OnInteractPerformed();
    }
}
