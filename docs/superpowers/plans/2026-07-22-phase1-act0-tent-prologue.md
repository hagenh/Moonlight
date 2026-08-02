# Phase 1 — Act 0: The Tent Prologue Implementation Plan

> **Superseded (2026-07-23):** Task 6 Step 5 (Homestead placed "at the town edge") and Task 6 Step 4 (3 berry bushes clustered at camp) below are historical record only — they describe what originally shipped, not the current design. See `docs/superpowers/plans/2026-07-23-pacing-and-homestead-camp-rework.md` for the current plan: Homestead relocated onto the player's own camp clearing, and berry bushes scattered 8–10 across the map. Do not treat those two checked boxes as current truth.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Act 0 tent prologue — forage berries, wild-ferment berry shine, sell to Tormod at dusk, purchase and restore the homestead.

**Architecture:** Reuse existing systems (FermentVat/FermentManager for the campfire pot, SellerInteractable/SellManager for Tormod, Building/BuildingManager for the homestead). Add one new interactable (BerryBush) and two new items (Berry, BerryShine). Minimal map extension for the camp clearing.

**Tech Stack:** Unity 6 (6000.2.14f1), C#, NUnit (EditMode + PlayMode via Unity Test Framework).

## Global Constraints

- Follow `AGENTS.md` exactly: no comments in code, event bus via `GameEvents`, no direct cross-manager calls, Rules/ stays pure C# (only `Mathf` allowed), IMGUI for UI.
- Commit messages: plain descriptive text. NEVER add Co-Authored-By or any Claude/AI attribution.
- Do NOT commit this plan file or anything under `docs/superpowers/plans/`.
- Unity must be CLOSED before running batchmode test commands.
- Test commands (PowerShell, from repo root):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected after a green run: `Select-String` prints `False`. For PlayMode replace both `editmode` strings with `playmode` and `-testPlatform EditMode` with `-testPlatform PlayMode`.

---

### Task 1: Add Berry and BerryShine items to ContentDb

**Files:**
- Modify: `Assets/Scripts/ContentDb.cs`

**Interfaces:**
- Produces: `ContentDb.Berry` (ItemDef, isIngredient=true, basePrice=2), `ContentDb.BerryShine` (ItemDef, isIngredient=false, basePrice=15, isBottle=true). Task 2 and Task 4 reference these.

- [ ] **Step 1: Add the two static readonly fields after the existing field declarations (e.g., after `Nails`)**

```csharp
    public static readonly ItemDef Berry = new ItemDef("berry", "Berry", true, 2);
    public static readonly ItemDef BerryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
```

- [ ] **Step 2: Add Register calls in `Awake()` after the existing Register calls**

```csharp
        Register(Berry);
        Register(BerryShine);
```

- [ ] **Step 3: Verify compilation via EditMode test run**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/ContentDb.cs
git commit -m "Add Berry and BerryShine items to ContentDb"
```

---

### Task 2: Add Berry Shine recipe and gate Basic Mash behind Homestead

**Files:**
- Modify: `Assets/Scripts/FermentManager.cs`
- Test: `Assets/Tests/PlayMode/FermentationFlowTests.cs`

**Interfaces:**
- Consumes: `ContentDb.Berry`, `ContentDb.BerryShine` from Task 1.
- Produces: Berry Shine recipe in `FermentManager._recipes` (always unlocked, 6h ferment, 2 output, ingredient: 3 Berry). Basic Mash recipe now gated by `unlockedByBuildingId = "Homestead"`. Task 4 and Task 5 depend on Berry Shine being fermentable.

- [ ] **Step 1: Write a failing EditMode test for the recipe data**

Create `Assets/Tests/EditMode/BerryShineRecipeTests.cs`:

```csharp
using NUnit.Framework;

public class BerryShineRecipeTests
{
    private ItemDef _berry;
    private ItemDef _berryShine;

    [SetUp]
    public void SetUp()
    {
        _berry = new ItemDef("berry", "Berry", true, 2);
        _berryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    }

    [Test]
    public void BerryShineRecipe_Requires3Berry()
    {
        var recipe = new RecipeData("Berry Shine", 6, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(3, recipe.Costs[_berry]);
        Assert.AreEqual(1, recipe.Costs.Count);
    }

    [Test]
    public void BerryShineRecipe_Yields2()
    {
        var recipe = new RecipeData("Berry Shine", 6, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(2, recipe.outputCount);
    }

    [Test]
    public void BerryShineRecipe_6HourFerment()
    {
        var recipe = new RecipeData("Berry Shine", 6, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(6, recipe.fermentationHours);
    }

    [Test]
    public void BerryShineRecipe_NoBuildingGate()
    {
        var recipe = new RecipeData("Berry Shine", 6, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.IsNull(recipe.unlockedByBuildingId);
    }
}
```

- [ ] **Step 2: Run EditMode tests to verify new tests pass (recipe is pure data, no compile dependency)**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 3: Edit `Assets/Scripts/FermentManager.cs` — add Berry Shine recipe and gate Basic Mash**

In the `_recipes` array in `Awake()`, add the Berry Shine recipe as the first element (index 0) so it's always available:

```csharp
        _recipes = new RecipeData[]
        {
            new RecipeData("Berry Shine", 6, 2, ContentDb.BerryShine)
                .AddIngredient(ContentDb.Berry, 3),
            new RecipeData("Basic Mash", 4, 3, ContentDb.BasicMoonshine, "Homestead")
                .AddIngredient(ContentDb.Grain, 2)
                .AddIngredient(ContentDb.Water, 1)
                .AddIngredient(ContentDb.Yeast, 1),
```

(Rest of the array unchanged — Sweet Batch, Highland Mash, Aged Reserve stay as-is.)

- [ ] **Step 4: Add a PlayMode test for berry shine fermentation**

Add to `Assets/Tests/PlayMode/FermentationFlowTests.cs` — a new test at the end of the class:

```csharp
    [UnityTest]
    public IEnumerator BerryShineFermentation_CompletesAndCollects()
    {
        _inventory.TryAdd(ContentDb.Berry, 3);

        var berryRecipe = new RecipeData("Berry Shine", 1, 2, ContentDb.BerryShine)
            .AddIngredient(ContentDb.Berry, 3);

        _fermentManager.TryStartBatch(_vat, berryRecipe);
        Assert.AreEqual(VatState.Fermenting, _vat.State);

        _timeManager.SetTime(
            _timeManager.Day + 1,
            _timeManager.Hour,
            _timeManager.Minute);

        for (int i = 0; i < 5; i++)
            yield return null;

        Assert.AreEqual(VatState.Ready, _vat.State);

        bool collected = _fermentManager.TryCollectBatch(_vat);
        Assert.IsTrue(collected);
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.BerryShine));
    }
```

- [ ] **Step 5: Run both test suites**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False` twice.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/FermentManager.cs Assets/Tests/EditMode/BerryShineRecipeTests.cs Assets/Tests/PlayMode/FermentationFlowTests.cs
git commit -m "Add Berry Shine recipe; gate Basic Mash behind Homestead"
```

---

### Task 3: Add Forage InteractType and BerryBush component

**Files:**
- Modify: `Assets/Scripts/IInteractable.cs`
- Create: `Assets/Scripts/BerryBush.cs`
- Modify: `Assets/Scripts/UI/GameHUD.cs`
- Test: `Assets/Tests/EditMode/BerryBushTests.cs`

**Interfaces:**
- Consumes: `ContentDb.Berry` from Task 1, `InteractType.Forage` from this task.
- Produces: `BerryBush` component — `Interact()` adds 1 Berry to InventoryManager, disables itself, respawns on `DayEnded`. `BerryBush.Create(Vector3)` factory method. Task 6 places bush instances in the scene.

- [ ] **Step 1: Add `Forage` to the `InteractType` enum**

In `Assets/Scripts/IInteractable.cs`, add `Forage` to the enum:

```csharp
public enum InteractType
{
    Building,
    FermentVat,
    Seller,
    Bed,
    Debris,
    DebrisPile,
    NPC,
    ExitDoor,
    Crate,
    DeliveryPoint,
    Forage
}
```

- [ ] **Step 2: Create `Assets/Scripts/BerryBush.cs`**

```csharp
using UnityEngine;

public class BerryBush : MonoBehaviour, IInteractable
{
    [SerializeField] private int berryYield = 1;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private bool _harvested;

    public InteractType InteractType => InteractType.Forage;

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

        InventoryManager.Instance.TryAdd(ContentDb.Berry, berryYield);
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

    public static BerryBush Create(Vector3 position)
    {
        var go = new GameObject("BerryBush");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.6f, 0.2f, 0.7f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var bush = go.AddComponent<BerryBush>();
        bush._spriteRenderer = sr;
        bush._collider = col;

        return bush;
    }
}
```

- [ ] **Step 3: Add the Forage prompt to GameHUD**

In `Assets/Scripts/UI/GameHUD.cs`, in `UpdateInteractPrompt`, add after the `DebrisPile` case and before the `Resident` case:

```csharp
        else if (interactable is BerryBush)
        {
            promptText.text = "[E] Forage";
        }
```

- [ ] **Step 4: Write EditMode tests for BerryBush**

Create `Assets/Tests/EditMode/BerryBushTests.cs`:

```csharp
using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class BerryBushTests
{
    private InventoryManager _inventory;
    private BerryBush _bush;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var go = TestBootstrap.CreateGameObject("TestBush");
        _bush = go.AddComponent<BerryBush>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_AddsBerryToInventory()
    {
        _bush.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));
    }

    [Test]
    public void Interact_Twice_OnlyAddsOnce()
    {
        _bush.Interact();
        _bush.Interact();

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));
    }

    [Test]
    public void Interact_DayEnded_Respawns()
    {
        _bush.Interact();
        Assert.AreEqual(1, _inventory.GetCount(ContentDb.Berry));

        GameEvents.OnDayEnded(1);

        _bush.Interact();
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.Berry));
    }
}
```

- [ ] **Step 5: Run EditMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/IInteractable.cs Assets/Scripts/BerryBush.cs Assets/Scripts/UI/GameHUD.cs Assets/Tests/EditMode/BerryBushTests.cs
git commit -m "Add BerryBush interactable with daily respawn and Forage prompt"
```

---

### Task 4: Add Tormod dusk-to-dawn schedule to SellManager

**Files:**
- Modify: `Assets/Scripts/SellManager.cs`
- Modify: `Assets/Scripts/UI/SellUI.cs`
- Test: `Assets/Tests/PlayMode/EconomyFlowTests.cs`

**Interfaces:**
- Consumes: `SellerType.Tormod` (already exists), `EconomyRules.IsSellable` (Tormod buys non-ingredients — BerryShine qualifies since `isBottle=true, isIngredient=false`).
- Produces: Tormod spawns at dusk (hour 18), despawns at dawn (hour 6), at a configurable position. `_tormodInstance` managed alongside `_cartInstance`. `IsTormodInTown` property. SellUI window title changes based on seller type.

- [ ] **Step 1: Edit `Assets/Scripts/SellManager.cs`**

Add these fields after the existing serialized fields:

```csharp
    [SerializeField] private Transform tormodPosition;
    [SerializeField] private int tormodArriveHour = 18;
    [SerializeField] private int tormodLeaveHour = 6;
```

Add this field after `_cartInstance`:

```csharp
    private SellerInteractable _tormodInstance;
```

Add this property after `IsCartInTown`:

```csharp
    public bool IsTormodInTown => _tormodInstance != null;
```

In `OnHourChanged`, add Tormod spawn/despawn logic alongside the cart logic:

Replace the existing `OnHourChanged` method:

```csharp
    private void OnHourChanged(int hour, int day)
    {
        if (hour == cartArriveHour && EconomyRules.IsCartDay(day) && _cartInstance == null)
            SpawnCart();

        if (hour == cartLeaveHour && _cartInstance != null)
            RemoveCart();

        if (hour == tormodArriveHour && _tormodInstance == null)
            SpawnTormod();

        if (hour == tormodLeaveHour && _tormodInstance != null)
            RemoveTormod();
    }
```

Add the two new methods before `OpenSellMenu`:

```csharp
    private void SpawnTormod()
    {
        Vector3 pos = tormodPosition != null ? tormodPosition.position : Vector3.zero;
        _tormodInstance = SellerInteractable.Create(SellerType.Tormod, pos);
        GameEvents.OnSellerArrived(SellerType.Tormod);
    }

    private void RemoveTormod()
    {
        if (_tormodInstance != null)
        {
            Destroy(_tormodInstance.gameObject);
            _tormodInstance = null;
            GameEvents.OnSellerLeft(SellerType.Tormod);
        }
    }
```

Also add `RemoveTormod();` call in `OnDayEnded` after `RemoveCart();`:

```csharp
    private void OnDayEnded(int day)
    {
        RemoveCart();
        RemoveTormod();
    }
```

- [ ] **Step 2: Edit `Assets/Scripts/UI/SellUI.cs` — vary window title by seller type**

Add a field to track which seller opened the menu:

```csharp
    private SellerType _currentSeller;
```

In `OnSellMenuRequested`, store the type:

```csharp
    private void OnSellMenuRequested(SellerType type)
    {
        _visible = true;
        _currentSeller = type;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }
```

In `OnGUI`, change the window title from the hardcoded string:

```csharp
        string title = _currentSeller == SellerType.Tormod
            ? "Tormod — Buy Ingredients"
            : "Traveling Cart — Buy Ingredients";
        _windowRect = GUI.Window(2, _windowRect, DrawWindow, title);
```

- [ ] **Step 3: Add a PlayMode test for Tormod spawn/despawn**

Add to `Assets/Tests/PlayMode/EconomyFlowTests.cs` — a new test at the end of the class (read the existing file first to know the exact class structure and field names, then append):

```csharp
    [UnityTest]
    public IEnumerator TormodSpawnsAtDusk()
    {
        var timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        var sellManager = TestBootstrap.CreateSingleton<SellManager>();

        timeManager.SetTime(1, 17, 0);

        Assert.IsFalse(sellManager.IsTormodInTown);

        timeManager.SetTime(1, 18, 0);

        for (int i = 0; i < 3; i++)
            yield return null;

        Assert.IsTrue(sellManager.IsTormodInTown);
    }
```

(Note: This test may fail if `HourChanged` doesn't fire from `SetTime` — check `TimeManager.SetTime`. It sets `_lastHour = Hour` but doesn't fire `OnHourChanged`. If needed, use `AdvanceHour` instead or fire the event manually. Adjust if the test fails.)

- [ ] **Step 4: Run PlayMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 5: Commit**

```powershell
git add Assets/Scripts/SellManager.cs Assets/Scripts/UI/SellUI.cs Assets/Tests/PlayMode/EconomyFlowTests.cs
git commit -m "Add Tormod dusk-to-dawn spawn schedule to SellManager"
```

---

### Task 5: Add BerryShine sellability test and EconomyRules verification

**Files:**
- Modify: `Assets/Tests/EditMode/EconomyRulesTests.cs`

**Interfaces:**
- Consumes: BerryShine item properties (`isBottle=true, isIngredient=false`) from Task 1.
- Produces: Confirmed that `EconomyRules.IsSellable(BerryShine, SellerType.Tormod)` returns true, verifying the core Act 0 economy works.

- [ ] **Step 1: Add BerryShine-specific sellability tests**

Add to `Assets/Tests/EditMode/EconomyRulesTests.cs`:

```csharp
    [Test]
    public void IsSellable_TormodBuysBerryShine()
    {
        var berryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
        Assert.IsTrue(EconomyRules.IsSellable(berryShine, SellerType.Tormod));
    }

    [Test]
    public void IsSellable_CartBuysBerryShine()
    {
        var berryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
        Assert.IsTrue(EconomyRules.IsSellable(berryShine, SellerType.TravelingCart));
    }

    [Test]
    public void IsSellable_TormodDoesNotBuyIngredients()
    {
        var berry = new ItemDef("berry", "Berry", true, 2);
        Assert.IsFalse(EconomyRules.IsSellable(berry, SellerType.Tormod));
    }
```

- [ ] **Step 2: Run EditMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 3: Commit**

```powershell
git add Assets/Tests/EditMode/EconomyRulesTests.cs
git commit -m "Add BerryShine sellability tests for Tormod and Cart"
```

---

### Task 6: Scene setup — camp clearing, tent, pot, bushes, Tormod position, homestead, player spawn

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity editor, not by hand)

**Interfaces:**
- Consumes: All systems from Tasks 1–4. `BerryBush` component, `FermentVat` for the pot, `Building` for the homestead, `SellManager` serialized fields for Tormod position.
- Produces: A playable Act 0 scene. All Phase 1 systems are visible and interactable.

- [ ] **Step 1: Open the project in Unity. Extend the tilemap west of the current town strip with a small camp clearing (~8×6 tiles). Use existing ground tiles for now (no new art).**

- [ ] **Step 2: Create the Tent object**

Add a GameObject named "Tent" in the camp clearing:
- SpriteRenderer (white texture, orange/brown tint, scaled up to ~1.5×1.5)
- BoxCollider2D (trigger, ~1.2×1.0) — purely cosmetic landmark, no IInteractable

- [ ] **Step 3: Create the Campfire Pot**

Add a GameObject named "CampfirePot" in the camp clearing:
- SpriteRenderer (white texture, gray tint, ~0.8×0.8)
- Add `FermentVat` component (leave vatRenderer null for now — the vat color refresh won't fire but the fermentation pipeline works)
- Set it on the Interactable layer
- Add a BoxCollider2D (trigger, ~0.6×0.6) — required for interaction detection

- [ ] **Step 4: Create Berry Bushes**

Add 3 GameObjects in the camp clearing with the `BerryBush` component. Either:
- Use `BerryBush.Create()` from code at runtime, or
- Manually add: SpriteRenderer (purple tint), BoxCollider2D (trigger, 0.6×0.8), `BerryBush` component, Interactable layer

- [ ] **Step 5: Create the Homestead Building**

Add a GameObject named "Homestead" at the town edge:
- `Building` component with: `buildingName = "Homestead"`, `purchaseCost = 80`, `dailyIncome = 0`
- Set `isFacadeOnly = false`, `smashHitsRequired = 3`, `debrisCount = 3`, `totalRepairPoints = 3`
- SpriteRenderer (abandoned color will be applied automatically)
- Board trigger collider + door trigger collider on Interactable layer
- Window Light2D(s) for the restored state visual

- [ ] **Step 6: Create the Tormod Position marker**

Add an empty GameObject named "TormodPosition" near the Roadhouse building (back door side). Wire it to the `SellManager.tormodPosition` field in the scene.

- [ ] **Step 7: Move the Player spawn**

Move the `PlayerController` GameObject's starting position to the camp clearing area.

- [ ] **Step 8: Verify no console errors on Play**

Enter Play mode once. The console must show no red errors. Walk to the berry bushes and forage. Walk to the campfire pot and start a Berry Shine batch. Walk to town and verify Tormod appears at dusk.

- [ ] **Step 9: Save the scene and commit**

```powershell
git add Assets/Scenes/SampleScene.unity
git commit -m "Add camp clearing, tent, pot, bushes, homestead, and Tormod position to scene"
```

---

### Task 7: End-to-end PlayMode verification and BuildPlan update

**Files:**
- Modify: `Assets/Docs/BuildPlan.md`

**Interfaces:**
- Consumes: Everything from Tasks 1–6.

- [ ] **Step 1: Manual play verification (10 minutes, in Play mode)**

1. HUD shows cash/day/clock/rep — no errors.
2. Forage a berry bush: +1 Berry toast, bush disappears.
3. Forage all 3 bushes: 3 Berry total.
4. Walk to campfire pot, interact: Berry Shine recipe available (6h, 2 output, 3 Berry). Start batch.
5. Sleep through the night. Berry bushes respawn. Batch progresses.
6. If batch is ready: collect as crate (2 Berry Shine).
7. Carry crate to town. At dusk (hour 18), Tormod appears near Roadhouse.
8. Interact with Tormod: sell Berry Shine for 15g each = 30g.
9. Repeat: forage → ferment → sell until you have 80g.
10. Walk to Homestead: "[E] Buy Homestead (80g)". Purchase it.
11. Smash boards, clear debris, repair. Homestead becomes Restored.
12. Basic Mash recipe now appears in the pot's recipe selection.

- [ ] **Step 2: Run both test suites**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False` twice.

- [ ] **Step 3: Tick Phase 1 checkboxes in `Assets/Docs/BuildPlan.md`**

Replace the Phase 1 section checkboxes:

```markdown
## Phase 1 — Act 0: the tent prologue (done)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), forage verb = existing interact.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, longer ferment).
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Homestead purchase: derelict building at town edge; price reachable in ~3 sales; unlocks proper still + vat + game proper.
- [x] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.
```

(The "Done" checkbox stays unchecked until a playtester confirms the 20–40 min timing.)

- [ ] **Step 4: Commit**

```powershell
git add Assets/Docs/BuildPlan.md
git commit -m "Tick Phase 1 build plan checkboxes"
```

---

## Self-Review Notes

- **Spec coverage:** Every item in the spec's Act 0 section has a task. Berry + BerryShine items (Task 1), Berry Shine recipe (Task 2), BerryBush foraging (Task 3), Tormod dusk schedule (Task 4), economy verification (Task 5), scene layout (Task 6), end-to-end verification (Task 7).
- **Placeholder scan:** No TBDs or TODOs. All code is shown inline.
- **Type consistency:** `ContentDb.Berry` / `ContentDb.BerryShine` used consistently across Tasks 1–5. `InteractType.Forage` defined in Task 3, used in BerryBush and GameHUD. `SellerType.Tormod` already exists. `BerryBush.Create(Vector3)` matches project factory-method pattern.
- **Compile order:** Tasks 1–2 are content-only (no new types referenced before definition). Task 3 creates BerryBush and InteractType.Forage — self-contained. Task 4 modifies SellManager (existing type). Task 5 is tests only. Task 6 is scene-only.
