# Unity Pitfalls Catalog

## 1. Singleton Lifecycle

### Missing duplicate-destroy in Awake

**Wrong:**
```csharp
private void Awake()
{
    Instance = this;
}
```

**Right:**
```csharp
private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
}
```

**Why:** Scene reloads or duplicate GameObjects will overwrite the singleton reference, causing the old instance to become orphaned. The duplicate-destroy pattern ensures only one instance exists.

### Singleton not nulled on destroy

**Risk:** If a singleton GameObject is destroyed (not via duplicate-destroy), `Instance` still references the destroyed object. Accessing it returns `null` in Unity's override but `true` in C# null checks.

**Mitigation:** In this project, `TestBootstrap.DestroyAll()` handles cleanup via `ClearInstanceFields`. Production code doesn't destroy singletons at runtime.

### Accessing Instance in Awake before it's set

**Wrong:**
```csharp
private void Awake()
{
    Instance = this;
    // OtherManager.Instance might not be set yet!
    OtherManager.Instance.DoSomething();
}
```

**Right:** Use `GameEvents` for cross-system communication. If direct access is needed, do it in `Start()` (not `Awake()`) or on-demand.

---

## 2. Event Bus Hygiene

### Subscribe in Awake instead of OnEnable

**Wrong:**
```csharp
private void Awake()
{
    GameEvents.CashChanged += OnCashChanged;
}
```

**Right:**
```csharp
private void OnEnable()
{
    GameEvents.CashChanged += OnCashChanged;
}

private void OnDisable()
{
    GameEvents.CashChanged -= OnCashChanged;
}
```

**Why:** `Awake` is called once. If the GameObject is disabled/re-enabled, the subscription is lost. `OnEnable`/`OnDisable` are called every time, keeping subscriptions in sync.

### Unpaired subscribe/unsubscribe

Every `+=` in `OnEnable` must have a matching `-=` in `OnDisable` with the **same method reference**. Lambda subscriptions are un-unsubscribeable.

**Wrong:**
```csharp
private void OnEnable()
{
    GameEvents.CashChanged += (cash) => Debug.Log(cash);
}

// Can't unsubscribe this lambda!
```

**Right:**
```csharp
private void OnEnable()
{
    GameEvents.CashChanged += OnCashChanged;
}

private void OnDisable()
{
    GameEvents.CashChanged -= OnCashChanged;
}

private void OnCashChanged(int cash)
{
    Debug.Log(cash);
}
```

### Cross-manager direct calls

**Wrong:**
```csharp
public class SellManager : MonoBehaviour
{
    public void SellItem(ItemDef item)
    {
        GameManager.Instance.AddCash(price);  // Direct call!
        InventoryManager.Instance.Remove(item, 1);  // Direct call!
    }
}
```

**Right:**
```csharp
public class SellManager : MonoBehaviour
{
    public void SellItem(ItemDef item)
    {
        GameManager.Instance.AddCash(price);
        // Manager may call its own domain objects directly
        // But for cross-system communication, publish events:
        GameEvents.OnItemSold(item, price);
    }
}
```

**Exception:** A manager calling its own domain objects (e.g., `GameManager` calling `Economy.TrySpend()`) is fine. Direct calls to other managers' **state-querying** methods (read-only) are acceptable. Direct calls that **mutate** another manager's state should use events.

---

## 3. Coroutine Safety

### Dangling coroutines

**Wrong:**
```csharp
private void Start()
{
    StartCoroutine(SomeCoroutine());
}

// If this object is disabled/destroyed, the coroutine silently stops
// but no cleanup happens
```

**Right:**
```csharp
private Coroutine _activeRoutine;

private void OnDisable()
{
    if (_activeRoutine != null)
    {
        StopCoroutine(_activeRoutine);
        _activeRoutine = null;
    }
}

private void StartRoutine()
{
    if (_activeRoutine != null)
        StopCoroutine(_activeRoutine);
    _activeRoutine = StartCoroutine(SomeCoroutine());
}
```

### StartCoroutine without StopCoroutine

If you start a coroutine that could be started again before it finishes, always stop the previous one first.

### Yield on Destroyed objects

After `Destroy(gameObject)`, any coroutine owned by that object stops at the next yield. If other objects are waiting on that coroutine, they may hang.

---

## 4. Rules/ Layer Purity

### UnityEngine types in Rules/

**Wrong:**
```csharp
// In Assets/Scripts/Rules/
using UnityEngine;

public static class SomeRules
{
    public static Vector3 CalculatePosition(...)  // UnityEngine.Vector3
    ...
}
```

**Right:**
```csharp
// In Assets/Scripts/Rules/
using System;

public static class SomeRules
{
    public static int CalculateValue(...)
    ...
}
```

**Exception:** `Mathf` is acceptable in static rules methods (e.g., `Mathf.RoundToInt`).

### MonoBehaviour references in domain state

**Wrong:**
```csharp
public sealed class SomeState
{
    private PlayerController _player;  // MonoBehaviour reference!
}
```

**Right:**
```csharp
public sealed class SomeState
{
    private int _playerCash;  // Plain data
}
```

### Direct Random usage instead of IRng

**Wrong:**
```csharp
public static bool ShouldHappen()
{
    return Random.value < 0.5f;  // UnityEngine.Random — not testable!
}
```

**Right:**
```csharp
public static bool ShouldHappen(IRng rng)
{
    return rng.Value01() < 0.5f;  // Injected, testable with StubRng
}
```

---

## 5. Player State FSM

### Direct state field mutation

**Wrong:**
```csharp
public class PlayerController : MonoBehaviour
{
    private PlayerState _currentState;

    public void ForceIdle()
    {
        _currentState = new IdleState(this);  // Direct mutation!
    }
}
```

**Right:**
```csharp
public void ChangeState(PlayerState newState)
{
    _currentState?.Exit();
    _currentState = newState;
    _currentState.Enter();
}
```

### Enter/Exit asymmetry

If `Enter()` starts a coroutine or timer, `Exit()` must stop it.

**Wrong:**
```csharp
public override void Enter()
{
    _routine = controller.StartCoroutine(TimerRoutine());
}

public override void Exit()
{
    // Forgot to stop the coroutine!
}
```

**Right:**
```csharp
public override void Exit()
{
    if (_routine != null)
    {
        controller.StopCoroutine(_routine);
        _routine = null;
    }
}
```

### Missing UpdateFacingDirection before transition

When transitioning from Idle/Move on `OnMovePerformed`, always call `UpdateFacingDirection(input)` first so the facing direction is set before the new state reads it.

---

## 6. IInteractable

### Missing InteractType enum value

Every interactable must have a corresponding value in the `InteractType` enum. If you add a new interactable, add the enum value.

### Interact() without null guard

**Wrong:**
```csharp
public void Interact()
{
    PlayerController.Instance.PickUp(this);  // NullRef if no player!
}
```

**Right:**
```csharp
public void Interact()
{
    if (PlayerController.Instance == null) return;
    PlayerController.Instance.PickUp(this);
}
```

### Missing factory Create() for runtime-spawned interactables

Interactables created at runtime (debris, sellers, NPCs) must use a static `Create()` factory method. Never require manual prefab setup.

---

## 7. IMGUI UI

### Missing Escape key handling

Every UI panel must close on Escape:

```csharp
private void Update()
{
    if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
        Close();
}
```

### Missing IsMenuOpen toggle

Opening/closing a panel must toggle `PlayerController.Instance.IsMenuOpen` to prevent player movement while the menu is open.

### Null manager guard in DrawWindow

```csharp
private void DrawWindow(int id)
{
    if (XxxManager.Instance == null) return;  // Guard!
    // ... draw UI
}
```

---

## 8. Testing

### Missing GameEventsReset.ClearAll() in TearDown

**Wrong:**
```csharp
[TearDown]
public void TearDown()
{
    TestBootstrap.DestroyAll();
    // Missing GameEventsReset.ClearAll() — stale delegates leak between tests!
}
```

**Right:**
```csharp
[TearDown]
public void TearDown()
{
    TestBootstrap.DestroyAll();
    GameEventsReset.ClearAll();
}
```

### EditMode test using MonoBehaviour

Pure logic tests (Rules/ classes) should not create GameObjects or MonoBehaviours. If a test needs `TestBootstrap`, it should be a PlayMode test with `[UnityTest]`.

### PlayMode test without yield return null

`[UnityTest]` methods must return `IEnumerator` and yield at least once:

```csharp
[UnityTest]
public IEnumerator SomeTest()
{
    // ... assertions
    yield return null;
}
```

### Direct callback assertions instead of EventRecorder

**Wrong:**
```csharp
bool eventFired = false;
GameEvents.CashChanged += (_) => eventFired = true;
// ... act
Assert.IsTrue(eventFired);
```

**Right:**
```csharp
var recorder = new EventRecorder();
GameEvents.CashChanged += (cash) => recorder.Record("CashChanged", cash);
// ... act
Assert.AreEqual(1, recorder.Count);
```

**Why:** `EventRecorder` captures order and payloads, making sequence assertions possible and readable.

---

## 9. General Unity Pitfalls

### Comparing to null with == instead of Unity's null override

Unity overrides `== null` for destroyed objects. Use `== null` (not `ReferenceEquals` or `is null`) to check if a Unity object has been destroyed.

### DontDestroyOnLoad without duplicate handling

If using `DontDestroyOnLoad`, the singleton `Awake()` must still have the duplicate-destroy pattern, otherwise scene reloads create duplicates.

### Time.deltaTime in FixedUpdate

`Time.deltaTime` in `FixedUpdate` returns `Time.fixedDeltaTime`. While technically correct, be intentional about which you use.

### Camera.main in hot paths

`Camera.main` searches all GameObjects by tag. Cache the reference.

### FindObjectOfType in Awake/Start

Expensive search. Use singletons or serialized references instead.
