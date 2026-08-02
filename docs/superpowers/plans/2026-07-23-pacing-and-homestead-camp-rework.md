# Pacing Fix & Homestead-on-Camp Rework — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**This is an amendment to Phase 1 (Act 0), not a new phase.** It does not open any Phase 2+ scope in `Assets/Docs/BuildPlan.md` and must not touch Phase 2+ line items. It combines two approved design revisions written after Phase 1's checkboxes were already ticked off in code:

- `docs/superpowers/specs/2026-07-22-pacing-fix-design.md` — starter berries on day 1, 6h→3h Berry Shine ferment, scattered berry bushes, a `RecipeDiscovered` hidden/discovered recipe system (Berry Shine exempt, always visible).
- `docs/superpowers/specs/2026-07-22-phase1-act0-design.md` (Map / Homestead sections, revised 2026-07-22) — the camp clearing is the player's own land; the Homestead is built/restored on that same plot, not purchased as a separate derelict building at town edge.

**Goal:** Land both design revisions in code without regressing anything Phase 1 already shipped, then update `Assets/Docs/BuildPlan.md` Phase 1 to describe the new design — while leaving the Phase 1 "Done" playtest gate unchecked until a real playtest confirms the 20–40 min timing under the new flow.

## Architecture

No new manager types. Every change either adds an additive layer on top of existing systems or reworks a single already-shipped behavior in place:

- **Recipe discovery** is a new gating layer inside `FermentManager`, orthogonal to the existing `unlockedByBuildingId`/`minReputation` gate (`IsRecipeUnlocked`). A recipe must be **both** discovered and unlocked to be startable; discovered-but-locked recipes show "(Locked)" (existing behavior); undiscovered recipes don't render in the list at all (new). Discovery state is a `HashSet<string>` keyed by `recipe.recipeName` — no new `recipeId` field on `RecipeData`, reusing the same "string identity" convention `unlockedByBuildingId` already uses against `Building.BuildingName`.
- **Building-gated recipes auto-discover** when their gating building becomes `Restored`, by having `FermentManager` subscribe to the existing `GameEvents.BuildingStateChanged` (no new call sites needed inside `BuildingManager`/`Building`). This is a deliberate deviation from the spec's literal wording (which imagines hand-fired `RecipeDiscovered` calls at each discovery trigger) — for Phase 1 the only building-gated recipe that matters is Basic Mash/Homestead, and piggybacking on the existing event preserves the current "Homestead restored → Basic Mash appears" behavior with zero new call sites. Reputation-gated recipes (Aged Reserve) and other building gates (Bakery, Mill — buildings that don't exist yet) simply stay hidden until a future phase adds their own discovery trigger; this is intentional, not a gap to fix here.
- **Starting inventory** is granted once, in `InventoryManager.Start()`, gated on `TimeManager.Instance.Day == 1`. This supersedes the pacing spec's literal suggestion to gate the grant on "Berry Shine not yet discovered" — that condition can never be true, since Berry Shine is defined as permanently discovered and never passes through the `RecipeDiscovered` gate.
- **Ferment time and berry bush distribution** are direct edits/relocations of already-shipped Act 0 content (see per-task "Replaces" notes).
- **Homestead relocation** moves the existing `Building` GameObject's exterior transform into the camp clearing; the interior transition (`InteriorManager`) is decoupled from exterior position (it only cares about `Building.InteriorSpawn`), so no interior rework is needed.

## Tech Stack

Unity 6 (6000.2.14f1), C#, NUnit (EditMode + PlayMode via Unity Test Framework).

## Current-code findings that shaped task order

Verified directly in the repo before writing this plan:

- `Assets/Scripts/FermentManager.cs:30` — Berry Shine is `new RecipeData("Berry Shine", 6, 2, ContentDb.BerryShine)` — still 6h, not yet 3h.
- `Assets/Scripts/InventoryManager.cs` — no `Start()` method, no day-1 grant logic anywhere in the codebase.
- `Assets/Scripts/GameEvents.cs` — no `RecipeDiscovered` event; `FermentManager` has no discovery tracking at all (`Recipes`/`UnlockedRecipes`/`IsRecipeUnlocked` only, gated purely by building/reputation).
- `Assets/Scripts/UI/RecipeSelectUI.cs:72-90` — iterates **all** `FermentManager.Instance.Recipes` and shows locked recipes with a "(Locked)" hint; there is no concept of a hidden/undiscovered recipe today.
- Scene geometry (`Assets/Scenes/SampleScene.unity`, confirmed via the actual Transform values): the camp clearing is clustered around **x ≈ -18 to -22, y ≈ 2-3** (Tent at `(-22, 2.5)`, CampfirePot at `(-20, 2)`, one BerryBush at `(-18.5, 3)`) — and the **Homestead building sits at `x=16, y=-5.3`**, roughly 35+ tiles away on the opposite side of the map, near the town strip. This is the single biggest finding driving task order: the Homestead is not "near" the camp at all right now, it's a fully separate standalone building at the far side of town, exactly as the old town-edge design intended and exactly as the new design says must change. There are exactly 3 `BerryBush` instances in the scene today (not 8-10), consistent with the pacing spec's "remove the 3 clustered bushes" framing.
- `docs/superpowers/specs/2026-07-22-phase1-act0-design.md` already carries a "Design change pending (2026-07-23)" callout in its Map section flagging this exact gap and pointing at this plan (written after that callout was added).

Task order below follows the instructions' rationale: additive/low-risk scaffolding first (discovery system, starting inventory), then a pure numeric tune (ferment time), then a scene-only content reshuffle (bush scatter), then the largest rework of already-shipped work (Homestead relocation) last, then the doc update.

## Global Constraints

- Follow `AGENTS.md` exactly: no comments in code, event bus via `GameEvents`, no direct cross-manager calls for cross-system notification (managers may still read/call another manager's own public methods the way `FermentManager`/`BuildingManager` already do today, as long as the resulting state change still fires a `GameEvents.OnXxx`), `Rules/` stays pure C# (only `Mathf` allowed), IMGUI for UI.
- Commit messages: plain descriptive text. NEVER add Co-Authored-By or any Claude/AI attribution.
- Do NOT commit this plan file or anything under `docs/superpowers/plans/` (user rule).
- `Assets/Docs/BuildPlan.md` IS committed — it's a project doc, not a plan file.
- Unity must be CLOSED before running batchmode test commands, or they fail with "already open in another instance."
- Test commands (PowerShell, from repo root):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected after a green run: `Select-String` prints `False`. For PlayMode replace both `editmode` strings with `playmode` and `-testPlatform EditMode` with `-testPlatform PlayMode`. If the Unity MCP server is running, the `run_tests` tool with an assembly filter is an acceptable faster alternative.
- If the Unity Hub editor path differs, find it with: `Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Directory`

---

### Task 1: Recipe discovery scaffolding (`RecipeDiscovered` event, hidden/discovered recipes, Berry Shine exempt)

**Files:**
- Modify: `Assets/Scripts/GameEvents.cs`
- Modify: `Assets/Scripts/FermentManager.cs`
- Modify: `Assets/Scripts/UI/RecipeSelectUI.cs`
- Test: `Assets/Tests/PlayMode/RecipeDiscoveryTests.cs` (new)

**Interfaces:**
- Consumes: existing `GameEvents.BuildingStateChanged` (fired by `BuildingManager`), existing `RecipeData.recipeName`/`unlockedByBuildingId`.
- Produces: `GameEvents.RecipeDiscovered` event + `OnRecipeDiscovered(string recipeId)` invoker. `FermentManager.IsRecipeDiscovered(RecipeData)` — Berry Shine always true; other recipes true once discovered (via the event, or automatically once their gating building is `Restored`). `RecipeSelectUI` no longer renders undiscovered recipes at all (previously it rendered every recipe, locked or not). Task 3 depends on nothing here; this task is purely additive and does not change Berry Shine's availability.
- This is additive/low-risk: it doesn't change what's startable today (Berry Shine stays always-visible, Basic Mash still appears exactly when Homestead is restored) — it only adds the plumbing for future recipes to start hidden.

- [ ] **Step 1: Add the event to `Assets/Scripts/GameEvents.cs`**

Add the declaration after `RecipeSelectionRequested`:

```csharp
    public static event System.Action<FermentVat> RecipeSelectionRequested;
    public static event System.Action<string> RecipeDiscovered;
```

Add the invoker after `OnRecipeSelectionRequested`:

```csharp
    public static void OnRecipeSelectionRequested(FermentVat vat)
        => RecipeSelectionRequested?.Invoke(vat);

    public static void OnRecipeDiscovered(string recipeId)
        => RecipeDiscovered?.Invoke(recipeId);
```

- [ ] **Step 2: Edit `Assets/Scripts/FermentManager.cs`**

Add the discovery set field, seeded with Berry Shine, right after `_lastProgressPercent`:

```csharp
    private readonly List<FermentVat> _vats = new();
    private readonly Dictionary<FermentVat, int> _lastProgressPercent = new();
    private readonly HashSet<string> _discoveredRecipes = new() { "Berry Shine" };
```

Add `OnEnable`/`OnDisable` right after `Awake()`:

```csharp
    private void OnEnable()
    {
        GameEvents.RecipeDiscovered += OnRecipeDiscovered;
        GameEvents.BuildingStateChanged += OnBuildingRestored;
    }

    private void OnDisable()
    {
        GameEvents.RecipeDiscovered -= OnRecipeDiscovered;
        GameEvents.BuildingStateChanged -= OnBuildingRestored;
    }

    private void OnRecipeDiscovered(string recipeId)
    {
        _discoveredRecipes.Add(recipeId);
    }

    private void OnBuildingRestored(Building b, BuildingState oldState, BuildingState newState)
    {
        if (newState != BuildingState.Restored) return;
        foreach (var recipe in _recipes)
            if (recipe.unlockedByBuildingId == b.BuildingName)
                _discoveredRecipes.Add(recipe.recipeName);
    }
```

Add the public query method right after `IsRecipeUnlocked`:

```csharp
    public bool IsRecipeDiscovered(RecipeData recipe) => _discoveredRecipes.Contains(recipe.recipeName);
```

- [ ] **Step 3: Edit `Assets/Scripts/UI/RecipeSelectUI.cs` — skip undiscovered recipes entirely**

In `DrawWindow`, change:

```csharp
        foreach (var recipe in FermentManager.Instance.Recipes)
        {
            bool unlocked = FermentManager.Instance.IsRecipeUnlocked(recipe);
```

to:

```csharp
        foreach (var recipe in FermentManager.Instance.Recipes)
        {
            if (!FermentManager.Instance.IsRecipeDiscovered(recipe))
                continue;

            bool unlocked = FermentManager.Instance.IsRecipeUnlocked(recipe);
```

- [ ] **Step 4: Write `Assets/Tests/PlayMode/RecipeDiscoveryTests.cs`**

```csharp
using System.Collections;
using System.Linq;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class RecipeDiscoveryTests
{
    private FermentManager _fermentManager;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<BuildingManager>();
        _fermentManager = TestBootstrap.CreateSingleton<FermentManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator BerryShine_IsDiscoveredFromStart()
    {
        var recipe = _fermentManager.Recipes.First(r => r.recipeName == "Berry Shine");
        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(recipe));
        yield return null;
    }

    [UnityTest]
    public IEnumerator OtherRecipes_NotDiscovered_UntilTriggered()
    {
        var highlandMash = _fermentManager.Recipes.First(r => r.recipeName == "Highland Mash");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(highlandMash));
        yield return null;
    }

    [UnityTest]
    public IEnumerator RecipeDiscoveredEvent_AddsToDiscoveredSet()
    {
        var highlandMash = _fermentManager.Recipes.First(r => r.recipeName == "Highland Mash");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(highlandMash));

        GameEvents.OnRecipeDiscovered("Highland Mash");

        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(highlandMash));
        yield return null;
    }

    [UnityTest]
    public IEnumerator BuildingRestored_AutoDiscoversRecipesGatedOnIt()
    {
        var sweetBatch = _fermentManager.Recipes.First(r => r.recipeName == "Sweet Batch");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(sweetBatch));

        var buildingGo = TestBootstrap.CreateGameObject("TestBuilding");
        var building = buildingGo.AddComponent<Building>();

        GameEvents.OnBuildingStateChanged(building, BuildingState.Cleared, BuildingState.Restored);

        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(sweetBatch));
        yield return null;
    }
}
```

(`Sweet Batch` is gated on `"Bakery"`, and a freshly-`AddComponent`ed `Building` defaults its private `buildingName` field to `"Bakery"` — see `Assets/Scripts/Building.cs` — so this test needs no new setter on `Building` to exercise the auto-discovery path.)

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
git add Assets/Scripts/GameEvents.cs Assets/Scripts/FermentManager.cs Assets/Scripts/UI/RecipeSelectUI.cs Assets/Tests/PlayMode/RecipeDiscoveryTests.cs
git commit -m "Add RecipeDiscovered event and hidden/discovered recipe gating to FermentManager"
```

---

### Task 2: Day 1 starting inventory (3 Berry)

**Replaces:** nothing shipped — this is new behavior. Today a fresh player starts with an empty inventory and must forage before their first ferment, the exact dead-time gap `docs/superpowers/specs/2026-07-22-pacing-fix-design.md` is written to close.

**Files:**
- Modify: `Assets/Scripts/InventoryManager.cs`
- Test: `Assets/Tests/PlayMode/StartingInventoryTests.cs` (new)

**Interfaces:**
- Consumes: `TimeManager.Instance.Day` (read-only), `ContentDb.Berry`.
- Produces: on the first `Start()` tick, if `TimeManager.Instance.Day == 1`, `InventoryManager` grants 3 Berry via its own existing `TryAdd`. Guarded by a null check on `TimeManager.Instance` so no behavior changes in any test/scene that doesn't have a `TimeManager` present.

- [ ] **Step 1: Edit `Assets/Scripts/InventoryManager.cs`**

Add a `Start()` method right after `Awake()`:

```csharp
    private void Start()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.Day == 1)
            TryAdd(ContentDb.Berry, 3);
    }
```

- [ ] **Step 2: Write `Assets/Tests/PlayMode/StartingInventoryTests.cs`**

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class StartingInventoryTests
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
    public IEnumerator Day1_GrantsThreeBerry()
    {
        TestBootstrap.CreateSingleton<TimeManager>();
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(3, inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator NotDay1_GrantsNoBerry()
    {
        var timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        timeManager.SetTime(2, 8, 0);
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Berry));
    }

    [UnityTest]
    public IEnumerator NoTimeManager_GrantsNoBerry_DoesNotThrow()
    {
        var inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        yield return null;
        yield return null;

        Assert.AreEqual(0, inventory.GetCount(ContentDb.Berry));
    }
}
```

- [ ] **Step 3: Run PlayMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/InventoryManager.cs Assets/Tests/PlayMode/StartingInventoryTests.cs
git commit -m "Grant 3 starting Berry on day 1 so the first ferment starts immediately"
```

---

### Task 3: Reduce Berry Shine ferment time 6h → 3h

**Replaces:** the shipped `Assets/Scripts/FermentManager.cs:30` recipe (`new RecipeData("Berry Shine", 6, 2, ContentDb.BerryShine)`, landed in the tent-prologue plan's Task 2, commit "Add Berry Shine recipe; gate Basic Mash behind Homestead") and the shipped `Assets/Tests/EditMode/BerryShineRecipeTests.cs` assertion that hardcodes 6h.

**Files:**
- Modify: `Assets/Scripts/FermentManager.cs`
- Modify: `Assets/Tests/EditMode/BerryShineRecipeTests.cs`

**Interfaces:**
- Consumes: nothing new.
- Produces: Berry Shine ferments in 3h instead of 6h. Other recipes (Basic Mash 4h, Sweet Batch 6h, Highland Mash 8h, Aged Reserve 12h) are unchanged. `Assets/Tests/PlayMode/FermentationFlowTests.cs`'s `BerryShineFermentation_CompletesAndCollects` test constructs its own local 1-hour `RecipeData` and does not reference this constant — no change needed there.

- [ ] **Step 1: Edit `Assets/Scripts/FermentManager.cs`**

Change:

```csharp
            new RecipeData("Berry Shine", 6, 2, ContentDb.BerryShine)
                .AddIngredient(ContentDb.Berry, 3),
```

to:

```csharp
            new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)
                .AddIngredient(ContentDb.Berry, 3),
```

- [ ] **Step 2: Edit `Assets/Tests/EditMode/BerryShineRecipeTests.cs`**

Replace the whole file with:

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
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(3, recipe.Costs[_berry]);
        Assert.AreEqual(1, recipe.Costs.Count);
    }

    [Test]
    public void BerryShineRecipe_Yields2()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(2, recipe.outputCount);
    }

    [Test]
    public void BerryShineRecipe_3HourFerment()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.AreEqual(3, recipe.fermentationHours);
    }

    [Test]
    public void BerryShineRecipe_NoBuildingGate()
    {
        var recipe = new RecipeData("Berry Shine", 3, 2, _berryShine)
            .AddIngredient(_berry, 3);

        Assert.IsNull(recipe.unlockedByBuildingId);
    }
}
```

- [ ] **Step 3: Run EditMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/FermentManager.cs Assets/Tests/EditMode/BerryShineRecipeTests.cs
git commit -m "Reduce Berry Shine ferment time from 6h to 3h"
```

---

### Task 4: Scatter berry bushes 8-10 across the map

**Replaces:** the shipped scene layout from the tent-prologue plan's Task 6 Step 4 ("Add 3 GameObjects in the camp clearing with the BerryBush component"). Confirmed in the live scene: exactly 3 `BerryBush` instances exist today, clustered at camp (one confirmed at world position `(-18.5, 3)`, i.e. right next to the Tent at `(-22, 2.5)` and CampfirePot at `(-20, 2)`) — the opposite of the exploration-during-brew pacing this task is meant to create.

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity editor, not by hand)

**Interfaces:**
- Consumes: existing `BerryBush.Create(Vector3)` factory / existing `BerryBush` component (no code changes — `Assets/Scripts/BerryBush.cs` and its respawn-on-`DayEnded` behavior are untouched, `Assets/Tests/EditMode/BerryBushTests.cs` needs no changes).
- Produces: 8-10 `BerryBush` instances distributed as: a couple still near camp (but not immediately adjacent to the CampfirePot vat — keep a short walk between "start a ferment" and "the nearest bush already picked clean"), a few along the path between the camp clearing (~x -18 to -22) and the town strip, a few in the town outskirts, and 1-2 in hidden corners/edges of the explorable area.

- [ ] **Step 1: Open the project in Unity. Locate and delete the 3 existing `BerryBush` GameObjects clustered at camp** (one is at world position `(-18.5, 3)`; find the other two nearby in the Hierarchy/Scene view — they were all added together in the tent-prologue plan's Task 6).

- [ ] **Step 2: Re-add 2 BerryBush instances near camp**, a bit further from the CampfirePot than before (e.g., at the edges of the ~8×6 camp clearing rather than immediately beside the pot), so the player still finds berries on arrival but has to walk for them.

- [ ] **Step 3: Add 3-4 BerryBush instances along the path from camp toward the town strip** (between camp's x ≈ -20 and the town strip's x ≈ 0), so the player encounters them naturally while walking to sell.

- [ ] **Step 4: Add 2-3 BerryBush instances in the town outskirts** (near the edges of the existing 60×20 town strip, away from the main street).

- [ ] **Step 5: Add 1-2 BerryBush instances in hidden corners** — behind a building, at a map edge, or another spot that rewards actually exploring rather than beelining camp-to-town.

Each new instance: either call `BerryBush.Create(position)` from a temporary editor script/console, or manually build one matching the existing prefab shape (SpriteRenderer with purple tint, `BoxCollider2D` trigger sized `0.6×0.8`, `Interactable` layer, `BerryBush` component) — same as the other bushes already in the scene.

- [ ] **Step 6: Verify in Play mode.** No console errors. Walk from the tent to each new bush and forage it once; confirm none overlap the Tent/CampfirePot/other colliders and all are reachable within the existing walkable area.

- [ ] **Step 7: Save the scene and commit**

```powershell
git add Assets/Scenes/SampleScene.unity
git commit -m "Scatter berry bushes across camp, road, town outskirts, and hidden corners"
```

---

### Task 5: Relocate Homestead onto the player's own camp clearing; reframe purchase as build/restore

**Replaces:** the shipped scene placement from the tent-prologue plan's Task 6 Step 5 ("Create the Homestead Building ... at the town edge") and commit `3c0cd96` ("rebuild Homestead as proper building"). Confirmed in the live scene: the Homestead `Building` instance currently sits at world position `x=16, y=-5.3`, roughly 35+ tiles from the camp clearing cluster (`x ≈ -18 to -22, y ≈ 2-3`) — exactly the "derelict building at town edge" the old design called for, and exactly what `docs/superpowers/specs/2026-07-22-phase1-act0-design.md`'s pending-change note (already in that file) says must move.

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity editor, not by hand)
- Modify: `Assets/Scripts/UI/GameHUD.cs`
- Modify: `Assets/Scripts/BuildingManager.cs`

**Interfaces:**
- Consumes: existing `Building`/`BuildingManager`/`InteriorManager` — no changes to `Building.cs` itself; `InteriorManager.EnterInterior` only reads `Building.InteriorSpawn`, which is unaffected by moving the exterior GameObject, so no interior rework is needed.
- Produces: the Homestead's exterior GameObject lives on the same plot as the Tent/CampfirePot. The purchase prompt and toasts read "Build"/"Built" instead of "Buy"/"Purchased" specifically for the Homestead (other buildings — Bakery, etc. — keep "Buy"/"Purchased" wording, since those genuinely are separate purchases in later phases).

- [ ] **Step 1: In the Unity Editor, select the Homestead GameObject** (currently at world position `(16, -5.3)`) and move it into the camp clearing — e.g. somewhere around `(-14, 1)` to `(-15, 0)`, close enough to the Tent (`-22, 2.5`) and CampfirePot (`-20, 2`) to read as the same plot, but with enough clearance that its board/debris/repair interaction footprint (debris spawns relative to `building.transform.position`/`BoardTrigger`, per `BuildingManager.SpawnDebris`) doesn't overlap the Tent, CampfirePot, or the relocated berry bushes from Task 4. Extend the camp clearing's ground tilemap slightly if needed to cover the new footprint (existing ground tiles only — no new art, per the "No art before Phase 7" rule).

- [ ] **Step 2: Verify the Homestead's `interiorSpawn` reference and board/door trigger colliders moved correctly with the parent** (they're children of the Building GameObject, so they should follow automatically — confirm in the Inspector after the move).

- [ ] **Step 3: Edit `Assets/Scripts/UI/GameHUD.cs` — "Build" instead of "Buy" for the Homestead**

In `UpdateInteractPrompt`, change:

```csharp
                    BuildingState.Abandoned => $"[E] Buy {building.BuildingName} ({building.PurchaseCost}g)",
```

to:

```csharp
                    BuildingState.Abandoned => building.BuildingName == "Homestead"
                        ? $"[E] Build {building.BuildingName} ({building.PurchaseCost}g)"
                        : $"[E] Buy {building.BuildingName} ({building.PurchaseCost}g)",
```

- [ ] **Step 4: Edit `Assets/Scripts/BuildingManager.cs` — "Built"/"Can't afford to build" instead of "Purchased"/"Can't afford" for the Homestead**

Replace `TryPurchase`:

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

with:

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

(No test file references `TryPurchase`'s toast strings or the Homestead by name — confirmed via search of `Assets/Tests/` — so no test updates are needed here; `BuildingRenovationFlowTests.cs` only asserts on `BuildingState`/cash, not toast text.)

- [ ] **Step 5: Verify compilation via EditMode run**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 6: Manual play verification (Play mode)**

1. Walk from the Tent to the Homestead — should now be a short walk within the camp clearing, not a cross-map trek.
2. Prompt reads `[E] Build Homestead (80g)` while Abandoned.
3. Purchase it — toast reads `Started building Homestead (-80g)`.
4. Smash boards, clear debris, repair as before — no change to that pipeline.
5. Enter the restored interior — `InteriorManager` transition still works (fade + spawn at `interiorSpawn`).
6. Confirm no other building's prompts/toasts changed wording (spot check the Bakery or another `Building` instance in the scene still says "Buy"/"Purchased").

- [ ] **Step 7: Save the scene and commit**

```powershell
git add Assets/Scenes/SampleScene.unity Assets/Scripts/UI/GameHUD.cs Assets/Scripts/BuildingManager.cs
git commit -m "Relocate Homestead onto the player's own camp clearing; reframe purchase as build/restore"
```

---

### Task 6: Update BuildPlan.md Phase 1 to reflect the new design

**Files:**
- Modify: `Assets/Docs/BuildPlan.md`

**Interfaces:**
- Consumes: Tasks 1-5.
- Produces: an accurate Phase 1 section. The "Done" gate line stays unchecked — it requires an actual playtest under the new flow, not just code landing.

- [ ] **Step 1: Replace the Phase 1 section**

Change:

```markdown
## Phase 1 — Act 0: the tent prologue (in progress)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), forage verb = existing interact.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, longer ferment).
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Homestead purchase: derelict building at town edge; price reachable in ~3 sales; unlocks proper still + vat + game proper.
- [x] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.
```

to:

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

Do NOT check the "Done" box — it stays unchecked until a playtester confirms the 20-40 min timing under this new flow.

- [ ] **Step 2: Commit**

```powershell
git add Assets/Docs/BuildPlan.md
git commit -m "Update Phase 1 build plan for pacing fix and homestead-on-camp rework"
```

---

## Self-Review Notes

- **Spec coverage:** every item in both specs has a task — starter inventory (Task 2), 3h ferment (Task 3), scattered bushes (Task 4), `RecipeDiscovered`/hidden-recipes/Berry-Shine-exempt (Task 1), Homestead-on-camp + build/restore reframing (Task 5), BuildPlan sync (Task 6).
- **Order rationale:** additive scaffolding (Tasks 1-2) before numeric tuning (Task 3) before scene content reshuffle (Task 4) before the largest rework of shipped work (Task 5) before the doc update (Task 6) — matches the instructions' stated ordering logic and keeps each task independently compilable and testable.
- **No Phase 2+ creep:** nothing here touches near-forest/deep-woods tilemap expansion, delivery runs, patrols, or infrastructure sockets — those stay Phase 2/3/4 scope untouched.
- **Regression care:** Task 1's building-auto-discovery hides Sweet Batch/Highland Mash/Aged Reserve (gated on Bakery/Mill/reputation) behind the new discovery gate even though they were previously visible-but-locked — this is intentional (those buildings don't exist yet in the Phase 1 scene) and doesn't affect any Phase 1 acceptance criteria, which only concern Berry Shine and Basic Mash.
- **Gate discipline:** Task 6 explicitly leaves the Phase 1 "Done" checkbox unchecked; only a real playtest under the new flow may check it.
