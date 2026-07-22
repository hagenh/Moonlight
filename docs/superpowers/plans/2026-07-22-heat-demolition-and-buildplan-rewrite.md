# Heat Demolition & BuildPlan Rewrite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the persistent heat/suspicion/raid systems per the approved redesign spec, rework the guard bribe into "pay to keep cargo," and rewrite `Assets/Docs/BuildPlan.md` as the roadmap for the front-town empire redesign.

**Architecture:** Pure deletion/rework of the tension layer. The spec (`docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md`) replaces the persistent heat meter with opt-in delivery-run tension built in later plans. This plan leaves the game compiling, tests green, and free of heat — guards stay as a single fixed patrol until the runs plan repurposes them. Reputation is NOT removed here (it still drives dialogue tiers and recipe gates; it dies in the narrative plan when conspiracy trust replaces it).

**Tech Stack:** Unity 6 (6000.2.14f1), C#, NUnit (EditMode + PlayMode via Unity Test Framework).

## Global Constraints

- This is plan 1 of a series. Later plans (not this one): Act 0 prologue, world map, delivery runs, infrastructure, narrative. Do not build any of those here.
- Follow `AGENTS.md` exactly: no comments in code, event bus via `GameEvents`, no direct cross-manager calls, Rules/ stays pure C# (only `Mathf` allowed), IMGUI for UI.
- Commit messages: plain descriptive text. NEVER add Co-Authored-By or any Claude/AI attribution (user rule, overrides all defaults).
- Do NOT commit this plan file or anything under `docs/superpowers/plans/` (user rule). `Assets/Docs/BuildPlan.md` IS committed — it's a project doc.
- Unity must be CLOSED before running batchmode test commands, or they fail with "already open in another instance."
- Test commands (PowerShell, from repo root; create `TestResults/` implicitly):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected after a green run: `Select-String` prints `False`. For PlayMode replace both `editmode` strings with `playmode` and `-testPlatform EditMode` with `-testPlatform PlayMode`. If the Unity MCP server is running, the `run_tests` tool with an assembly filter is an acceptable faster alternative (see `AGENTS.md`).
- If the Unity Hub editor path differs, find it with: `Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Directory`

---

### Task 1: Rewrite Assets/Docs/BuildPlan.md

**Files:**
- Modify: `Assets/Docs/BuildPlan.md` (full replacement)

**Interfaces:**
- Consumes: `docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md` (the approved spec — read it first).
- Produces: the phase roadmap that later plans (Act 0, world, runs, infrastructure, narrative) will be written against. Phase D of this doc describes THIS plan's code tasks.

- [ ] **Step 1: Replace the entire content of `Assets/Docs/BuildPlan.md` with:**

```markdown
# LAMPLIGHT — Build Plan v2 (Front-Town Empire slice)

Spec: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md — read it before any phase.
Stack: Unity 2D (URP), C#, Tilemap, Light2D. Evenings/weekends; phases are scoped, not dated.

## Slice contents
- Fantasy: moonshiner rebuilds a dying town as the perfect front. Day = cozy front life; night = opt-in delivery runs. You light the town and keep the woods dark.
- One connected exterior map, three depths: street (existing 60×20) → near forest (camp, foraging) → deep woods (routes, destinations). Interiors: Roadhouse + homestead only; rest facade-only.
- Systems: movement/interaction · building states · staged construction · production (mash → ferment → bottle) · stand (safe channel) · delivery runs (routes, patrols, load-outs) · day-night + sleep-save · conspiracy trust · recruitment beats · two-layer infrastructure · JSON save.
- 8 NPCs: Tormod, Berta, Signe, Aksel, Ingrid, Elias, Mrs. Holt, Constable Aas (antagonist, not recruitable).
- Cliffhanger: Mill cellar, locked from the inside. Metric: do they ask what's in the cellar?
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene framework, combat, minimap, co-op, free placement (sockets only), corrupt-deputy arc.

## Phase D — Demolition (in progress)
- [ ] Delete heat/suspicion: meter, tiers, guard-count scaling, sleep raids, heat decay, suspicion pricing, risky buyer.
- [ ] Bribe rework: caught while carrying → pay to keep cargo, refuse to lose it. No heat aftermath.
- [ ] Guards: single fixed patrol until Phase 3 repurposes them onto routes.
- [ ] Keep: reputation (dies in Phase 5 with conspiracy trust), stand plan, staged construction, sleep pipeline (minus punishment).
- [ ] Done: compiles, all tests green, no reference to Heat anywhere in Assets/Scripts.

## Phase 1 — Act 0: the tent prologue
- [ ] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [ ] Foraging: berry bushes (respawn daily), forage verb = existing interact.
- [ ] Berry shine recipe (wild yeast — no yeast ingredient, longer ferment).
- [ ] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [ ] Homestead purchase: derelict building at town edge; price reachable in ~3 sales; unlocks proper still + vat + game proper.
- [ ] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.

## Phase 2 — World: one map, three depths
- [ ] Extend tilemap: near forest (camp + foraging) and deep woods with 3 route corridors and 3 destination sites (logging camp, river dock, crossroads).
- [ ] No exterior scene loads; interiors stay separate (existing InteriorManager).
- [ ] Walk timings: town end-to-end 12–15 s (existing), camp ~20 s from town, destinations 60–90 s.
- [ ] Darkness pass: night in the woods is genuinely dark (Light2D); the lit town visible from the treeline — screenshot this.
- [ ] Done: walk street → camp → each destination and back, day and night.

## Phase 3 — Delivery runs
- [ ] Destinations buy at run prices: stand (safe, low) < Roadhouse (safe, capped/day, medium) < runs (high).
- [ ] Appointments: logging camp payday Fridays (2× demand) · river dock barge nights · crossroads wagon on set nights/hours. All recur; nothing permanently missable.
- [ ] Routes: main road (fast, night checkpoints) · forest trail (slow, dark, sparse) · creek path (locked until shortcut plank).
- [ ] Patrols: existing Guard vision cones on fixed waypoint schedules per route/hour. NO random spawns, ever. Detection only while carrying cargo. Patrols only on routes at night — town and near forest never patrolled.
- [ ] Caught: cargo confiscated; bribe keeps it (cost scales with load). Nothing else. Ever.
- [ ] Load-outs: satchel 2 jars (off-path capable) → handcart 8 jars (path-bound, wider profile, built by Aksel in Phase 6) → courier automation (5 clean runs on a route + Boarding House recruit → auto-resolve for a cut).
- [ ] Done: full loop — brew by day, run by night, near-miss stories happen unscripted.

## Phase 4 — Two-layer infrastructure
- [ ] Public sockets (street): lamppost, plank sidewalk, bench, flower box, sign. Effects: night light, small stand buff at beauty thresholds, dialogue reactions. Never any downside.
- [ ] Covert sockets (forest): stash barrel (ditch/retrieve cargo mid-run) · trail marker (faint night glint) · shortcut plank (unlocks creek path). Lookout perch = stretch, cut first.
- [ ] Done: a player who beautifies the street AND builds the smuggler's toolkit feels both are "mine."

## Phase 5 — Narrative: conspiracy trust + recruitment
- [ ] NarrativeFlags + MilestoneDetector + conditional DialogueResolver per Assets/Docs/NarrativeDesign.md architecture (still valid — reskin meanings only).
- [ ] Per-NPC conspiracy trust gates function tiers AND dialogue (Signe t1 discounts, t2 sales buff).
- [ ] Recruitment beats on move-in coroutine tech: Tormod (Act 0), Berta (catches you, covers unprompted), Signe, Aksel, Ingrid, Elias.
- [ ] Global reputation DIES here: remove rep meter/HUD/recipe gates; replace gates with trust/flags.
- [ ] 5 fragments = the old operation's story; sources: clearing debris, recruit gifts, milestones.
- [ ] Done: full Bakery arc — restore → Berta beat → bread-cart cover unlocked → her window lights.

## Phase 6 — Content build-out
- [ ] Buildings ×7 (front / function / track): Roadhouse (first buyer) · Bakery (yeast, bread-cart cover) · General Store (supply, sales buff) · Smithy & Cooperage (still upgrades, handcart, barrels) · Apothecary (botanicals, recipes) · Boarding House (recruits, rent) · Old Mill (bulk grain, cellar, endgame — Holt-gated).
- [ ] Constable's office: never purchasable. Light always on.
- [ ] Quality ladder: berry shine → corn/grain → aged (barrels) → flavored (botanicals).
- [ ] Mill stage 1 complete → cellar door → locked-from-inside line → title card.
- [ ] Numbers pass: homestead 20–40 min · first night run ~1 h · Mill cliffhanger 4–6 h.
- [ ] Done: stranger plays start → cliffhanger, zero instructions.

## Phase 7 — Art + audio
- [ ] One 16×16 tileset family incl. forest. Facades: 7 restored bases + shared overlay kit.
- [ ] Portraits > walk cycles; 2 emotion variants for Holt/Aas/Berta.
- [ ] Light pass: warm windows (flicker), player-placed lampposts, dark woods, lantern cone on runs.
- [ ] Audio ≈ 20 SFX + 2 loops. Priorities: deed stamp (THICK) · lamp-lighting sting (commission if anything) · night-run ambience layer.

## Phase 8 — Playtest + tune
- [ ] 3 testers, recorded, you silent.
- [ ] Collect: time to homestead (20–40 min) · time to first night run (<75 min) · caught-players can name their mistake (legibility check — if not, patrol/telegraphing bug) · stuck >60 s anywhere · unprompted reaction at first lamppost lighting · do they ask what's in the cellar?
- [ ] Cut pass: confused 2 of 3 → fix or cut; noticed by nobody → cut. No additions in final week.

## Rules
- Still minigame stays deferred — revisit only if a fun design emerges.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 7. Juice allowed early.
- Save versioning + tolerant deserializer from day one.
- Design guardrails (from spec, non-negotiable): never punish daytime play · no hidden dice against the player · Act 0 is a prologue · appointments recur · beautification is never punished.
- Every mid-build idea → the LATER note, unexamined. Review once, at playtest.
```

- [ ] **Step 2: Verify the file renders sanely**

Run: `Get-Content Assets\Docs\BuildPlan.md -TotalCount 5`
Expected: the new `# LAMPLIGHT — Build Plan v2` header.

- [ ] **Step 3: Commit**

```powershell
git add Assets/Docs/BuildPlan.md
git commit -m "Rewrite build plan around front-town empire redesign"
```

---

### Task 2: Remove heat decay and raids from SleepManager

**Files:**
- Modify: `Assets/Scripts/SleepManager.cs`
- Test: `Assets/Tests/PlayMode/SleepTickOrderingTests.cs`

**Interfaces:**
- Consumes: `GameEvents.OnSleepInitiated(int)`, `OnSleepCompleted(int)`, `TimeManager.AdvanceToDayEnd()`, `ResidentManager.RunMoveInChecks()` — all unchanged.
- Produces: `SleepManager` no longer references `GameManager.AddHeat`, `EconomyRules.RaidThreshold/RaidFine/RaidCrateLossPercent/RaidSuspicionReset`, or `GameEvents.OnRaidOccurred`. Sleep pipeline order becomes: fade → SleepInitiated → AdvanceToDayEnd → move-in checks → SleepCompleted → fade → move-in sequence. Task 4 relies on the raid constants being unreferenced; Task 5 relies on `AddHeat` and `OnRaidOccurred` being unreferenced here.

- [ ] **Step 1: Rewrite the tests to the target behavior**

(Deletion task: the reduced suite also passes against the OLD code — that's expected. The gate is Step 3 passing after Step 2's edit; the point of rewriting first is that the new suite no longer references `SetHeat`/`HeatChanged`, so Step 2 can't break compilation of tests.)

Replace the entire content of `Assets/Tests/PlayMode/SleepTickOrderingTests.cs` with:

```csharp
using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SleepTickOrderingTests
{
    private GameManager _gameManager;
    private TimeManager _timeManager;
    private SleepManager _sleepManager;
    private ResidentManager _residentManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        _residentManager = TestBootstrap.CreateSingleton<ResidentManager>();
        _sleepManager = TestBootstrap.CreateSingleton<SleepManager>();

        _recorder = new EventRecorder();

        GameEvents.SleepInitiated += (day) => _recorder.Record("SleepInitiated", day);
        GameEvents.DayEnded += (day) => _recorder.Record("DayEnded", day);
        GameEvents.HourChanged += (hour, day) => _recorder.Record("HourChanged", $"{hour}/{day}");
        GameEvents.ResidentMovedIn += (def, b) => _recorder.Record("ResidentMovedIn");
        GameEvents.SleepCompleted += (newDay) => _recorder.Record("SleepCompleted", newDay);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator BeginSleep_FiresEventsInCorrectOrder()
    {
        _timeManager.SetTime(1, 10, 0);
        _recorder.Clear();

        _sleepManager.BeginSleep();

        for (int i = 0; i < 5; i++)
            yield return null;

        int sleepInitiatedIdx = FindFirstIndexStartingWith("SleepInitiated");
        int dayEndedIdx = FindFirstIndexStartingWith("DayEnded");
        int sleepCompletedIdx = FindFirstIndexStartingWith("SleepCompleted");

        Assert.GreaterOrEqual(sleepInitiatedIdx, 0, "SleepInitiated should have fired");
        Assert.GreaterOrEqual(dayEndedIdx, 0, "DayEnded should have fired");
        Assert.GreaterOrEqual(sleepCompletedIdx, 0, "SleepCompleted should have fired");

        Assert.Less(sleepInitiatedIdx, dayEndedIdx, "SleepInitiated must fire before DayEnded");
        Assert.Less(dayEndedIdx, sleepCompletedIdx, "DayEnded must fire before SleepCompleted");
    }

    [UnityTest]
    public IEnumerator BeginSleep_AdvancesDay()
    {
        _timeManager.SetTime(1, 10, 0);

        _sleepManager.BeginSleep();

        for (int i = 0; i < 5; i++)
            yield return null;

        Assert.AreEqual(2, _timeManager.Day, "Day should advance after sleep");
    }

    [UnityTest]
    public IEnumerator BeginSleep_WhenAlreadySleeping_DoesNothing()
    {
        _timeManager.SetTime(1, 10, 0);

        _sleepManager.BeginSleep();
        yield return null;

        _recorder.Clear();

        _sleepManager.BeginSleep();
        yield return null;

        Assert.AreEqual(0, _recorder.Count, "Second BeginSleep should not fire any additional events");
    }

    private int FindFirstIndexStartingWith(string prefix)
    {
        for (int i = 0; i < _recorder.Count; i++)
        {
            if (_recorder.Order[i].StartsWith(prefix))
                return i;
        }
        return -1;
    }
}
```

(Deleted: `BeginSleep_DecaysHeat`, the `HeatChanged` recording, the `SetHeat(30)` calls, and the heat-ordering assertions.)

- [ ] **Step 2: Edit `Assets/Scripts/SleepManager.cs`**

Remove the field:

```csharp
    [SerializeField] private int heatDecayPerNight = 5;
```

In `SleepRoutine()`, replace this block:

```csharp
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddHeat(-heatDecayPerNight);

            if (GameManager.Instance.Heat >= EconomyRules.RaidThreshold)
                ExecuteRaid();
        }

        bool moveInPending = false;
```

with:

```csharp
        bool moveInPending = false;
```

Delete the entire `ExecuteRaid()` method (the whole method from `private void ExecuteRaid()` through its closing brace, including the `FindObjectsByType<Crate>` loop, the fine, `SetHeat`, and both `GameEvents.OnToastRequested`/`OnRaidOccurred` calls).

- [ ] **Step 3: Run PlayMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False` (no failures).

- [ ] **Step 4: Commit**

```powershell
git add Assets/Scripts/SleepManager.cs Assets/Tests/PlayMode/SleepTickOrderingTests.cs
git commit -m "Remove sleep raids and nightly heat decay"
```

---

### Task 3: Rework guard bribe (pay keeps cargo) and fix guard count

**Files:**
- Modify: `Assets/Scripts/Guard.cs`
- Modify: `Assets/Scripts/GuardManager.cs`
- Modify: `Assets/Scripts/UI/BribeUI.cs`

**Interfaces:**
- Consumes: `GameEvents.CaughtBribe/BribePaid/BribeRefused` (unchanged), `GameManager.TrySpend(int)` (unchanged), `PlayerController.Instance.IsCarryingCrate/CarriedCrate/DropCrate()` (unchanged).
- Produces: new bribe contract — **paid = cargo kept, refused = cargo confiscated; heat is never touched.** `Guard` loses fields `suspicionOnBribe`/`suspicionOnCaught`; `GuardManager` loses the `HeatChanged` subscription and `EconomyRules.GetGuardCountForSuspicion` call (Task 4 relies on that call being gone; Task 5 relies on the `HeatChanged` subscription and `GameManager.Heat` read being gone). Guard count becomes the serialized field `guardCount` (default 1).

No automated tests exist for Guard/GuardManager (MonoBehaviour + scene-dependent); behavior is verified manually in Task 6 Step 3. Do not build test scaffolding for them in this plan — they get rebuilt as route patrols in the runs plan.

- [ ] **Step 1: Edit `Assets/Scripts/Guard.cs`**

Remove these two fields from the `[Header("Bribe")]` block (keep `bribeCost` and `lookAwayDuration`):

```csharp
    [SerializeField] private int suspicionOnBribe = 5;
    [SerializeField] private int suspicionOnCaught = 20;
```

Replace the `ResolveBribe` method:

```csharp
    private void ResolveBribe(bool paid)
    {
        ConfiscateCrate();
        if (GameManager.Instance != null)
        {
            if (paid)
            {
                GameManager.Instance.TrySpend(bribeCost);
                GameManager.Instance.AddHeat(suspicionOnBribe);
            }
            else
            {
                GameManager.Instance.AddHeat(suspicionOnCaught);
            }
        }
        _lookingAway = true;
        _lookAwayTimer = lookAwayDuration;
        _caught = false;
    }
```

with:

```csharp
    private void ResolveBribe(bool paid)
    {
        if (paid && GameManager.Instance != null && GameManager.Instance.TrySpend(bribeCost))
            GameEvents.OnToastRequested("The guard looks the other way.");
        else
            ConfiscateCrate();
        _lookingAway = true;
        _lookAwayTimer = lookAwayDuration;
        _caught = false;
    }
```

(Note the affordability guard: if `TrySpend` fails, the cargo is confiscated as if refused — `BribeUI` already disables the Pay button when unaffordable, this is defense in depth.)

- [ ] **Step 2: Edit `Assets/Scripts/GuardManager.cs`**

Replace the field:

```csharp
    private int _targetGuardCount = 1;
```

with:

```csharp
    [SerializeField] private int guardCount = 1;
```

In `OnEnable`, remove the line `GameEvents.HeatChanged += OnHeatChanged;`.
In `OnDisable`, remove the line `GameEvents.HeatChanged -= OnHeatChanged;`.

Replace `Start()`:

```csharp
    private void Start()
    {
        int suspicion = GameManager.Instance != null ? GameManager.Instance.Heat : 0;
        _targetGuardCount = EconomyRules.GetGuardCountForSuspicion(suspicion);
        SyncGuardCount();
    }
```

with:

```csharp
    private void Start()
    {
        SyncGuardCount();
    }
```

Delete the entire `OnHeatChanged` method:

```csharp
    private void OnHeatChanged(int newHeat, int oldHeat)
    {
        int newCount = EconomyRules.GetGuardCountForSuspicion(newHeat);
        if (newCount != _targetGuardCount)
        {
            _targetGuardCount = newCount;
            SyncGuardCount();
        }
    }
```

In `SyncGuardCount()`, replace both occurrences of `_targetGuardCount` with `guardCount`.

- [ ] **Step 3: Edit `Assets/Scripts/UI/BribeUI.cs` — make the copy match the new deal**

In `DrawWindow`, replace:

```csharp
        GUILayout.Label($"A guard caught you carrying moonshine!");
        GUILayout.Label($"Pay {_cost}g? (You have {cash}g)");
```

with:

```csharp
        GUILayout.Label("A guard caught you carrying moonshine!");
        GUILayout.Label($"Pay {_cost}g to keep your cargo? Refuse and lose it. (You have {cash}g)");
```

- [ ] **Step 4: Verify compilation via EditMode run (compilation errors fail the run)**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 5: Commit**

```powershell
git add Assets/Scripts/Guard.cs Assets/Scripts/GuardManager.cs Assets/Scripts/UI/BribeUI.cs
git commit -m "Rework bribe: pay keeps cargo, refuse loses it; fixed guard count"
```

---

### Task 4: Purge heat from EconomyRules and all remaining callers

**Files:**
- Modify: `Assets/Scripts/Rules/EconomyRules.cs`
- Modify: `Assets/Scripts/DeliveryPoint.cs`
- Modify: `Assets/Scripts/UI/GameHUD.cs`
- Modify: `Assets/Scripts/SellerType.cs`
- Modify: `Assets/Scripts/SellerInteractable.cs`
- Modify: `Assets/Scripts/RecipeData.cs`
- Modify: `Assets/Scripts/FermentManager.cs:30-46,177`
- Test: `Assets/Tests/EditMode/EconomyRulesTests.cs` (rewrite)
- Delete: `Assets/Tests/EditMode/SellConfiscationTests.cs` (and its `.meta` file)

**Interfaces:**
- Consumes: Tasks 2–3 already removed the raid-constant / guard-count callers.
- Produces: new signatures later tasks and plans rely on — `EconomyRules.GetSellPrice(ItemDef item)` (no seller param), `EconomyRules.GetDeliveryPrice(ItemDef item, DeliveryType type)` (no suspicion param), `SellerType` = `{ Tormod, TravelingCart }`, `RecipeData` constructor `(string recipeName, int fermentationHours, int outputCount, ItemDef outputItem, string unlockedByBuildingId = null, int minReputation = 0)`. Deleted entirely: `SuspicionTier`, `GetSuspicionTier`, `GetGuardCountForSuspicion`, `ShouldConfiscate`, `GetSuspicionForDrop`, `RiskyBuyerAppearsToday`, `PickHour`, all Risky/Confiscation/Raid constants, `FermentManager.FindRecipeForItem`.

- [ ] **Step 1: Rewrite `Assets/Tests/EditMode/EconomyRulesTests.cs` to the target API (fails compile until Step 3)**

```csharp
using NUnit.Framework;

public class EconomyRulesTests
{
    private ItemDef _moonshine;
    private ItemDef _grain;

    [SetUp]
    public void SetUp()
    {
        _moonshine = new ItemDef("moonshine", "Moonshine", false, 25, true);
        _grain = new ItemDef("grain", "Grain", true, 5);
    }

    [Test]
    public void GetSellPrice_IsBasePrice()
    {
        Assert.AreEqual(25, EconomyRules.GetSellPrice(_moonshine));
        Assert.AreEqual(5, EconomyRules.GetSellPrice(_grain));
    }

    [Test]
    public void GetBuyPrice_EqualsBasePrice()
    {
        Assert.AreEqual(5, EconomyRules.GetBuyPrice(_grain));
        Assert.AreEqual(25, EconomyRules.GetBuyPrice(_moonshine));
    }

    [Test]
    public void IsCartDay_DayModuloThree()
    {
        Assert.IsFalse(EconomyRules.IsCartDay(3));
        Assert.IsFalse(EconomyRules.IsCartDay(6));
        Assert.IsFalse(EconomyRules.IsCartDay(9));
        Assert.IsTrue(EconomyRules.IsCartDay(1));
        Assert.IsTrue(EconomyRules.IsCartDay(2));
        Assert.IsTrue(EconomyRules.IsCartDay(4));
    }

    [Test]
    public void IsSellable_CartBuysBottles()
    {
        Assert.IsTrue(EconomyRules.IsSellable(_moonshine, SellerType.TravelingCart));
        Assert.IsFalse(EconomyRules.IsSellable(_grain, SellerType.TravelingCart));
    }

    [Test]
    public void IsSellable_TormodBuysNonIngredients()
    {
        Assert.IsTrue(EconomyRules.IsSellable(_moonshine, SellerType.Tormod));
        Assert.IsFalse(EconomyRules.IsSellable(_grain, SellerType.Tormod));
    }

    [Test]
    public void GetDeliveryPrice_Backwoods_Is1_5x()
    {
        Assert.AreEqual(38, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Backwoods));
    }

    [Test]
    public void GetDeliveryPrice_Cart_IsBasePrice()
    {
        Assert.AreEqual(25, EconomyRules.GetDeliveryPrice(_moonshine, DeliveryType.Cart));
    }
}
```

- [ ] **Step 2: Delete the confiscation test file**

```powershell
Remove-Item Assets\Tests\EditMode\SellConfiscationTests.cs, Assets\Tests\EditMode\SellConfiscationTests.cs.meta
```

- [ ] **Step 3: Replace the entire content of `Assets/Scripts/Rules/EconomyRules.cs` with:**

```csharp
using UnityEngine;

public static class EconomyRules
{
    public static int GetSellPrice(ItemDef item) => item.basePrice;

    public static int GetBuyPrice(ItemDef item) => item.basePrice;

    public static bool IsCartDay(int day) => day % 3 != 0;

    public static bool IsSellable(ItemDef item, SellerType seller)
    {
        return seller == SellerType.TravelingCart ? item.isBottle : !item.isIngredient;
    }

    public static int GetDeliveryPrice(ItemDef item, DeliveryType type)
    {
        float mult = type == DeliveryType.Backwoods ? 1.5f : 1f;
        return Mathf.RoundToInt(item.basePrice * mult);
    }
}
```

- [ ] **Step 4: Edit `Assets/Scripts/DeliveryPoint.cs` — flat pricing, no suspicion gain**

Replace this block in `Interact()`:

```csharp
        int suspicion = GameManager.Instance != null ? GameManager.Instance.Heat : 0;
        int price = EconomyRules.GetDeliveryPrice(crate.item, deliveryType, suspicion) * crate.count;

        if (price <= 0 && deliveryType == DeliveryType.Cart)
        {
            GameEvents.OnToastRequested("The cart driver won't deal with you — too much heat.");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCash(price);
            GameEvents.OnToastRequested($"+{price}g");

            if (deliveryType == DeliveryType.Backwoods && TimeManager.Instance != null)
            {
                var recipe = FermentManager.Instance != null
                    ? FermentManager.Instance.FindRecipeForItem(crate.item)
                    : null;
                if (recipe != null)
                {
                    int suspicionGain = EconomyRules.GetSuspicionForDrop(recipe, TimeManager.Instance.Hour);
                    if (suspicionGain > 0)
                        GameManager.Instance.AddHeat(suspicionGain);
                }
            }
        }
```

with:

```csharp
        int price = EconomyRules.GetDeliveryPrice(crate.item, deliveryType) * crate.count;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCash(price);
            GameEvents.OnToastRequested($"+{price}g");
        }
```

- [ ] **Step 5: Edit `Assets/Scripts/FermentManager.cs`**

Delete the `FindRecipeForItem` method (at line ~177 — only `DeliveryPoint` called it, and that call is now gone):

```csharp
    public RecipeData FindRecipeForItem(ItemDef item)
```

(delete the whole method body through its closing brace).

Update the four recipe constructions at lines 30–46, dropping the final `suspicionPerDrop` argument:

```csharp
            new RecipeData("Basic Mash", 4, 3, ContentDb.BasicMoonshine)
                .AddIngredient(ContentDb.Grain, 2)
                .AddIngredient(ContentDb.Water, 1)
                .AddIngredient(ContentDb.Yeast, 1),
            new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine, "Bakery")
                .AddIngredient(ContentDb.Grain, 1)
                .AddIngredient(ContentDb.Sugar, 2)
                .AddIngredient(ContentDb.Yeast, 1)
```

(and correspondingly `new RecipeData("Highland Mash", 8, 5, ContentDb.HighlandMoonshine, "Mill")` and `new RecipeData("Aged Reserve", 12, 3, ContentDb.AgedReserve, null, 50)` — keep every `.AddIngredient` line exactly as it is.)

- [ ] **Step 6: Edit `Assets/Scripts/RecipeData.cs`**

Remove the field `public int suspicionPerDrop;` and change the constructor from:

```csharp
    public RecipeData(string recipeName, int fermentationHours, int outputCount, ItemDef outputItem,
        string unlockedByBuildingId = null, int minReputation = 0, int suspicionPerDrop = 5)
```

to:

```csharp
    public RecipeData(string recipeName, int fermentationHours, int outputCount, ItemDef outputItem,
        string unlockedByBuildingId = null, int minReputation = 0)
```

and delete the line `this.suspicionPerDrop = suspicionPerDrop;` from the constructor body.

- [ ] **Step 7: Edit `Assets/Scripts/SellerType.cs`**

```csharp
public enum SellerType
{
    Tormod,
    TravelingCart
}
```

- [ ] **Step 8: Edit `Assets/Scripts/SellerInteractable.cs`**

In `Create`, remove the line `SellerType.RiskyBuyer => new Color(0.9f, 0.2f, 0.2f),` from the color switch.

- [ ] **Step 9: Edit `Assets/Scripts/UI/GameHUD.cs`**

Remove the field `[SerializeField] private TMPro.TextMeshProUGUI heatText;`.
Remove from `Awake`: `if (heatText != null) heatText.text = "Suspicion: 0 (Clean)";`.
Remove from `OnEnable`: `GameEvents.HeatChanged += OnHeatChanged;`.
Remove from `OnDisable`: `GameEvents.HeatChanged -= OnHeatChanged;`.
Delete the entire `OnHeatChanged` method (the one using `EconomyRules.GetSuspicionTier` with the tier-label and color switches).
In `UpdateInteractPrompt`, remove the line `SellerType.RiskyBuyer => "[E] Shady Deal",` from the seller prompt switch.

- [ ] **Step 10: Run EditMode tests**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False`.

- [ ] **Step 11: Commit**

```powershell
git add Assets/Scripts/Rules/EconomyRules.cs Assets/Scripts/DeliveryPoint.cs Assets/Scripts/FermentManager.cs Assets/Scripts/RecipeData.cs Assets/Scripts/SellerType.cs Assets/Scripts/SellerInteractable.cs Assets/Scripts/UI/GameHUD.cs Assets/Tests/EditMode/EconomyRulesTests.cs
git rm Assets/Tests/EditMode/SellConfiscationTests.cs Assets/Tests/EditMode/SellConfiscationTests.cs.meta
git commit -m "Remove suspicion economics, risky buyer, and heat HUD"
```

(If Step 2 already deleted the files from disk, `git rm` reports they're gone — use `git add -A Assets/Tests/EditMode/` instead.)

---

### Task 5: Remove Heat from the state layer (EconomyState, GameManager, GameEvents, DebugMenu)

**Files:**
- Modify: `Assets/Scripts/Rules/EconomyState.cs`
- Modify: `Assets/Scripts/GameManager.cs`
- Modify: `Assets/Scripts/GameEvents.cs`
- Modify: `Assets/Scripts/DebugMenu.cs`
- Test: `Assets/Tests/EditMode/EconomyStateTests.cs`, `Assets/Tests/EditMode/GameEventsTests.cs`, `Assets/Tests/PlayMode/GameManagerEventTests.cs`

**Interfaces:**
- Consumes: Tasks 2–4 removed every production caller of `GameManager.Heat/AddHeat/SetHeat`, `GameEvents.HeatChanged/OnHeatChanged`, `GameEvents.RaidOccurred/OnRaidOccurred`.
- Produces: `EconomyState` = `{ Cash, Reputation, TrySpend, AddCash, SetReputation }`. `GameManager` = `{ Cash, Reputation, TrySpend, AddCash, SetReputation }`. `GameEvents` loses `HeatChanged`, `RaidOccurred` and their invokers. **Reputation survives intact** (dialogue tiers + recipe gates until the narrative plan).

- [ ] **Step 1: Update the tests to the target API (fail compile until Steps 3–5)**

In `Assets/Tests/EditMode/EconomyStateTests.cs`, delete these four tests entirely: `SetHeat_ClampsAtZero`, `AddHeat_NegativeClampsAtZero`, `SetHeat_ReturnsOldValue`, `SetHeat_NoChangeReturnsSameOld`. Keep everything else (including `SetReputation_ReturnsOld`).

In `Assets/Tests/PlayMode/GameManagerEventTests.cs`: delete the `SetUp` line `GameEvents.HeatChanged += (newHeat, oldHeat) => _recorder.Record("HeatChanged", $"{newHeat}/{oldHeat}");` and delete these three tests entirely: `AddHeat_FiresHeatChanged`, `SetHeat_NoChange_DoesNotFireEvent`, `AddHeat_Negative_ClampsAtZero`.

In `Assets/Tests/EditMode/GameEventsTests.cs`, replace the `MultipleSubscribers_AllFire` test body's heat usage:

```csharp
    [Test]
    public void MultipleSubscribers_AllFire()
    {
        int a = 0, b = 0;
        GameEvents.RepChanged += (newRep, oldRep) => a++;
        GameEvents.RepChanged += (newRep, oldRep) => b++;

        GameEvents.OnRepChanged(50, 30);

        Assert.AreEqual(1, a);
        Assert.AreEqual(1, b);
    }
```

- [ ] **Step 2: Edit `Assets/Scripts/Rules/EconomyState.cs`**

Remove the property `public int Heat { get; private set; }` and delete the `SetHeat` and `AddHeat` methods. The `using System;` directive becomes unused (it was for `Math.Max`) — remove it. Resulting file:

```csharp
public sealed class EconomyState
{
    public int Cash { get; private set; }
    public int Reputation { get; private set; }

    public EconomyState(int startingCash)
    {
        Cash = startingCash;
    }

    public bool TrySpend(int amount)
    {
        if (Cash < amount) return false;
        Cash -= amount;
        return true;
    }

    public void AddCash(int amount)
    {
        Cash += amount;
    }

    public int SetReputation(int value)
    {
        int old = Reputation;
        Reputation = value;
        return old;
    }
}
```

- [ ] **Step 3: Edit `Assets/Scripts/GameManager.cs`**

Remove the property `public int Heat => Economy.Heat;` and delete both methods `AddHeat(int delta)` and `SetHeat(int value)`.

- [ ] **Step 4: Edit `Assets/Scripts/GameEvents.cs`**

Remove the declarations:

```csharp
    public static event System.Action<int, int> HeatChanged;
```

```csharp
    public static event System.Action<int, int> RaidOccurred;
```

and their invokers:

```csharp
    public static void OnHeatChanged(int newHeat, int oldHeat)
        => HeatChanged?.Invoke(newHeat, oldHeat);
```

```csharp
    public static void OnRaidOccurred(int cratesLost, int fine)
        => RaidOccurred?.Invoke(cratesLost, fine);
```

(Keep `CaughtBribe`, `BribePaid`, `BribeRefused` — the bribe flow lives on.)

- [ ] **Step 5: Edit `Assets/Scripts/DebugMenu.cs`**

Remove the label line `GUILayout.Label($"Heat: {GameManager.Instance.Heat}");` and both heat buttons:

```csharp
        if (GUILayout.Button("Heat +10"))
            GameManager.Instance.SetHeat(GameManager.Instance.Heat + 10);
```

```csharp
        if (GUILayout.Button("Reset Heat"))
            GameManager.Instance.SetHeat(0);
```

- [ ] **Step 6: Prove the purge is total**

Run: `Get-ChildItem Assets\Scripts -Recurse -Filter *.cs | Select-String -Pattern "Heat|Suspicion|Raid" | Measure-Object | Select-Object -ExpandProperty Count`
Expected: `0`. If anything appears, it's a missed reference — fix it before proceeding.

- [ ] **Step 7: Run both test suites**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\TestResults\editmode.log" | Out-Null
Select-String -Path .\TestResults\editmode.xml -Pattern 'result="Failed"' -Quiet
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\TestResults\playmode.log" | Out-Null
Select-String -Path .\TestResults\playmode.xml -Pattern 'result="Failed"' -Quiet
```

Expected: `False` twice.

- [ ] **Step 8: Commit**

```powershell
git add Assets/Scripts/Rules/EconomyState.cs Assets/Scripts/GameManager.cs Assets/Scripts/GameEvents.cs Assets/Scripts/DebugMenu.cs Assets/Tests/EditMode/EconomyStateTests.cs Assets/Tests/EditMode/GameEventsTests.cs Assets/Tests/PlayMode/GameManagerEventTests.cs
git commit -m "Remove heat from state layer and debug menu"
```

---

### Task 6: Scene cleanup and end-to-end verification

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity editor, not by hand)

**Interfaces:**
- Consumes: everything above.
- Produces: a playable, heat-free build; the demolition checklist in `Assets/Docs/BuildPlan.md` Phase D can be ticked.

- [ ] **Step 1: Scene cleanup in the Unity editor (manual — scene files are not hand-edited)**

Open the project in Unity. In `SampleScene`:
1. Delete the orphaned `RiskyBuyerPosition` GameObject (nothing references it anymore).
2. Find the HUD's heat/suspicion `TextMeshProUGUI` GameObject (it was wired to `GameHUD.heatText`, showing "Suspicion: 0 (Clean)") and delete it.
3. Save the scene.

- [ ] **Step 2: Confirm no console errors on load**

Enter Play mode once; the console must show no red errors (a missing-reference error here means a serialized field still points at a deleted object).

- [ ] **Step 3: Manual play verification (10 minutes, in Play mode)**

1. HUD shows cash/day/clock/rep — no suspicion line.
2. Sleep through a night: no raid toast, day advances, autosave step unchanged.
3. With the debug menu (P): no Heat label or buttons; grant cash, buy a building — all working.
4. Pick up a crate, walk into the guard's vision cone until caught: bribe window says "Pay Ng to keep your cargo? Refuse and lose it."
   - Pay → cargo still in hands, cash reduced, guard looks away.
   - Get caught again, refuse → crate destroyed, "Moonshine confiscated!" toast, guard looks away.
5. Deliver a crate to the backwoods point: flat 1.5× price, no heat side effects.

- [ ] **Step 4: Tick Phase D checkboxes in `Assets/Docs/BuildPlan.md`** (all five items, including "Done: compiles, all tests green, no reference to Heat anywhere in Assets/Scripts").

- [ ] **Step 5: Final commit**

```powershell
git add Assets/Scenes/SampleScene.unity Assets/Docs/BuildPlan.md
git commit -m "Remove risky buyer marker and heat HUD element from scene"
```

---

## Self-Review Notes

- **Spec coverage:** this plan implements only the spec's "What dies" section plus the BuildPlan.md roadmap; the spec's new systems are explicitly deferred to later plans (listed in Global Constraints). Reputation removal is deliberately deferred to the narrative plan — the spec's "global reputation dies" lands there, documented in BuildPlan Phase 5.
- **Compile-order:** Task 2 unreferences raid constants → Task 3 unreferences guard scaling + `AddHeat` in Guard → Task 4 deletes the rules + last callers → Task 5 deletes state/events. Each task compiles and tests green independently.
- **Type consistency:** `GetSellPrice(ItemDef)`, `GetDeliveryPrice(ItemDef, DeliveryType)`, `RecipeData(...6 params)`, `SellerType.{Tormod, TravelingCart}` — used consistently across Tasks 4–5 and the rewritten tests.
