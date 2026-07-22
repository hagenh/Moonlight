# Test Pattern Templates

## 1. Pure Logic Test (Rules/ static classes)

For: `EconomyRules`, `RenovationRules`, and other static rule classes.

**No MonoBehaviour needed. No TestBootstrap. No GameEventsReset in TearDown (no static state).**

```csharp
using NUnit.Framework;

public class XxxRulesTests
{
    private ItemDef _testItem;

    [SetUp]
    public void SetUp()
    {
        _testItem = new ItemDef("test", "Test Item", true, 10);
    }

    [Test]
    public void Method_Condition_Expected()
    {
        int result = XxxRules.SomeMethod(_testItem, someParam);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Method_EdgeCase_Expected()
    {
        // boundary test
    }
}
```

**Key points:**
- Direct method calls on static rules class
- Create test data inline in `[SetUp]` (ItemDef, etc.)
- Use `StubRng` if the method takes `IRng`:
  ```csharp
  var rng = new StubRng(0.5f); // queues 0.5 for next Value01()
  ```

---

## 2. Pure State Test (Rules/ state classes)

For: `EconomyState`, `Inventory`, `GameClock`, `FermentBatch` — sealed classes with mutable state.

**No MonoBehaviour needed. But use GameEventsReset if state changes trigger events indirectly.**

```csharp
using NUnit.Framework;

public class XxxStateTests
{
    private XxxState _state;

    [SetUp]
    public void SetUp()
    {
        _state = new XxxState(initialValue);
    }

    [Test]
    public void TrySpend_InsufficientFunds_ReturnsFalse()
    {
        bool result = _state.TrySpend(999);
        Assert.IsFalse(result);
    }

    [Test]
    public void TrySpend_SufficientFunds_DeductsAndReturnsTrue()
    {
        bool result = _state.TrySpend(10);
        Assert.IsTrue(result);
        Assert.AreEqual(initialValue - 10, _state.SomeProperty);
    }
}
```

**Key points:**
- Instantiate state objects directly
- Assert property values after mutations
- Test both success and failure paths for TryXxx methods
- Test boundary values (0, max, negative input)

---

## 3. Manager Integration Test (PlayMode)

For: `GameManager`, `BuildingManager`, `FermentManager`, `SellManager`, etc.

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class XxxManagerFlowTests
{
    private XxxManager _manager;
    private GameManager _gameManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _manager = TestBootstrap.CreateSingleton<XxxManager>();
        _recorder = new EventRecorder();

        GameEvents.SomeEvent += (args) => _recorder.Record("SomeEvent", args);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator Method_Condition_Expected()
    {
        _manager.DoSomething();

        Assert.AreEqual(expected, _manager.SomeProperty);
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("SomeEvent"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Method_InvalidState_ReturnsFalse()
    {
        bool result = _manager.TrySomething(invalidInput);
        Assert.IsFalse(result);
        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }
}
```

**Key points:**
- Always create `GameManager` first if the manager depends on economy
- Create `InventoryManager` if the system uses inventory
- Create `TimeManager` if the system depends on time
- Use `EventRecorder` to verify events, not direct callback assertions
- Use `_recorder.Clear()` between logical sections within a test
- Always `yield return null` at end of `[UnityTest]`
- Create dependent singletons in the order they're needed

---

## 4. Player State Transition Test (PlayMode)

For: Player FSM states — `IdleState`, `MoveState`, `InteractState`, etc.

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class XxxStateTests
{
    private PlayerController _player;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _player = TestBootstrap.CreateSingleton<PlayerController>();
        _recorder = new EventRecorder();

        GameEvents.SomeEvent += (args) => _recorder.Record("SomeEvent");
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator Enter_SetsAnimatorTrigger()
    {
        // Setup state
        var state = new XxxState(_player);
        _player.ChangeState(state);

        // Assert initial state
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnMovePerformed_TransitionsToMoveState()
    {
        var idleState = new IdleState(_player);
        _player.ChangeState(idleState);

        idleState.OnMovePerformed(Vector2.right);

        // Assert state changed
        yield return null;
    }
}
```

**Key points:**
- Create `PlayerController` via `TestBootstrap.CreateSingleton`
- State transitions tested by calling input callbacks
- Verify the new state type after transitions
- May need to set up `CurrentInteractable` for interact states

---

## Common Patterns

### StubRng Usage

```csharp
var rng = new StubRng(0.5f);              // Next Value01() returns 0.5
var rng = new StubRng(0.9f, 0.1f);        // Queue: 0.9, then 0.1
var rng = new StubRng();                   // All Value01() returns 0f (default)
```

### SeededRng Usage

```csharp
var rng = new SeededRng(42);              // Deterministic sequence from seed 42
```

### EventRecorder Patterns

```csharp
// Subscribe
GameEvents.CashChanged += (cash) => _recorder.Record("CashChanged", cash);

// Assert count
Assert.AreEqual(2, _recorder.Count);

// Assert order
Assert.AreEqual("CashChanged: 400", _recorder.Order[0]);

// Assert sequence
StringAssert.Contains("CashChanged", _recorder.Sequence);

// Clear between sections
_recorder.Clear();
```

### Multiple Singleton Setup

Create singletons in dependency order. Common dependency chains:

```
GameManager → (standalone, create first)
InventoryManager → (depends on GameManager for cash)
BuildingManager → (depends on GameManager + InventoryManager)
TimeManager → (standalone)
FermentManager → (depends on InventoryManager + TimeManager)
SellManager → (depends on GameManager + InventoryManager)
```
