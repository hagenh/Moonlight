using System.Collections;
using System.Reflection;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TryEnterForageStateTests
{
    private class FakeForageable : MonoBehaviour, IInteractable, IForageable
    {
        public bool IsHarvested { get; set; }
        public float SwingDuration => 3f;
        public ItemDef RequiredTool { get; set; }
        public int SwingsCompleted { get; private set; }

        public InteractType InteractType => InteractType.Forage;
        public bool CanInteract => true;

        public void Interact() { }
        public void CompleteSwing() => SwingsCompleted++;
    }

    private InventoryManager _inventory;
    private PlayerController _player;
    private FakeForageable _target;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _player = TestBootstrap.CreateSingleton<PlayerController>();
        _target = TestBootstrap.AddComponent<FakeForageable>();

        _recorder = new EventRecorder();
        GameEvents.ForageStarted += (t) => _recorder.Record("ForageStarted");
        GameEvents.ToastRequested += (msg) => _recorder.Record("ToastRequested", msg);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator EntersForageState_WhenNoToolRequired()
    {
        _target.RequiredTool = null;
        _player.CurrentInteractable = _target;

        InvokeInteract();

        Assert.AreEqual(1, _recorder.Count);
        Assert.AreEqual("ForageStarted", _recorder.Order[0]);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EntersForageState_WhenPlayerHasRequiredTool()
    {
        _target.RequiredTool = ContentDb.Pickaxe;
        _inventory.TryAdd(ContentDb.Pickaxe, 1);
        _player.CurrentInteractable = _target;

        InvokeInteract();

        Assert.AreEqual(1, _recorder.Count);
        Assert.AreEqual("ForageStarted", _recorder.Order[0]);
        yield return null;
    }

    [UnityTest]
    public IEnumerator FiresNeedToolToast_WhenToolMissing()
    {
        _target.RequiredTool = ContentDb.Pickaxe;
        _player.CurrentInteractable = _target;

        InvokeInteract();

        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("ToastRequested"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesNotEnterForageState_WhenAlreadyHarvested()
    {
        _target.IsHarvested = true;
        _player.CurrentInteractable = _target;

        InvokeInteract();

        bool forageStarted = false;
        foreach (var entry in _recorder.Order)
            if (entry.StartsWith("ForageStarted")) forageStarted = true;
        Assert.IsFalse(forageStarted);
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
