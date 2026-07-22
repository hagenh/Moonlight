---
name: unity-system-design
description: "Design a complete new game system following the project's layered architecture — domain model, manager, events, interactables, UI, and tests."
argument-hint: "<SystemName> <brief description>"
---

## When to Use

When you need to add a new game system (not just a single script) — something that spans multiple architectural layers. For example: a weather system, a quest board, a crafting system, a festival event system.

For single scripts, use `unity-script-scaffold` instead.

## Argument Parsing

| Token | Effect |
|-------|--------|
| `<SystemName>` | PascalCase name for the system (e.g., `Weather`, `Festival`) |
| `<description>` | Brief description of what the system does |

## Workflow

### Stage 1: Research Existing Systems

1. Search `Assets/Scripts/` for similar systems (same domain, similar scope)
2. Read the manager, rules, and test files for the most similar existing system
3. Identify integration points: which existing managers/events does this system need to interact with?
4. Check `Assets/Docs/BuildPlan.md` for planned features that this system may relate to

### Stage 2: Design Domain Layer

Design the pure C# domain model in `Rules/`:

1. **State class** (`XxxState.cs` in `Rules/`):
   - Sealed class with private setters
   - Immutable from outside — mutations only through methods that return old values
   - No Unity types
   - Constructor takes initial values

2. **Rules class** (`XxxRules.cs` in `Rules/`):
   - Static class with pure methods
   - All randomness through `IRng`
   - Constants for thresholds and multipliers
   - No side effects — returns results, doesn't mutate

### Stage 3: Design Manager Layer

Design the MonoBehaviour singleton manager:

1. **Class** (`XxxManager.cs` in `Scripts/`):
   - Singleton pattern with duplicate-destroy
   - Owns a private instance of the domain state class
   - Public methods delegate to domain state/rules
   - Publishes events via `GameEvents.OnXxx()` after mutations
   - Subscribes to relevant events in `OnEnable`/`OnDisable`

2. **Integration points:**
   - Which events does it subscribe to?
   - Which events does it publish?
   - Which other managers' state does it read (via `Instance`)?

### Stage 4: Design Events

List new events to add to `GameEvents.cs`:

```
Event Name                     | Type                          | Published By      | Subscribed By
-------------------------------|-------------------------------|-------------------|---------------
XxxStarted                     | Action<args>                  | XxxManager        | XxxUI, GameHUD
XxxCompleted                   | Action<args>                  | XxxManager        | XxxManager, GameHUD
```

### Stage 5: Design Interactable (if player-facing)

If the player interacts with this system:

1. Which `InteractType` enum value?
2. Does it need a factory `Create()` method?
3. What happens in `Interact()`?
4. What events does it publish?

### Stage 6: Design UI (if needed)

If the system needs a UI panel:

1. IMGUI or HUD element?
2. What events trigger opening/closing?
3. What data does it display?
4. What actions can the player take?

### Stage 7: Design Tests

1. **EditMode tests** for Rules/ classes:
   - Test every public method
   - Test boundary conditions
   - Use `StubRng` for randomness

2. **PlayMode tests** for integration:
   - Test manager methods with `TestBootstrap`
   - Verify event sequences with `EventRecorder`
   - Test full flow (setup → act → assert)

### Stage 8: Generate Design Document

Produce a design document using the template from `~/.config/opencode/skills/unity-system-design/references/system-template.md`. The document includes:

1. System overview and responsibilities
2. File list with paths
3. Class/API signatures
4. Event flow
5. Integration points with existing systems
6. Test plan

### Stage 9: Scaffold Files (optional)

If the user confirms the design, use `unity-script-scaffold` to generate the actual files.

## Included References

- `system-template.md` — Template for the system design document
