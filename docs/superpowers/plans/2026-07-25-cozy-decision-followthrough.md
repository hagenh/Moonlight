# Cozy Decision Follow-Through Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the repository match the design decisions settled on 2026-07-25 — reconcile `BuildPlan.md` against `GameDesign.md`, and delete the guard/bribe system that the cozy decision orphaned for good.

**Architecture:** Two independent workstreams. One is pure documentation (`BuildPlan.md`). The other is a deletion: `Guard`, `GuardManager`, `BribeUI`, their scene objects, their prefab, and the three bribe events on `GameEvents`. Nothing is being built — every code change removes something, so the verification bar is "still compiles, all remaining tests green."

**Tech Stack:** Unity 6 (6000.2.14f1), URP 17.2.0, C#, NUnit via Unity Test Framework (`Lamplight.EditModeTests`, `Lamplight.PlayModeTests`).

## Global Constraints

- **No comments in code** unless explicitly requested (`AGENTS.md`).
- Unity requires that every deleted asset's `.meta` file be deleted alongside it. A `.cs` deleted without its `.cs.meta` leaves the project dirty.
- **Never** call another manager's methods directly for cross-system communication — use `GameEvents`. (Not exercised by this plan, which only removes events, but it governs any incidental edit.)
- `Assets/Scripts/Rules/` must stay pure C# — no `UnityEngine` types except `Mathf`. (Not touched by this plan.)
- Design guardrails now in force, from `Assets/Docs/GameDesign.md`: no loss anywhere at any hour · no hidden dice · Act 0 is 20-40 min · appointments recur · beautification never punished · restoration doubles as defense *in the fiction* · **cozy is the genre, not a fallback**.
- `docs/superpowers/` is untracked in this repository by convention. Do **not** `git add` anything under it.

## Context: what is already done

Audit item 12 (`tormodLeaveHour = -1`) **was fixed before this plan was written** and is currently uncommitted in the working tree: `SellManager` now uses `SellerRules.IsPresent` for an 18:00-06:00 window, with `Assets/Tests/EditMode/SellerRulesTests.cs` covering it. `GameDesign.md` has been corrected to say so. **Do not re-fix it.** Task 1 only needs to give `BuildPlan.md` a line item acknowledging it.

## File Structure

| File | Fate | Responsibility after this plan |
|---|---|---|
| `Assets/Docs/BuildPlan.md` | Modify | Build order matching `GameDesign.md` |
| `Assets/Scripts/Guard.cs` | **Delete** | — |
| `Assets/Scripts/GuardManager.cs` | **Delete** | — |
| `Assets/Scripts/UI/BribeUI.cs` | **Delete** | — |
| `Assets/Tests/EditMode/GuardAnimationTests.cs` | **Delete** | — |
| `Assets/Prefabs/Guard.prefab` | **Delete** | — (untracked; the *sprite* under `Assets/Sprite/` is kept for Constable Aas) |
| `Assets/Scripts/GameEvents.cs` | Modify | Event bus, minus `CaughtBribe` / `BribePaid` / `BribeRefused` |
| `Assets/Scenes/SampleScene.unity` | Modify (Unity editor) | Scene, minus Guard instances and the GuardManager object |

**Not touched, despite matching a `Guard`/`Bribe`/carry grep:** `PlayerController.IsCarryingCrate`, `CarryState`, `InteractState`, `DeliveryPoint`, `SellerInteractable`, `InteriorManager`. Crate-hauling serves *deliveries*, not guards, and survives the cozy decision intact.

---

### Task 1: Reconcile `BuildPlan.md` with `GameDesign.md`

Documentation only — no code, no tests. Verification is a read-through against the audit.

**Files:**
- Modify: `Assets/Docs/BuildPlan.md`
- Reference (read, do not modify): `Assets/Docs/GameDesign.md` Part 4, "`BuildPlan.md` reconciliation — the audit"

**Interfaces:**
- Consumes: nothing.
- Produces: nothing consumed by later tasks. Task 1 and Tasks 2-4 are independent and may be done in either order.

- [ ] **Step 1: Read the audit**

Read `Assets/Docs/GameDesign.md`, Part 4, section "`BuildPlan.md` reconciliation — the audit". It lists twelve numbered problems. Item 12 is already closed (see "Context" above). The remaining eleven are the work.

- [ ] **Step 2: Fix the header (audit item 1)**

`Assets/Docs/BuildPlan.md` line 3 currently reads:

```
Spec: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md — read it before any phase.
```

Replace with:

```
Design: Assets/Docs/GameDesign.md — the master design document. Read it before any phase.
Superseded: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md (kept as the dated record of what was approved on 2026-07-22; where the two disagree, GameDesign.md wins).
```

- [ ] **Step 3: Rewrite the slice summary (audit item 2)**

Replace the `## Slice contents` bullets for `Fantasy`, the three-depths line, and `Systems` with text matching `GameDesign.md`. Specifically:

- Fantasy line: strike "night = opt-in delivery runs" and "You light the town and keep the woods dark." Use the current design statement, *"you light the town, and the town covers for you,"* and the current day/night line, *"day is when you act; night is when the day answers back."*
- Map line: strike "deep woods (routes, destinations)". Deep woods currently have no justification — say so explicitly rather than listing them as planned content.
- Systems line: strike "delivery runs (routes, patrols, load-outs)" and "two-layer infrastructure" (now one layer, public sockets only). Upgrade "stand (safe channel)" to "roadside stand + request book (the primary economy)". Add "night beats".

- [ ] **Step 4: Delete Phase 4 entirely (audit item 3)**

Remove the whole `## Phase 4 — Delivery runs` section. Replace it with a one-line tombstone so the phase numbering below it still makes sense to a reader:

```
## Phase 4 — Delivery runs (CUT 2026-07-25)
- Cut entirely. See GameDesign.md Part 4, "The runs decision". Phase number retired, not reused.
```

- [ ] **Step 5: Halve Phases 3 and 5 (audit item 4)**

In `## Phase 3`, strike the route corridors and the three destination sites; keep near forest and camp. Rescope the darkness pass — it is now **unblocked** but must serve the new night design, so it reads as the homestead at night plus the lit town seen from the treeline, not dark woods to sneak through. Strike the "destinations 60-90 s" walk timing.

In `## Phase 5`, strike the entire covert-sockets bullet (stash barrel, trail marker, shortcut plank, lookout perch). Keep public sockets untouched. Retitle the phase from "Two-layer infrastructure" to "Public infrastructure".

- [ ] **Step 6: Add the two missing phases (audit item 5)**

Add a phase for the roadside stand and request book, drawn from `GameDesign.md` Part 3, "The stand and the request book". Add a second phase for night beats and Constable appearances, drawn from Part 4 threads #4 and #3 — and mark the beat-frequency and trigger-mechanism items as **open numbers deferred to design**, because they genuinely are; do not invent values.

- [ ] **Step 7: Fix the broken metrics (audit item 6)**

- Phase 7 `Numbers pass`: strike "first night run ~1 h".
- Phase 8: strike "night-run ambience layer"; replace with a night-beat ambience cue.
- Phase 9 `Collect`: strike "time to first night run (<75 min)" and "caught-players can name their mistake (legibility check — if not, patrol/telegraphing bug)". Two of six validation metrics measured a system that will not exist. Replace with metrics that match the design: time to first stand sale, and whether the player notices the request book's customer mix shifting.

- [ ] **Step 8: Fix the orphaned and dead-function items (audit items 7, 8, 9)**

- Phase D: annotate the bribe-rework and guards lines as superseded — nothing catches the player any more, and the guards are deleted in Tasks 2-4 of this plan.
- Phase 6: Berta's recruitment beat currently reads "catches you, covers unprompted", which was built on smuggling. Mark her trigger as **needing a non-jeopardy replacement** (`GameDesign.md` thread #8). Do not invent the replacement here.
- Phase 7: strike the handcart from Smithy & Cooperage, and the courier from Boarding House. Mark the Boarding House operation role as **needing redesign**.

- [ ] **Step 9: Fix the pre-existing reputation contradiction (audit item 10)**

Phase D says reputation dies in Phase 5; Phase 6 says it dies there. **Phase 6 is correct.** Edit the Phase D line to say Phase 6.

- [ ] **Step 10: Teach Phase 1 about shell-vs-site, and record the Tormod fix (audit items 11, 12)**

Phase 1's homestead bullet reads as a one-time build unlocking "proper still + vat". Rewrite it as the **shell** — three stages closing Act 0 — with everything after (stand, vats, storage, rooms) being ongoing site growth per `GameDesign.md` Part 3, "The homestead is a site, not a purchase."

Add a completed line item to Phase 1 recording the Tormod fix:

```
- [x] Tormod keeps dusk-to-dawn hours (18:00–06:00) via SellerRules.IsPresent; he is the Act 0 buyer, not a permanent shopfront.
```

- [ ] **Step 11: Update the guardrails line**

`## Rules`, last-but-one bullet, currently lists five guardrails from the old spec. Replace with the seven now in `GameDesign.md` Part 3, including the new **"cozy is the genre, not a fallback."**

- [ ] **Step 12: Verify by read-through**

Re-read the twelve audit items in `GameDesign.md` against your edited `BuildPlan.md`. Every item except 12 should now be addressed; item 12 should have its Phase 1 line. There is no automated check for this — read it.

Expected: no remaining reference in `BuildPlan.md` to delivery runs, routes, patrols, load-outs, couriers, handcarts, covert sockets, stash barrels, trail markers, shortcut planks, lookout perches, heat, or being caught.

- [ ] **Step 13: Commit**

```bash
git add Assets/Docs/BuildPlan.md
git commit -m "Reconcile BuildPlan with GameDesign after the cozy decision

Phase 4 dies entirely, Phases 3 and 5 halve, and the header stops pointing
at the superseded spec. Adds phases for the stand and for night beats, fixes
four validation metrics that measured delivery runs, and corrects the
reputation-death phase. Records Tormod's dusk-to-dawn fix against Phase 1."
```

---

### Task 2: Remove guard objects from the scene

**This task runs in the Unity editor, by hand.** `SampleScene.unity` contains 17 `Guard` references across GameObjects, components, and waypoint transforms. Hand-editing that YAML is how you corrupt a scene — do it in the editor, where reference cleanup is automatic.

**It must run before Task 3.** Deleting the scripts first leaves every one of those GameObjects showing "missing script", and Unity cannot then tell you what they were.

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via the Unity editor)

**Interfaces:**
- Consumes: nothing.
- Produces: a scene with no `Guard` or `GuardManager` components, which Task 3 depends on.

- [ ] **Step 1: Open the scene**

Open `Assets/Scenes/SampleScene.unity` in the Unity editor.

- [ ] **Step 2: Delete the guard GameObjects**

In the Hierarchy, delete every GameObject with a `Guard` component, every waypoint transform those guards referenced (they are typically children or siblings named `WP*` / `Waypoint*` and have no other purpose), and the GameObject holding `GuardManager`.

If a `BribeUI` component exists on a UI GameObject in the scene, delete that too.

- [ ] **Step 3: Save and verify**

Save the scene. Then confirm the references are gone:

```bash
grep -c "Guard" Assets/Scenes/SampleScene.unity
```

Expected: `0`. If it is non-zero, inspect what is left — a leftover is usually a waypoint transform still named "Guard…".

- [ ] **Step 4: Commit**

```bash
git add Assets/Scenes/SampleScene.unity
git commit -m "Remove guard objects and GuardManager from SampleScene

Scene-side half of deleting the patrol system. Scripts follow separately so
the scene never passes through a missing-script state."
```

---

### Task 3: Delete the guard and bribe code

**Depends on Task 2.** All deletions land together because `BribeUI` subscribes to the three bribe events — removing the events without removing `BribeUI` breaks the compile.

**Files:**
- Delete: `Assets/Scripts/Guard.cs` and `Assets/Scripts/Guard.cs.meta`
- Delete: `Assets/Scripts/GuardManager.cs` and `Assets/Scripts/GuardManager.cs.meta`
- Delete: `Assets/Scripts/UI/BribeUI.cs` and `Assets/Scripts/UI/BribeUI.cs.meta`
- Delete: `Assets/Tests/EditMode/GuardAnimationTests.cs` and `Assets/Tests/EditMode/GuardAnimationTests.cs.meta`
- Delete: `Assets/Prefabs/Guard.prefab` and `Assets/Prefabs/Guard.prefab.meta` (untracked — remove from disk, nothing to stage)
- Modify: `Assets/Scripts/GameEvents.cs:41-43` and `Assets/Scripts/GameEvents.cs:147-154`

**Interfaces:**
- Consumes: a scene with no guard references (Task 2).
- Produces: a `GameEvents` class with no `CaughtBribe`, `BribePaid`, or `BribeRefused` members. Nothing else in the codebase references them after this task.

- [ ] **Step 1: Confirm nothing else references the bribe events**

```bash
grep -rn "CaughtBribe\|BribePaid\|BribeRefused\|GuardManager\|\bGuard\b" Assets/Scripts Assets/Tests --include=*.cs
```

Expected: matches only in `Guard.cs`, `GuardManager.cs`, `UI/BribeUI.cs`, `GameEvents.cs`, `GuardAnimationTests.cs`, and one false positive — the comment `// Guards against a window that silently swallows an hour.` in `SellerRulesTests.cs`, which stays.

If anything else appears, stop and report it rather than deleting.

- [ ] **Step 2: Note what the deleted test actually covered**

`GuardAnimationTests.cs` has exactly two assertions — that the GameObject has a `DirectionalSpriteAnimator`, and that its `animationSet` is assigned. Both are already covered by `Assets/Tests/EditMode/DirectionalSpriteAnimatorTests.cs` and `DirectionalAnimationSetTests.cs`, which test the animator directly rather than through a Guard. **No coverage is lost by deleting it.** Confirm this by reading those two files before proceeding.

- [ ] **Step 3: Delete the files**

```bash
git rm Assets/Scripts/Guard.cs Assets/Scripts/Guard.cs.meta \
       Assets/Scripts/GuardManager.cs Assets/Scripts/GuardManager.cs.meta \
       Assets/Scripts/UI/BribeUI.cs Assets/Scripts/UI/BribeUI.cs.meta \
       Assets/Tests/EditMode/GuardAnimationTests.cs Assets/Tests/EditMode/GuardAnimationTests.cs.meta
rm -f Assets/Prefabs/Guard.prefab Assets/Prefabs/Guard.prefab.meta
```

- [ ] **Step 4: Remove the event declarations**

In `Assets/Scripts/GameEvents.cs`, delete these three lines (currently 41-43):

```csharp
    public static event System.Action<int> CaughtBribe;
    public static event System.Action BribePaid;
    public static event System.Action BribeRefused;
```

The `DeliveryMade` declaration immediately above them stays.

- [ ] **Step 5: Remove the invoker methods**

In the same file, delete these (currently 147-154):

```csharp
    public static void OnCaughtBribe(int cost)
        => CaughtBribe?.Invoke(cost);

    public static void OnBribePaid()
        => BribePaid?.Invoke();

    public static void OnBribeRefused()
        => BribeRefused?.Invoke();
```

`OnDeliveryMade` immediately above them stays, and the class closing brace stays.

- [ ] **Step 6: Confirm the test support layer needs no change**

`Assets/Tests/Shared/GameEventsReset.cs` clears event delegates by **reflecting over every static field on `GameEvents`** — it names no individual event. Removing three events therefore requires no edit there, and nothing in `Assets/Tests/` references the bribe events at all.

Verify rather than assume:

```bash
grep -rn "Bribe" Assets/Tests/ --include=*.cs
```

Expected: no matches. If there are any, remove them before compiling.

- [ ] **Step 7: Compile and run the full test suite**

Let Unity reimport, then run both assemblies:

```bash
Unity.exe -runTests -testPlatform EditMode -testResults editmode-results.xml -projectPath . -batchmode -quit
Unity.exe -runTests -testPlatform PlayMode -testResults playmode-results.xml -projectPath . -batchmode -quit
```

Expected: zero compile errors, and every remaining test green. `GuardAnimationTests` should be absent from the results, not failing.

If the Unity CLI is unavailable in this environment, open the project and use Window → General → Test Runner, running both EditMode and PlayMode. **Do not mark this step done on a compile check alone** — the deletion touches the shared event bus, and the test assemblies are what prove nothing else was leaning on it.

- [ ] **Step 8: Commit**

```bash
git add -A Assets/Scripts/GameEvents.cs Assets/Tests
git commit -m "Delete the guard and bribe system

Orphaned by the runs cut and finally by the cozy decision: nothing in the
design will ever need patrol or detection code. Removes Guard, GuardManager,
BribeUI, the three bribe events on GameEvents, and GuardAnimationTests, whose
two assertions are already covered by DirectionalSpriteAnimatorTests.

The Guard sprite is kept — Constable Aas uses it."
```

---

### Task 4: Close out the audit in `GameDesign.md`

**Depends on Tasks 1-3.** Records that the reconciliation and the deletion actually happened, so the next reader is not told to redo them.

**Files:**
- Modify: `Assets/Docs/GameDesign.md` — Part 4, the audit section and "Smaller open items"

**Interfaces:**
- Consumes: a reconciled `BuildPlan.md` (Task 1) and a deleted guard system (Task 3).
- Produces: nothing.

- [ ] **Step 1: Mark the audit resolved**

In Part 4, "`BuildPlan.md` reconciliation — the audit", add a line directly beneath the twelve-row table:

```markdown
> **Resolved 2026-07-25.** All twelve items are addressed — `BuildPlan.md` has been reconciled and the guard system deleted. This table is kept as the record of what was wrong, not as a work queue.
```

- [ ] **Step 2: Update the "Warning" in the document header**

The header currently reads: *"**Warning: BuildPlan Phases 3-5 now contradict this document.** See Part 4."* That is no longer true. Replace with a plain pointer to `BuildPlan.md` as the build-order document, with no warning.

- [ ] **Step 3: Update the runs-decision table and smaller open items**

In "The runs decision", change the `Guard.cs` / `GuardManager` row from **Delete** to **Deleted 2026-07-25**.

In "Smaller open items", change the `Guard.cs` / `GuardManager` bullet the same way. The Boarding House and deep-woods bullets stay open.

- [ ] **Step 4: Verify no stale claims remain**

```bash
grep -n "contradict\|Guard.cs\|GuardManager\|not deleted yet" Assets/Docs/GameDesign.md
```

Expected: every remaining hit describes the deletion in the past tense. No hit should tell a reader to do something already done.

- [ ] **Step 5: Commit**

```bash
git add Assets/Docs/GameDesign.md
git commit -m "Close out the BuildPlan audit and the guard deletion

All twelve audit items are addressed. The header no longer warns that
BuildPlan contradicts this document, and the guard scripts are recorded as
deleted rather than pending."
```

---

## Self-Review

**Spec coverage.** The spec's "Follow-on work unblocked" table lists six items. Thread #3 (Constable writing) and thread #5 (side activities) are design work, explicitly out of scope. Audit item 5 (missing phases) → Task 1 Step 6. Audit item 7 (delete guards) → Tasks 2-3. Phase 3 darkness rescope → Task 1 Step 5. Deep woods → left open deliberately; the spec flags it as more pressing but does not decide it, and inventing a decision here would exceed the approved design.

**Placeholder scan.** Task 1 Steps 6 and 8 deliberately instruct the implementer *not* to invent beat frequencies or Berta's replacement trigger, and to mark them open. That is a real design deferral recorded in the spec, not a plan gap.

**Type consistency.** The only signatures touched are the three `GameEvents` members, named identically in Task 3 Steps 1, 4, 5, and 6.

**Known risk.** Task 2 is a manual Unity-editor step and cannot be executed by an agent. If the executing worker has no editor access, it must stop and hand back rather than attempt the scene YAML — that is called out in the task header.
