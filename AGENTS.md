# Lamplight — Agent Instructions

## Project Identity

- **Name:** Lamplight (product name: Moonlighter)
- **Engine:** Unity 6 (6000.2.14f1), URP 17.2.0, 2D
- **Language:** C# (.cs)
- **Assembly:** `Lamplight.Runtime` (game code), `Lamplight.EditModeTests`, `Lamplight.PlayModeTests`, `Lamplight.TestSupport`
- **Namespace:** No root namespace; `Player.States` for player state classes, `Lamplight.TestSupport` for test utilities

## Architecture Layers

| Layer | Location | Pattern | Unity Dependency |
|-------|----------|---------|-----------------|
| Domain | `Assets/Scripts/Rules/` | Pure C# classes and static methods | None |
| Managers | `Assets/Scripts/` (root) | MonoBehaviour singletons | Full |
| Player | `Assets/Scripts/Player/` | FSM (State pattern) | Full |
| Interactables | `Assets/Scripts/` (root) | IInteractable implementations | Full |
| UI | `Assets/Scripts/UI/` | IMGUI (OnGUI) panels | Full |
| Data | `Assets/Scripts/` | Immutable def classes + ContentDb singleton | Full |

## Conventions

### Singleton Managers

Every manager follows this exact pattern:

```csharp
public class XxxManager : MonoBehaviour
{
    public static XxxManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
```

Naming: `XxxManager` for managers, `XxxDef` for immutable data definitions, `XxxRules` for static pure-logic classes, `XxxState` for player FSM states and domain state objects.

### Event Bus (GameEvents)

All cross-system communication goes through `GameEvents` — a static class with C# events and `OnXxx()` invoker methods.

```csharp
public static class GameEvents
{
    public static event System.Action<int> CashChanged;
    public static void OnCashChanged(int newCash) => CashChanged?.Invoke(newCash);
}
```

- Managers publish events via `GameEvents.OnXxx()`
- UI and other managers subscribe to events
- **Never** call another manager's methods directly for cross-system communication — use events
- Managers may call their own domain objects directly (e.g., `GameManager` calls `Economy.TrySpend()`)

### Rules/ Layer (Pure C#)

Domain logic lives in `Assets/Scripts/Rules/` as pure C# with zero Unity dependency:

- State classes: `EconomyState`, `Inventory`, `GameClock`, `FermentBatch` — sealed classes with private setters
- Rules classes: `EconomyRules`, `RenovationRules` — static classes with pure methods
- Interfaces: `IRng` with `UnityRng` (production) and `StubRng`/`SeededRng` (tests)
- **Never** reference `UnityEngine` types in Rules/ classes (except `Mathf` which is acceptable in static rules)

### Player FSM

Player states inherit `PlayerState` (abstract) and implement `Enter()`, `Exit()`, `LogicUpdate()`, `PhysicsUpdate()`:

```csharp
public class IdleState : PlayerState
{
    public IdleState(PlayerController controller) : base(controller) { }

    public override void Enter() { }
    public override void Exit() { }
    public override void LogicUpdate() { }
    public override void PhysicsUpdate() { }
}
```

State transitions use `ChangeState(newState)` — never mutate the current state field directly.

### IInteractable

Objects the player can interact with implement `IInteractable`:

```csharp
public interface IInteractable
{
    InteractType InteractType { get; }
    void Interact();
}
```

Interactables that need runtime creation use a static `Create()` factory method that programmatically builds the GameObject (sprite, collider, component).

### UI Panels

All UI uses IMGUI (`OnGUI`). Pattern:

- Subscribe to `GameEvents` in `OnEnable`, unsubscribe in `OnDisable`
- Toggle visibility via a `_visible` bool, set by event handlers
- Use `GUI.Window` with a `DrawWindow` callback
- Close on Escape key in `Update()`
- Set `PlayerController.Instance.IsMenuOpen` when opening/closing

The one exception is `GameHUD` which uses TextMeshPro for HUD elements.

### Content Database

Game data is hardcoded as static readonly fields in `ContentDb`:

```csharp
public static readonly ItemDef Grain = new ItemDef("grain", "Grain", true, 5);
```

New items/residents get a static readonly field + a `Register()` call in `Awake()`.

## File Placement

| Type | Path |
|------|------|
| Managers | `Assets/Scripts/XxxManager.cs` |
| Domain state | `Assets/Scripts/Rules/XxxState.cs` |
| Domain rules | `Assets/Scripts/Rules/XxxRules.cs` |
| Player states | `Assets/Scripts/Player/States/XxxState.cs` |
| Player enums | `Assets/Scripts/Player/Enums/` |
| Interactables | `Assets/Scripts/Xxx.cs` (root Scripts/) |
| UI panels | `Assets/Scripts/UI/XxxUI.cs` |
| Data definitions | `Assets/Scripts/XxxDef.cs` |
| Input | `Assets/Scripts/Input/` |
| EditMode tests | `Assets/Tests/EditMode/XxxTests.cs` |
| PlayMode tests | `Assets/Tests/PlayMode/XxxFlowTests.cs` |
| Test support | `Assets/Tests/Shared/` and `Assets/Tests/Shared/Fakes/` |

## Testing

### Test Infrastructure

- `TestBootstrap.CreateSingleton<T>()` — creates a MonoBehaviour singleton for testing
- `TestBootstrap.DestroyAll()` — cleans up all tracked GameObjects
- `EventRecorder` — records event names and payloads for assertions
- `GameEventsReset.ClearAll()` — clears all static event delegates between tests
- `StubRng` — deterministic RNG that returns queued values (defaults to 0)
- `SeededRng` — seeded RNG for reproducible random tests

### Test Conventions

- `[SetUp]`: Call `GameEventsReset.ClearAll()`, then create singletons via `TestBootstrap`
- `[TearDown]`: Call `TestBootstrap.DestroyAll()` then `GameEventsReset.ClearAll()`
- Test naming: `Method_Condition_Expected` (e.g., `TrySpend_InsufficientCash_ReturnsFalse`)
- EditMode tests for pure logic (Rules/ classes) — no MonoBehaviour needed
- PlayMode tests for integration — use `[UnityTest]` with `TestBootstrap`
- Use `EventRecorder` to verify event sequences, not direct callback assertions

### Running Tests

```bash
# Via Unity MCP (if server running)
# Use run_tests tool with assembly filter

# Via command line
# Unity.exe -runTests -testPlatform EditMode -testResults results.xml -projectPath .
```

## Hard Rules

- **No frameworks.** Hand-roll dialogue, quests, cutscenes. Extract patterns in game #2.
- **No ScriptableObjects** for game data. All content is hardcoded in `ContentDb`.
- **No dependency injection** (no Zenject/VContainer). Use singletons and events.
- **No direct cross-manager calls.** Use `GameEvents` for inter-system communication.
- **Rules/ must be pure C#.** No `UnityEngine` types except `Mathf`.
- **UI is IMGUI.** New panels use `OnGUI`, not uGUI Canvas or UI Toolkit.
- **No comments in code** unless explicitly requested.
