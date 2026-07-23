# Homestead Build-from-Scratch Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the shipped Homestead purchase-at-town-edge with a build-from-scratch system on the player's own camp clearing, where the player forages Stone and Wood from new forest interactables and constructs the Homestead in 3 stages.

**Architecture:** A new `BuildSign` component (IInteractable) replaces the Homestead `Building` in the scene at the camp clearing. It tracks build stages with its own `BuildStage` enum. When all 3 stages are complete, it activates the Homestead `Building` at `Restored` state (which already exists in the scene but is disabled). New `StonePile` and `FallenLog` forage interactables follow the BerryBush pattern. Tormod's first delivery grants 3 Nails via the existing `GameEvents.DeliveryMade` event. BerryBush sprite rendering is fixed (4×4 px at 16 PPU = 0.25 world units, too small to see — needs larger rect). Homestead position is moved to valid ground near camp.

**Tech Stack:** Unity 6 (6000.2.14f1), C#, NUnit (EditMode + PlayMode via Unity Test Framework).

## Current-code findings that shaped task order

- `Assets/Scripts/BerryBush.cs:62-66` — `Create()` uses `Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), ..., 16f)`. The 4×4 pixel rect at 16 PPU = 0.25 world units. That's invisible at gameplay zoom. Needs at least 16×16 pixels (= 1 world unit) to be visible.
- `Assets/Scripts/ContentDb.cs` — Stone and Wood ItemDefs don't exist yet. Nails already exists at line 20.
- `Assets/Scripts/IInteractable.cs:8-21` — `InteractType` enum has `Forage` which StonePile/FallenLog will use.
- `Assets/Scripts/DeliveryPoint.cs:42` — fires `GameEvents.OnDeliveryMade(deliveryType, item, count, price)`. The Tormod nails grant can subscribe to this event (filtering `DeliveryType.Tormod`), keeping the system event-driven with no direct cross-manager calls.
- `Assets/Scripts/Building.cs:53` — `BuildingState` enum is `Abandoned, Purchased, Cleared, Restored`. The Homestead Building will start disabled in the scene; BuildSign enables it and sets `Restored` when build completes.
- `Assets/Scripts/Building.cs:77-89` — Building self-registers with `BuildingManager` in `OnEnable`. Since the Homestead Building starts disabled, it won't register until BuildSign enables it — no double-registration risk.
- `Assets/Scripts/FermentManager.cs:33` — Basic Mash is gated on `"Homestead"` via `unlockedByBuildingId`. The recipe discovery system (already shipped) auto-discovers recipes when their gating building becomes `Restored`. Once BuildSign enables the Homestead Building at `Restored`, Basic Mash auto-discovers. This works seamlessly.
- `Assets/Scripts/UI/GameHUD.cs:180-182` — currently has Homestead-specific "Build" vs "Buy" wording for `BuildingState.Abandoned`. Since BuildSign replaces the Building entirely (the Building is disabled until built), the HUD branch for Building.Abandoned+Homestead is now dead code. The BuildSign branch in HUD is new.

## Global Constraints

- Follow `AGENTS.md` exactly: no comments in code, event bus via `GameEvents`, no direct cross-manager calls for cross-system notification, `Rules/` stays pure C#, IMGUI for UI.
- Commit messages: plain descriptive text. NEVER add Co-Authored-By or any Claude/AI attribution.
- Do NOT commit this plan file or anything under `docs/superpowers/plans/`.
- `Assets/Docs/BuildPlan.md` IS committed — it's a project doc.
- Unity must be CLOSED before running batchmode test commands.
- Test commands (PowerShell, from repo root):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`. For PlayMode replace both `editmode` strings with `playmode` and `-testPlatform EditMode` with `-testPlatform PlayMode`.

---

### Task 1: Add Stone and Wood item defs to ContentDb

**Files:**
- Modify: `Assets/Scripts/ContentDb.cs`

**Interfaces:**
- Consumes: existing `ItemDef` constructor, existing `Register()` pattern.
- Produces: `ContentDb.Stone` (forage item, stackable, 1g), `ContentDb.Wood` (forage item, stackable, 2g). Task 2 and Task 4 consume these.

- [ ] **Step 1: Edit `Assets/Scripts/ContentDb.cs`**

Add two new static readonly fields after `BerryShine` (line 22):

```csharp
    public static readonly ItemDef BerryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    public static readonly ItemDef Stone = new ItemDef("stone", "Stone", true, 1);
    public static readonly ItemDef Wood = new ItemDef("wood", "Wood", true, 2);
```

Add two `Register()` calls in `Awake()` after `Register(BerryShine)` (line 63):

```csharp
        Register(BerryShine);
        Register(Stone);
        Register(Wood);
```

- [ ] **Step 2: Commit**

```powershell
git add Assets/Scripts/ContentDb.cs
git commit -m "Add Stone and Wood forage item definitions to ContentDb"
```

---

### Task 2: Create StonePile forage interactable

**Files:**
- Create: `Assets/Scripts/StonePile.cs`
- Create: `Assets/Tests/EditMode/StonePileTests.cs`

**Interfaces:**
- Consumes: `ContentDb.Stone` (from Task 1), `InventoryManager.Instance.TryAdd`, `GameEvents.DayEnded` / `OnDayEnded` (respawn), `IInteractable`, `InteractType.Forage`.
- Produces: `StonePile` component (IInteractable, yields 1 Stone per forage, respawns daily, follows BerryBush pattern exactly), `StonePile.Create(Vector3)` factory.

- [ ] **Step 1: Write `Assets/Scripts/StonePile.cs`**

Follow the BerryBush pattern exactly (`Assets/Scripts/BerryBush.cs`), but:
- Class name: `StonePile`
- `Interact()` calls `InventoryManager.Instance.TryAdd(ContentDb.Stone, 1)`
- `Create()` uses `sr.color = new Color(0.6f, 0.6f, 0.6f)` (gray)
- Sprite rect: `new Rect(0, 0, 16, 16)` at 16 PPU = 1 world unit (fixes the visibility bug from BerryBush)
- `sr.sortingOrder = 5`
- Collider: `col.size = new Vector2(0.8f, 0.6f)`
- GameObject name: `"StonePile"`

```csharp
using UnityEngine;

public class StonePile : MonoBehaviour, IInteractable
{
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool IsHarvested => _harvested;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
    }

    private void OnDayEnded(int day)
    {
        if (_harvested)
            SetHarvested(false);
    }

    public void Interact()
    {
        if (_harvested) return;
        if (InventoryManager.Instance == null) return;

        InventoryManager.Instance.TryAdd(ContentDb.Stone, 1);
        SetHarvested(true);
    }

    private void SetHarvested(bool harvested)
    {
        _harvested = harvested;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !harvested;
        if (_collider != null)
            _collider.enabled = !harvested;
    }

    public static StonePile Create(Vector3 position)
    {
        var go = new GameObject("StonePile");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.6f, 0.6f, 0.6f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.6f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var pile = go.AddComponent<StonePile>();
        pile._spriteRenderer = sr;
        pile._collider = col;

        return pile;
    }
}
```

- [ ] **Step 2: Write `Assets/Tests/EditMode/StonePileTests.cs`**

Follow `BerryBushTests.cs` pattern exactly, but test for `ContentDb.Stone`:

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class StonePileTests
{
    private InventoryManager _inventory;
    private StonePile _pile;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestPile");
        _pile = go.AddComponent<StonePile>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsStoneToInventory()
    {
        _pile.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _pile.Interact();
        _pile.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _pile.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Stone));

        GameEvents.OnDayEnded(1);

        _pile.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Stone));
    }
}
```

- [ ] **Step 3: Commit**

```powershell
git add Assets/Scripts/StonePile.cs Assets/Tests/EditMode/StonePileTests.cs
git commit -m "Add StonePile forage interactable with daily respawn"
```

---

### Task 3: Create FallenLog forage interactable

**Files:**
- Create: `Assets/Scripts/FallenLog.cs`
- Create: `Assets/Tests/EditMode/FallenLogTests.cs`

**Interfaces:**
- Consumes: `ContentDb.Wood` (from Task 1), same interfaces as StonePile.
- Produces: `FallenLog` component (IInteractable, yields 1 Wood per forage, respawns daily), `FallenLog.Create(Vector3)` factory.

- [ ] **Step 1: Write `Assets/Scripts/FallenLog.cs`**

Same pattern as StonePile but:
- Class name: `FallenLog`
- `Interact()` calls `InventoryManager.Instance.TryAdd(ContentDb.Wood, 1)`
- `Create()` uses `sr.color = new Color(0.55f, 0.35f, 0.15f)` (brown)
- Collider: `col.size = new Vector2(1.0f, 0.5f)` (wider, flatter — it's a log)
- GameObject name: `"FallenLog"`

```csharp
using UnityEngine;

public class FallenLog : MonoBehaviour, IInteractable
{
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;
    public bool IsHarvested => _harvested;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
    }

    private void OnDayEnded(int day)
    {
        if (_harvested)
            SetHarvested(false);
    }

    public void Interact()
    {
        if (_harvested) return;
        if (InventoryManager.Instance == null) return;

        InventoryManager.Instance.TryAdd(ContentDb.Wood, 1);
        SetHarvested(true);
    }

    private void SetHarvested(bool harvested)
    {
        _harvested = harvested;
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !harvested;
        if (_collider != null)
            _collider.enabled = !harvested;
    }

    public static FallenLog Create(Vector3 position)
    {
        var go = new GameObject("FallenLog");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.55f, 0.35f, 0.15f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.0f, 0.5f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var log = go.AddComponent<FallenLog>();
        log._spriteRenderer = sr;
        log._collider = col;

        return log;
    }
}
```

- [ ] **Step 2: Write `Assets/Tests/EditMode/FallenLogTests.cs`**

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class FallenLogTests
{
    private InventoryManager _inventory;
    private FallenLog _log;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestLog");
        _log = go.AddComponent<FallenLog>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsWoodToInventory()
    {
        _log.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _log.Interact();
        _log.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _log.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Wood));

        GameEvents.OnDayEnded(1);

        _log.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Wood));
    }
}
```

- [ ] **Step 3: Commit**

```powershell
git add Assets/Scripts/FallenLog.cs Assets/Tests/EditMode/FallenLogTests.cs
git commit -m "Add FallenLog forage interactable with daily respawn"
```

---

### Task 4: Create BuildSign component and build-stage system

**Files:**
- Create: `Assets/Scripts/BuildSign.cs`
- Create: `Assets/Tests/EditMode/BuildSignTests.cs`

**Interfaces:**
- Consumes: `ContentDb.Stone`, `ContentDb.Wood`, `ContentDb.Nails`, `InventoryManager.Instance.Has/TryRemove`, `GameEvents.OnToastRequested`, `GameEvents.OnBuildingStateChanged`, `Building` component (disabled in scene, enabled at `Restored` when build completes), `BuildingState.Restored`.
- Produces: `BuildSign` component (IInteractable) with `BuildStage` enum, `Interact()` that advances stages if player has materials, `BuildStageChanged` event on `GameEvents`. Task 5 consumes `GameEvents.HomesteadBuildStageChanged`. Task 6 consumes the HUD prompt.

**Build stages:**

| Stage | Name | Materials | Prompt |
|-------|------|-----------|--------|
| 0 | Site | None | `[E] Homestead Site (need 3 Stone)` |
| 1 | Foundation | 3 Stone | `[E] Build Foundation (3 Stone)` |
| 2 | Frame | 3 Wood | `[E] Build Frame (3 Wood)` |
| 3 | Walls | 2 Wood + 3 Nails | `[E] Build Walls (2 Wood, 3 Nails)` |

After Walls: enable the Homestead Building at `Restored`, disable the BuildSign.

- [ ] **Step 1: Add `HomesteadBuildStageChanged` event to `Assets/Scripts/GameEvents.cs`**

Add after `RecipeDiscovered` (line 15):

```csharp
    public static event System.Action<string> RecipeDiscovered;
    public static event System.Action<int> HomesteadBuildStageChanged;
```

Add invoker after `OnRecipeDiscovered` (line 88):

```csharp
    public static void OnRecipeDiscovered(string recipeId)
        => RecipeDiscovered?.Invoke(recipeId);

    public static void OnHomesteadBuildStageChanged(int newStage)
        => HomesteadBuildStageChanged?.Invoke(newStage);
```

- [ ] **Step 2: Write `Assets/Scripts/BuildSign.cs`**

```csharp
using UnityEngine;

public enum BuildStage
{
    Site = 0,
    Foundation = 1,
    Frame = 2,
    Walls = 3
}

public class BuildSign : MonoBehaviour, IInteractable
{
    [SerializeField] private Building homesteadBuilding;

    private SpriteRenderer _spriteRenderer;
    private BuildStage _stage;
    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };

    public BuildStage Stage => _stage;
    public InteractType InteractType => InteractType.Building;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
    }

    public void Interact()
    {
        if (InventoryManager.Instance == null) return;

        switch (_stage)
        {
            case BuildStage.Site:
                if (!InventoryManager.Instance.Has(ContentDb.Stone, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Stone to build the foundation");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Stone, 3);
                AdvanceStage(BuildStage.Foundation, "Foundation built!");
                break;

            case BuildStage.Foundation:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Wood to build the frame");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 3);
                AdvanceStage(BuildStage.Frame, "Frame built!");
                break;

            case BuildStage.Frame:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 2) ||
                    !InventoryManager.Instance.Has(ContentDb.Nails, 3))
                {
                    GameEvents.OnToastRequested("Need 2 Wood and 3 Nails to build the walls");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 2);
                InventoryManager.Instance.TryRemove(ContentDb.Nails, 3);
                AdvanceStage(BuildStage.Walls, "Homestead built!");
                CompleteBuild();
                break;

            case BuildStage.Walls:
                break;
        }
    }

    private void AdvanceStage(BuildStage newStage, string toast)
    {
        _stage = newStage;
        if (_spriteRenderer != null && (int)_stage < _stageColors.Length)
            _spriteRenderer.color = _stageColors[(int)_stage];
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void CompleteBuild()
    {
        if (homesteadBuilding != null)
        {
            homesteadBuilding.gameObject.SetActive(true);
            homesteadBuilding.SetState(BuildingState.Restored);
        }
        gameObject.SetActive(false);
    }

    public static BuildSign Create(Vector3 position, Building homestead = null)
    {
        var go = new GameObject("BuildSign");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.7f, 0.6f, 0.4f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.0f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var sign = go.AddComponent<BuildSign>();
        sign._spriteRenderer = sr;
        sign.homesteadBuilding = homestead;

        return sign;
    }
}
```

- [ ] **Step 3: Write `Assets/Tests/EditMode/BuildSignTests.cs`**

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class BuildSignTests
{
    private InventoryManager _inventory;
    private BuildSign _sign;
    private GameObject _homesteadGo;
    private Building _homestead;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var signGo = TestBootstrap.CreateGameObject("TestSign");
        _sign = signGo.AddComponent<BuildSign>();

        _homesteadGo = TestBootstrap.CreateGameObject("TestHomestead");
        _homestead = _homesteadGo.AddComponent<Building>();
        _homesteadGo.SetActive(false);
        _sign.homesteadBuilding = _homestead;
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_NoStone_StaysAtSite()
    {
        _sign.Interact();

        Assert.AreEqual(BuildStage.Site, _sign.Stage);
    }

    [Test]
    public void Interact_WithStone_AdvancesToFoundation()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Foundation, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_WithWood_AdvancesToFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_WithWoodAndNails_AdvancesToWalls()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Walls, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));
    }

    [Test]
    public void Interact_CompleteBuild_EnablesHomesteadAtRestored()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.IsTrue(_homesteadGo.activeSelf);
        Assert.AreEqual(BuildingState.Restored, _homestead.State);
        Assert.IsFalse(_sign.gameObject.activeSelf);
    }

    [Test]
    public void Interact_FrameWithoutNails_StaysAtFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
    }

    [Test]
    public void HomesteadBuildStageChanged_FiresOnAdvance()
    {
        int firedStage = -1;
        GameEvents.HomesteadBuildStageChanged += s => firedStage = s;

        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(1, firedStage);
    }
}
```

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/GameEvents.cs Assets/Scripts/BuildSign.cs Assets/Tests/EditMode/BuildSignTests.cs
git commit -m "Add BuildSign component with 3-stage Homestead construction system"
```

---

### Task 5: Tormod first-delivery Nails grant

**Files:**
- Modify: `Assets/Scripts/SellManager.cs`
- Test: `Assets/Tests/PlayMode/TormodNailsGrantTests.cs` (new)

**Interfaces:**
- Consumes: `GameEvents.DeliveryMade` (fired by `DeliveryPoint`), `DeliveryType.Tormod`, `InventoryManager.Instance.TryAdd(ContentDb.Nails, 3)`, `ContentDb.Nails`.
- Produces: on the first Tormod delivery, grants 3 Nails to the player inventory + toast. Tracks grant state with a simple `bool` so it only fires once.

- [ ] **Step 1: Edit `Assets/Scripts/SellManager.cs`**

Add a field after `_rng` (line 19):

```csharp
    private IRng _rng = UnityRng.Instance;
    private bool _tormodNailsGranted;
```

Add event subscription in `OnEnable` after `GameEvents.HourChanged += OnHourChanged;` (line 36):

```csharp
        GameEvents.HourChanged += OnHourChanged;
        GameEvents.DeliveryMade += OnDeliveryMade;
```

Remove in `OnDisable` after `GameEvents.HourChanged -= OnHourChanged;` (line 41):

```csharp
        GameEvents.HourChanged -= OnHourChanged;
        GameEvents.DeliveryMade -= OnDeliveryMade;
```

Add the handler method before `OpenSellMenu` (line 132):

```csharp
    private void OnDeliveryMade(DeliveryType type, ItemDef item, int count, int price)
    {
        if (type != DeliveryType.Tormod || _tormodNailsGranted) return;
        _tormodNailsGranted = true;
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.TryAdd(ContentDb.Nails, 3);
            GameEvents.OnToastRequested("+3 Nails from Tormod");
        }
    }
```

- [ ] **Step 2: Write `Assets/Tests/PlayMode/TormodNailsGrantTests.cs`**

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class TormodNailsGrantTests
{
    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator FirstTormodDelivery_GrantsThreeNails()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }

    [UnityTest]
    public IEnumerator SecondTormodDelivery_DoesNotGrantNailsAgain()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);
        GameEvents.OnDeliveryMade(DeliveryType.Tormod, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Nails));
    }

    [UnityTest]
    public IEnumerator CartDelivery_DoesNotGrantNails()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<SellManager>();

        yield return null;

        GameEvents.OnDeliveryMade(DeliveryType.Cart, ContentDb.BerryShine, 1, 15);

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Nails));
    }
}
```

- [ ] **Step 3: Commit**

```powershell
git add Assets/Scripts/SellManager.cs Assets/Tests/PlayMode/TormodNailsGrantTests.cs
git commit -m "Grant 3 Nails from Tormod on first delivery"
```

---

### Task 6: BuildSign HUD prompt and Homestead Building interaction cleanup

**Files:**
- Modify: `Assets/Scripts/UI/GameHUD.cs`

**Interfaces:**
- Consumes: `BuildSign` component (from Task 4), `BuildStage` enum.
- Produces: HUD shows material-requirement prompts for BuildSign, removes dead Homestead-specific Building.Abandoned branch.

- [ ] **Step 1: Edit `Assets/Scripts/UI/GameHUD.cs`**

In `UpdateInteractPrompt`, add a `BuildSign` branch right after the `Building` block (after the closing `}` of `if (interactable is Building building)`, around line 195), before `else if (interactable is FermentVat vat)`:

```csharp
        else if (interactable is BuildSign sign)
        {
            promptText.text = sign.Stage switch
            {
                BuildStage.Site => $"[E] Homestead Site (need 3 Stone)",
                BuildStage.Foundation => $"[E] Build Frame (need 3 Wood)",
                BuildStage.Frame => $"[E] Build Walls (need 2 Wood, 3 Nails)",
                BuildStage.Walls => $"[E] Homestead",
                _ => $"[E] Homestead Site"
            };
        }
```

Also remove the now-dead Homestead-specific "Build" branch in the Building block. Change:

```csharp
                    BuildingState.Abandoned => building.BuildingName == "Homestead"
                        ? $"[E] Build {building.BuildingName} ({building.PurchaseCost}g)"
                        : $"[E] Buy {building.BuildingName} ({building.PurchaseCost}g)",
```

back to:

```csharp
                    BuildingState.Abandoned => $"[E] Buy {building.BuildingName} ({building.PurchaseCost}g)",
```

The Homestead Building is never in `Abandoned` state anymore (it starts disabled and appears at `Restored`), so this branch is unreachable for the Homestead.

- [ ] **Step 2: Commit**

```powershell
git add Assets/Scripts/UI/GameHUD.cs
git commit -m "Add BuildSign HUD prompts and remove dead Homestead Build branch"
```

---

### Task 7: Fix BerryBush sprite visibility and scene rework

**Files:**
- Modify: `Assets/Scripts/BerryBush.cs`
- Modify: `Assets/Scripts/BuildingManager.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity MCP RunCommand)

**Interfaces:**
- Consumes: existing BerryBush.Create, existing Building system.
- Produces: BerryBush sprites visible at gameplay zoom (16×16 pixels = 1 world unit instead of 4×4 = 0.25). Homestead Building starts disabled in the scene. BuildSign at camp clearing position. StonePile and FallenLog instances scattered near camp. BerryBush instances repositioned to valid ground.

- [ ] **Step 1: Fix `Assets/Scripts/BerryBush.cs` Create method**

In `Create()`, change the sprite rect from `4,4` to `16,16`:

```csharp
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
```

- [ ] **Step 2: Remove Homestead-specific "Build" text from `Assets/Scripts/BuildingManager.cs`**

Revert the `TryPurchase` method back to its original form (the Homestead no longer goes through TryPurchase). Change:

```csharp
    public bool TryPurchase(Building building)
    {
        if (!RenovationRules.CanPurchase(building.State)) return false;

        bool isHomestead = building.BuildingName == "Homestead";

        if (!GameManager.Instance.TrySpend(building.PurchaseCost))
        {
            GameEvents.OnToastRequested(isHomestead
                ? $"Can't afford to build {building.BuildingName} ({building.PurchaseCost}g)"
                : $"Can't afford {building.BuildingName} ({building.PurchaseCost}g)");
            return false;
        }

        var old = building.State;
        building.SetState(BuildingState.Purchased);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested(isHomestead
            ? $"Started building {building.BuildingName} (-{building.PurchaseCost}g)"
            : $"Purchased {building.BuildingName} (-{building.PurchaseCost}g)");
        return true;
    }
```

back to:

```csharp
    public bool TryPurchase(Building building)
    {
        if (!RenovationRules.CanPurchase(building.State)) return false;

        if (!GameManager.Instance.TrySpend(building.PurchaseCost))
        {
            GameEvents.OnToastRequested($"Can't afford {building.BuildingName} ({building.PurchaseCost}g)");
            return false;
        }

        var old = building.State;
        building.SetState(BuildingState.Purchased);
        GameEvents.OnBuildingStateChanged(building, old, building.State);
        GameEvents.OnToastRequested($"Purchased {building.BuildingName} (-{building.PurchaseCost}g)");
        return true;
    }
```

- [ ] **Step 3: Scene rework via Unity MCP RunCommand**

Delete all existing BerryBush instances, the Homestead Building, and re-add:
- 9 BerryBush instances at valid ground positions (near camp, along road, town outskirts, hidden corners)
- 3 StonePile instances in the forest near camp
- 3 FallenLog instances in the forest near camp
- 1 BuildSign at position `(-14, 2, 0)` (camp clearing, on solid ground near Tent/CampfirePot)
- Homestead Building at position `(-14, 2, 0)` but **disabled** (BuildSign holds reference)

RunCommand script:

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // Delete existing berry bushes
        var existingBushes = Object.FindObjectsByType<BerryBush>(FindObjectsSortMode.None);
        foreach (var bush in existingBushes)
        {
            result.DestroyObject(bush.gameObject);
        }
        result.Log("Deleted " + existingBushes.Length + " old BerryBush instances");

        // Find and disable the Homestead Building
        var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
        Building homestead = null;
        foreach (var b in buildings)
        {
            if (b.BuildingName == "Homestead")
            {
                homestead = b;
                result.RegisterObjectModification(b.gameObject);
                b.gameObject.SetActive(false);
                result.Log("Disabled Homestead Building at " + b.transform.position);
            }
        }

        // Move Homestead to camp clearing position
        if (homestead != null)
        {
            homestead.transform.position = new Vector3(-14f, 2f, 0f);
            result.Log("Moved Homestead to (-14, 2, 0)");
        }

        // Create BuildSign at camp clearing
        var buildSign = BuildSign.Create(new Vector3(-14f, 2f, 0f), homestead);
        result.RegisterObjectCreation(buildSign.gameObject);
        result.Log("Created BuildSign at (-14, 2, 0)");

        // Create berry bushes (9 across the map)
        Vector3[] bushPositions = {
            new Vector3(-16f, 4f, 0f),
            new Vector3(-20f, 5f, 0f),
            new Vector3(-10f, 3f, 0f),
            new Vector3(-6f, 2f, 0f),
            new Vector3(-3f, 1f, 0f),
            new Vector3(5f, -3f, 0f),
            new Vector3(-5f, -4f, 0f),
            new Vector3(12f, 2f, 0f),
            new Vector3(-24f, -2f, 0f),
        };
        foreach (var pos in bushPositions)
        {
            var bush = BerryBush.Create(pos);
            result.RegisterObjectCreation(bush.gameObject);
        }
        result.Log("Created " + bushPositions.Length + " BerryBush instances");

        // Create stone piles (3 near camp)
        Vector3[] stonePositions = {
            new Vector3(-22f, 5f, 0f),
            new Vector3(-18f, 6f, 0f),
            new Vector3(-25f, 1f, 0f),
        };
        foreach (var pos in stonePositions)
        {
            var pile = StonePile.Create(pos);
            result.RegisterObjectCreation(pile.gameObject);
        }
        result.Log("Created " + stonePositions.Length + " StonePile instances");

        // Create fallen logs (3 in forest)
        Vector3[] logPositions = {
            new Vector3(-20f, 6f, 0f),
            new Vector3(-24f, 3f, 0f),
            new Vector3(-16f, 6f, 0f),
        };
        foreach (var pos in logPositions)
        {
            var log = FallenLog.Create(pos);
            result.RegisterObjectCreation(log.gameObject);
        }
        result.Log("Created " + logPositions.Length + " FallenLog instances");

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        result.Log("Scene saved");
    }
}
```

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/BerryBush.cs Assets/Scripts/BuildingManager.cs Assets/Scenes/SampleScene.unity
git commit -m "Fix berry bush visibility, add forage objects and BuildSign to scene, disable Homestead until built"
```

---

### Task 8: Update BuildPlan.md Phase 1 for build-from-scratch

**Files:**
- Modify: `Assets/Docs/BuildPlan.md`

**Interfaces:**
- Consumes: Tasks 1-7.
- Produces: updated Phase 1 reflecting the new Homestead build system.

- [ ] **Step 1: Edit `Assets/Docs/BuildPlan.md`**

Change the Phase 1 section from:

```markdown
## Phase 1 — Act 0: the tent prologue (in progress)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), 8–10 scattered across camp, the road to town, town outskirts, and hidden corners — forage verb = existing interact.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, 3h ferment, always discovered).
- [x] Day 1 starting inventory: 3 Berry so the player can start fermenting immediately instead of waiting idle.
- [x] Recipe discovery scaffolding: `RecipeDiscovered` event on GameEvents, hidden/discovered recipe tracking in FermentManager; Berry Shine is exempt and always visible.
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Homestead build/restore: on the player's own camp clearing (not a separate town-edge lot) — smash/clear/repair doubles as cleaning up the player's own camp; price reachable in ~3 sales; unlocks proper still + vat + game proper.
- [x] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.
```

to:

```markdown
## Phase 1 — Act 0: the tent prologue (in progress)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), 8–10 scattered across camp, the road to town, town outskirts, and hidden corners — forage verb = existing interact.
- [x] Foraging: stone piles and fallen logs (respawn daily) yield Stone and Wood for Homestead construction.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, 3h ferment, always discovered).
- [x] Day 1 starting inventory: 3 Berry so the player can start fermenting immediately instead of waiting idle.
- [x] Recipe discovery scaffolding: `RecipeDiscovered` event on GameEvents, hidden/discovered recipe tracking in FermentManager; Berry Shine is exempt and always visible.
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Homestead build-from-scratch: 3 stages (Foundation 3 Stone → Frame 3 Wood → Walls 2 Wood + 3 Nails from Tormod) on the player's own camp clearing; player forages materials between ferment batches; unlocks proper still + vat + game proper.
- [x] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.
```

- [ ] **Step 2: Commit**

```powershell
git add Assets/Docs/BuildPlan.md
git commit -m "Update Phase 1 build plan for Homestead build-from-scratch system"
```

---

## Self-Review Notes

- **Spec coverage:** Every item in the design spec has a task — Stone/Wood items (Task 1), StonePile (Task 2), FallenLog (Task 3), BuildSign with 3 stages (Task 4), Tormod Nails grant (Task 5), HUD prompts (Task 6), scene rework + berry bush fix + BuildingManager cleanup (Task 7), BuildPlan update (Task 8).
- **No placeholders:** All code blocks contain complete implementations. No TBD/TODO.
- **Type consistency:** `BuildStage` enum defined in BuildSign.cs, referenced in BuildSignTests.cs and GameHUD.cs. `ContentDb.Stone`/`ContentDb.Wood` defined in Task 1, consumed by Tasks 2-4. `GameEvents.HomesteadBuildStageChanged` added in Task 4 Step 1, consumed by Task 4 Step 3 test.
- **Bug fixes covered:** BerryBush sprite rect fix (Task 7 Step 1), Homestead position (Task 7 Step 3 moves it to (-14, 2) which is solid ground near camp), dead code cleanup (Task 6 removes Homestead Build branch from HUD, Task 7 Step 2 reverts BuildingManager TryPurchase).
- **Order rationale:** Items first (Task 1) → forage interactables (Tasks 2-3) → build system (Task 4) → Nails source (Task 5) → HUD (Task 6) → scene rework that wires it all together (Task 7) → doc update (Task 8). Each task is independently compilable and testable.
