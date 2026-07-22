---
name: unity-test-scaffold
description: "Generate EditMode or PlayMode tests following the project's TestBootstrap, EventRecorder, GameEventsReset, and StubRng/SeededRng conventions."
argument-hint: "[editmode|playmode] <SystemName>"
---

## When to Use

When you need to write tests for a Lamplight system — a Rules/ class, a manager, a player state, or an interactable. This skill ensures tests follow the established conventions and use the correct test infrastructure.

## Argument Parsing

| Token | Effect |
|-------|--------|
| `editmode` | Generate an EditMode test (pure logic, no MonoBehaviour) |
| `playmode` | Generate a PlayMode test (integration, uses TestBootstrap + [UnityTest]) |
| `<SystemName>` | The class or system to test (e.g., `GuardManager`, `EconomyRules`, `IdleState`) |

If only a system name is given, infer the mode:
- Rules/ classes → `editmode`
- Managers, interactables, player states → `playmode`

## Workflow

### Stage 1: Locate Source

1. Search `Assets/Scripts/` for the target class file
2. Read the source file to understand:
   - Public API (methods, properties)
   - Dependencies (other managers, GameEvents, domain objects)
   - Whether it uses RNG (needs IRng/StubRng)
3. Check if tests already exist in `Assets/Tests/EditMode/` or `Assets/Tests/PlayMode/`

### Stage 2: Determine Test Archetype

Read `~/.config/opencode/skills/unity-test-scaffold/references/test-patterns.md` for full templates. Select the archetype:

| Target Type | Archetype | Mode | Key Pattern |
|-------------|-----------|------|-------------|
| Rules/ static class | Pure Logic | EditMode | No MonoBehaviour, direct method calls |
| Rules/ state class | Pure State | EditMode | Instantiate state, assert mutations |
| Manager singleton | Manager Integration | PlayMode | TestBootstrap.CreateSingleton, EventRecorder |
| Player state | State Transition | PlayMode | TestBootstrap, PlayerController setup, state change assertions |
| IInteractable | Interactable | PlayMode | TestBootstrap, interact + verify side effects |

### Stage 3: Generate Test File

1. Create the test class following the template from the reference file
2. Include:
   - Correct `using` statements (NUnit, UnityEngine, Lamplight.TestSupport)
   - `[SetUp]` with `GameEventsReset.ClearAll()` + singleton creation via `TestBootstrap`
   - `[TearDown]` with `TestBootstrap.DestroyAll()` then `GameEventsReset.ClearAll()`
   - `EventRecorder` subscriptions for any GameEvents the system fires
   - `StubRng` or `SeededRng` if the system uses `IRng`
   - Test methods named `Method_Condition_Expected`
3. For each public method on the target class, generate tests for:
   - Happy path (expected success)
   - Guard clause / invalid input (expected failure)
   - Edge cases (boundaries, empty state, max values)
4. Place the file:
   - EditMode: `Assets/Tests/EditMode/<SystemName>Tests.cs`
   - PlayMode: `Assets/Tests/PlayMode/<SystemName>FlowTests.cs`

### Stage 4: Event Coverage

For any new GameEvents the target fires:
- Subscribe with `EventRecorder` in `[SetUp]`
- Assert `_recorder.Order` contains expected event sequences
- Use `_recorder.Clear()` between logical test sections

### Stage 5: Review

Before finalizing, verify:
- No `UnityEngine` types in EditMode tests (except `UnityEngine.Assertions` or basic types)
- PlayMode tests use `[UnityTest]` and return `IEnumerator`
- Every `[SetUp]` has a matching `[TearDown]`
- `GameEventsReset.ClearAll()` appears in both `[SetUp]` and `[TearDown]`
- Test names follow `Method_Condition_Expected` pattern
- No test depends on execution order

## Included References

- `test-patterns.md` — Code templates for each test archetype with full examples
