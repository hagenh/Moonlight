# System Design Template

Fill in each section for the new system. This document serves as the implementation blueprint.

---

## System: [Name]

### Overview

1-3 sentence description of what the system does and why it exists.

### Responsibilities

- [What the system IS responsible for]
- [What the system is NOT responsible for]

### Boundaries

- Does NOT handle: [list things explicitly outside scope]
- Depends on: [list existing systems this integrates with]

---

## File List

| # | File | Type | Description |
|---|------|------|-------------|
| 1 | `Assets/Scripts/Rules/XxxState.cs` | Domain State | Pure C# state for [domain data] |
| 2 | `Assets/Scripts/Rules/XxxRules.cs` | Domain Rules | Static pure methods for [calculations/validation] |
| 3 | `Assets/Scripts/XxxManager.cs` | Manager Singleton | MonoBehaviour bridge between Unity and domain |
| 4 | `Assets/Scripts/Xxx.cs` | Interactable | IInteractable for player interaction |
| 5 | `Assets/Scripts/UI/XxxUI.cs` | UI Panel | IMGUI panel for [player actions] |
| 6 | `Assets/Scripts/GameEvents.cs` | Event Bus | New event declarations |
| 7 | `Assets/Tests/EditMode/XxxRulesTests.cs` | EditMode Tests | Pure logic tests |
| 8 | `Assets/Tests/PlayMode/XxxFlowTests.cs` | PlayMode Tests | Integration tests |

---

## Domain State: XxxState

```csharp
public sealed class XxxState
{
    public int SomeProperty { get; private set; }

    public XxxState(int initialValue)
    {
        SomeProperty = initialValue;
    }

    public int SetSomeProperty(int value)
    {
        int old = SomeProperty;
        SomeProperty = value;
        return old;
    }
}
```

---

## Domain Rules: XxxRules

```csharp
public static class XxxRules
{
    public const int SomeThreshold = 50;

    public static bool SomeCheck(int value)
    {
        return value >= SomeThreshold;
    }

    public static int CalculateResult(int input, IRng rng)
    {
        // Pure calculation, no side effects
        return input;
    }
}
```

---

## Manager: XxxManager

```csharp
public class XxxManager : MonoBehaviour
{
    public static XxxManager Instance { get; private set; }

    private XxxState _state;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _state = new XxxState();
    }

    private void OnEnable()
    {
        GameEvents.RelevantEvent += OnRelevantEventHandler;
    }

    private void OnDisable()
    {
        GameEvents.RelevantEvent -= OnRelevantEventHandler;
    }

    private void OnRelevantEventHandler(/* args */)
    {
        // Delegate to domain rules, publish events
    }
}
```

---

## Events

New events to add to `GameEvents.cs`:

```csharp
// Declarations
public static event System.Action<ArgType> XxxStarted;
public static event System.Action<ArgType> XxxCompleted;

// Invokers
public static void OnXxxStarted(ArgType arg) => XxxStarted?.Invoke(arg);
public static void OnXxxCompleted(ArgType arg) => XxxCompleted?.Invoke(arg);
```

---

## Event Flow

```
[Trigger] -> XxxManager.Method() -> XxxRules.Calculate() -> XxxState.Mutate()
         -> GameEvents.OnXxxStarted() -> [Subscribers react]
         -> GameEvents.OnXxxCompleted() -> [Subscribers react]
```

### Integration with Existing Systems

| This System | Direction | Existing System | Event/Method |
|-------------|-----------|-----------------|--------------|
| XxxManager | subscribes | TimeManager | `GameEvents.HourChanged` |
| XxxManager | publishes | GameHUD | `GameEvents.ToastRequested` |
| XxxManager | reads | GameManager | `GameManager.Instance.Cash` |

---

## Interactable (if player-facing)

```csharp
public class Xxx : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Xxx;

    public static Xxx Create(Vector3 position)
    {
        // Factory method
    }

    public void Interact()
    {
        if (PlayerController.Instance == null) return;
        GameEvents.OnXxxStarted();
    }
}
```

New `InteractType` enum value: `Xxx`

---

## UI Panel (if needed)

```csharp
public class XxxUI : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(0, 0, 300, 400);

    private void OnEnable()
    {
        GameEvents.XxxStarted += OnXxxStarted;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.XxxStarted -= OnXxxStarted;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    // ... standard IMGUI panel pattern
}
```

---

## Test Plan

### EditMode Tests (XxxRulesTests.cs)

| Test | Input | Expected |
|------|-------|----------|
| SomeCheck_BelowThreshold_ReturnsFalse | value=49 | false |
| SomeCheck_AtThreshold_ReturnsTrue | value=50 | true |
| SomeCheck_AboveThreshold_ReturnsTrue | value=51 | true |
| CalculateResult_ValidInput_ReturnsExpected | input=100, rng=StubRng(0.5f) | expected |

### PlayMode Tests (XxxFlowTests.cs)

| Test | Setup | Action | Expected |
|------|-------|--------|----------|
| FullFlow_StartToComplete | Create singletons | Trigger start | State changes, events fire |
| Method_InvalidState_ReturnsFalse | Wrong initial state | Call method | Returns false, no events |
| EventSequence_Order | Create singletons + recorder | Full flow | Recorder order matches |

### Singleton Dependencies (PlayMode setup order)

```
1. GameManager
2. InventoryManager (if needed)
3. TimeManager (if needed)
4. XxxManager
```

---

## Worked Example: Fermentation System

This example shows how an existing system maps to the template.

### Domain State: FermentBatch

- Tracks: recipe, start time, current progress
- Methods: `Advance()`, `IsComplete`

### Domain Rules: (inline in FermentBatch)

- Fermentation duration from RecipeData
- Progress = elapsed hours / total hours

### Manager: FermentManager

- Owns list of FermentVat components
- Subscribes to `HourChanged` to advance batches
- Publishes `VatStateChanged`, `BatchProgressed`

### Events

- `VatStateChanged(vat, oldState, newState)` — when a vat transitions
- `BatchProgressed(vat, progress)` — hourly progress update
- `RecipeSelectionRequested(vat)` — open recipe picker

### Interactable: FermentVat

- `InteractType.FermentVat`
- `Interact()` publishes `RecipeSelectionRequested` if vat is empty

### UI: RecipeSelectUI

- Opens on `RecipeSelectionRequested`
- Shows available recipes
- Player picks recipe → FermentManager starts batch

### Tests

- EditMode: `FermentBatchTests` — progress calculation, completion check
- PlayMode: `FermentationFlowTests` — full batch lifecycle, event sequences
