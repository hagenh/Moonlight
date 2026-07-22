# Scaffold Templates

## 1. Manager Singleton

**File:** `Assets/Scripts/XxxManager.cs`

```csharp
using UnityEngine;

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
    }
}
```

**When to add:**
- New game system that owns domain state
- Needs Unity lifecycle (Update, coroutines, physics)
- Needs to subscribe to GameEvents

**Dependencies to inject:**
- Domain state: private field of a Rules/ state class
- Other managers: access via `XxxManager.Instance` only for reading state
- Cross-system communication: publish events via `GameEvents.OnXxx()`

---

## 2. Player FSM State

**File:** `Assets/Scripts/Player/States/XxxState.cs`

```csharp
using Player.States;
using UnityEngine;

public class XxxState : PlayerState
{
    public XxxState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.SetAnimatorTrigger(AnimatorParams.Idle);
        controller.RB.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
    }

    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnMovePerformed(Vector2 input)
    {
        UpdateFacingDirection(input);
        ChangeState(new MoveState(controller));
    }

    public override void OnMoveCanceled()
    {
        ChangeState(new IdleState(controller));
    }

    public override void OnInteractPerformed()
    {
        ChangeState(new InteractState(controller));
    }
}
```

**When to add:**
- New player behavior (e.g., swimming, climbing, fishing)
- Distinct movement/interaction mode

**Key rules:**
- Always call `UpdateFacingDirection(input)` before transitioning in `OnMovePerformed`
- Use `ChangeState(newState)` for all transitions — never assign directly
- `Enter()` should set animator triggers and zero velocity if the state is stationary
- `Exit()` should clean up any state (stop coroutines, reset flags)

---

## 3. IInteractable Implementation

**File:** `Assets/Scripts/Xxx.cs`

```csharp
using UnityEngine;

public class Xxx : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Xxx;

    public static Xxx Create(Vector3 position)
    {
        var go = new GameObject("Xxx");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = Color.white;
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);
        go.layer = LayerMask.NameToLayer("Interactable");

        var comp = go.AddComponent<Xxx>();
        return comp;
    }

    public void Interact()
    {
        if (PlayerController.Instance == null) return;

        GameEvents.OnXxxHappened();
    }
}
```

**When to add:**
- New object the player can interact with
- Needs to exist in the scene at runtime

**Key rules:**
- Add a new `InteractType` enum value in `IInteractable.cs`
- Factory `Create()` method builds the GameObject programmatically
- `Interact()` should guard against null `PlayerController.Instance`
- Publish events for side effects, don't call managers directly

---

## 4. IMGUI UI Panel

**File:** `Assets/Scripts/UI/XxxUI.cs`

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class XxxUI : MonoBehaviour
{
    private bool _visible;
    private Rect _windowRect = new Rect(0, 0, 300, 400);

    private void OnEnable()
    {
        GameEvents.XxxMenuRequested += OnXxxMenuRequested;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.XxxMenuRequested -= OnXxxMenuRequested;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    private void OnXxxMenuRequested(/* args */)
    {
        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void OnMenuCloseRequested()
    {
        Close();
    }

    private void Update()
    {
        if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void Close()
    {
        if (!_visible) return;
        _visible = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUI.Window(uniqueId, _windowRect, DrawWindow, "Window Title");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
    }
}
```

**When to add:**
- New menu or panel the player opens
- Any UI that isn't a HUD element

**Key rules:**
- Subscribe in `OnEnable`, unsubscribe in `OnDisable` — always paired
- Center window on screen when opening
- Set `PlayerController.Instance.IsMenuOpen` when opening/closing
- Close on Escape in `Update()`
- Use unique window IDs (increment from existing IDs in other panels)
- Guard against null managers in `DrawWindow`

---

## 5. Static Rules Class

**File:** `Assets/Scripts/Rules/XxxRules.cs`

```csharp
using System;

public static class XxxRules
{
    public const int SomeThreshold = 50;

    public static bool SomeCheck(int value, IRng rng)
    {
        return value > SomeThreshold && rng.Value01() < 0.5f;
    }

    public static int CalculateResult(int input)
    {
        return Math.Max(0, input - SomeThreshold);
    }
}
```

**When to add:**
- New domain logic that can be expressed as pure functions
- Validation rules, pricing calculations, probability checks
- Any logic that should be unit-testable without Unity

**Key rules:**
- **No `UnityEngine` types** except `Mathf` (acceptable for rounding)
- `sealed` is not needed for static classes (they're inherently sealed)
- Use `IRng` for any randomness — never `Random.Range` or `UnityEngine.Random`
- Constants as `public const` with descriptive names
- All methods are `public static`
- Use `System.Math` over `Mathf` when possible (no Unity dependency)

---

## 6. Immutable Data Definition

**File:** `Assets/Scripts/XxxDef.cs`

```csharp
public sealed class XxxDef
{
    public string id { get; }
    public string displayName { get; }
    public int someValue { get; }

    public XxxDef(string id, string displayName, int someValue)
    {
        this.id = id;
        this.displayName = displayName;
        this.someValue = someValue;
    }
}
```

**When to add:**
- New type of content (items, residents, buildings, recipes)
- Immutable data that ContentDb will hold

**Key rules:**
- `sealed` class with `get`-only properties
- Lowercase `id` property (matches existing convention: `item.id`)
- PascalCase `displayName` property
- Constructor assigns all fields
- No methods — pure data
- Register in `ContentDb`:
  1. Add `public static readonly XxxDef Name = new XxxDef(...)` field
  2. Add `Register(Name)` call in `ContentDb.Awake()`
- If adding a new content type (not just a new instance), also add a `Dictionary<string, XxxDef>` and `Register(XxxDef)` method to `ContentDb`
