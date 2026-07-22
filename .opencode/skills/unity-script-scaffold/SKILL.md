---
name: unity-script-scaffold
description: "Scaffold new C# scripts following the project's singleton manager, GameEvents, Rules/ pure logic, FSM state, IInteractable, IMGUI UI, and ContentDb conventions."
argument-hint: "<type> <Name> [options]"
---

## When to Use

When you need to create a new script for the Lamplight project — a manager, player state, interactable, UI panel, rules class, or data definition. This skill ensures new code follows established patterns and is placed in the correct directory.

## Argument Parsing

| Token | Effect |
|-------|--------|
| `manager` | Scaffold a MonoBehaviour singleton manager |
| `state` | Scaffold a PlayerState subclass |
| `interactable` | Scaffold an IInteractable implementation |
| `uipanel` | Scaffold an IMGUI OnGUI panel |
| `rules` | Scaffold a static pure-logic rules class |
| `def` | Scaffold an immutable data definition class |
| `<Name>` | Class name (e.g., `WeatherManager`, `CraftState`) |

## Workflow

### Stage 1: Parse and Validate

1. Parse `<type>` and `<Name>` from arguments
2. Validate `<Name>` follows naming convention:
   - Managers: `XxxManager`
   - States: `XxxState`
   - Rules: `XxxRules`
   - Defs: `XxxDef`
   - UI: `XxxUI`
   - Interactables: descriptive noun (e.g., `Debris`, `Crate`)
3. If name doesn't match convention, auto-fix and note the change

### Stage 2: Load Template

Read `~/.config/opencode/skills/unity-script-scaffold/references/scaffold-templates.md` for the matching template.

### Stage 3: Generate Script

Generate the script from the template, filling in:
- Class name
- Namespace (if applicable — `Player.States` for player states)
- Required `using` statements
- Event declarations to add to `GameEvents.cs`
- ContentDb registrations (for def types)

### Stage 4: Update GameEvents

If the new script needs new events:

1. Read `Assets/Scripts/GameEvents.cs`
2. Add the event declaration in the declarations section:
   ```csharp
   public static event System.Action<ArgType> EventName;
   ```
3. Add the invoker method in the invoker section:
   ```csharp
   public static void OnEventName(ArgType arg) => EventName?.Invoke(arg);
   ```
4. Keep alphabetical grouping within each section

### Stage 5: Update ContentDb (def types only)

If scaffolding a `def` type:

1. Read `Assets/Scripts/ContentDb.cs`
2. Add a `public static readonly XxxDef` field
3. Add a `Register()` call in `Awake()`

### Stage 6: Place File

| Type | Directory |
|------|-----------|
| manager | `Assets/Scripts/XxxManager.cs` |
| state | `Assets/Scripts/Player/States/XxxState.cs` |
| interactable | `Assets/Scripts/Xxx.cs` |
| uipanel | `Assets/Scripts/UI/XxxUI.cs` |
| rules | `Assets/Scripts/Rules/XxxRules.cs` |
| def | `Assets/Scripts/XxxDef.cs` |

### Stage 7: Verify

- File compiles (check via Unity MCP `validate_script` if available, otherwise manual review)
- No `UnityEngine` references in Rules/ classes
- Event declarations match invoker signatures
- Singleton pattern matches the exact boilerplate
- No comments in generated code

## Included References

- `scaffold-templates.md` — Code templates for each of the 6 script types
