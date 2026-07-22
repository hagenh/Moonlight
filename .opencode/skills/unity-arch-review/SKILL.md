---
name: unity-arch-review
description: "Review code for Unity-specific architectural issues and project convention violations — singleton lifecycle, event bus hygiene, coroutine safety, Rules/ layer purity, FSM correctness, and more."
argument-hint: "[file or directory to review, blank for staged changes]"
---

## When to Use

- Before committing new code or a PR
- After implementing a new system or manager
- When debugging subtle Unity lifecycle issues
- As a quick sanity check on convention compliance

## Argument Parsing

| Token | Effect |
|-------|--------|
| `<file>` | Review a single file |
| `<directory>` | Review all `.cs` files in the directory |
| *(blank)* | Review git-staged changes (`git diff --cached`) |

## Workflow

### Stage 1: Determine Scope

1. If a file path is given, read that file
2. If a directory is given, glob `**/*.cs` in that directory
3. If blank, run `git diff --cached --name-only` to find staged `.cs` files
4. For each file in scope, read its contents

### Stage 2: Categorize Files

Classify each file by its architectural role:

| Category | Heuristic |
|----------|-----------|
| Rules/ | File is under `Assets/Scripts/Rules/` |
| Manager | Class name ends in `Manager` and inherits `MonoBehaviour` |
| PlayerState | Class inherits `PlayerState` |
| Interactable | Class implements `IInteractable` |
| UI | File is under `Assets/Scripts/UI/` |
| Def | Class name ends in `Def` |
| GameEvents | File is `GameEvents.cs` |
| Test | File is under `Assets/Tests/` |

### Stage 3: Run Checks

Read `~/.config/opencode/skills/unity-arch-review/references/unity-pitfalls.md` for the full catalog. For each file, apply the relevant checks:

#### All Files
- No comments in code (project convention)
- No `UnityEngine` using in Rules/ files (except `Mathf`)
- File is in the correct directory for its type
- Class name follows naming convention

#### Managers
- Singleton pattern: `Instance` property with `private set`, duplicate-destroy in `Awake()`
- Event subscriptions in `OnEnable`/`OnDisable` (not `Awake`/`OnDestroy`) — paired
- No direct calls to other managers for cross-system communication (use `GameEvents`)
- No UI rendering logic in manager (belongs in UI/ panel)
- Domain state is a private field of a Rules/ class, not raw primitives (when state is complex)

#### Rules/ Classes
- No `UnityEngine` types (except `Mathf`)
- Static classes only for rules (no instance methods)
- Sealed classes for state objects
- All randomness through `IRng` — never `Random.Range` or `UnityEngine.Random`
- No `MonoBehaviour` references

#### Player States
- Transitions use `ChangeState()` — never direct field mutation
- `Enter()`/`Exit()` are symmetrical (start something in Enter, stop in Exit)
- `UpdateFacingDirection()` called before transitioning in `OnMovePerformed`
- No Unity lifecycle methods (Start, Update, etc.) — use `LogicUpdate`/`PhysicsUpdate`

#### Interactables
- `InteractType` enum value exists for the class
- `Interact()` guards against null `PlayerController.Instance`
- Factory `Create()` method if objects are created at runtime
- Events published for side effects, not direct manager calls

#### UI Panels
- Subscribe in `OnEnable`, unsubscribe in `OnDisable` — always paired
- Close on Escape key in `Update()`
- Set `PlayerController.Instance.IsMenuOpen` when opening/closing
- Guard against null managers in `DrawWindow`
- No `Canvas` or `UI Toolkit` — IMGUI only

#### GameEvents
- Every `event` has a matching `OnXxx()` invoker
- Event parameter types are specific (not `object`)
- No lambda closures in event subscriptions that could leak

#### Tests
- `GameEventsReset.ClearAll()` in both `[SetUp]` and `[TearDown]`
- `TestBootstrap.DestroyAll()` in `[TearDown]`
- PlayMode tests use `[UnityTest]` and return `IEnumerator`
- `EventRecorder` for event assertions
- `StubRng`/`SeededRng` for randomness-dependent code
- Test names follow `Method_Condition_Expected`

### Stage 4: Report Findings

For each finding, report:

| Field | Content |
|-------|---------|
| **Severity** | P0 (will break), P1 (broken convention, may cause bugs), P2 (style/convention), P3 (advisory) |
| **File** | Path to the file |
| **Line** | Line number if applicable |
| **Category** | Which check category it falls under |
| **Issue** | What's wrong |
| **Fix** | Suggested fix |

Output as a markdown table, sorted by severity.

## Included References

- `unity-pitfalls.md` — Comprehensive catalog of Unity-specific pitfalls and anti-patterns
