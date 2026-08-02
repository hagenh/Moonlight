# Pre-Building-Stage Visual Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the flat tinted placeholder square that every unbuilt/unrestored building shows with the hand-built "staked-out construction site" fence-outline visual, for the Homestead's `BuildSign` (Site stage only) and the shared `Building.prefab` used by all six town buildings (Abandoned state only).

**Architecture:** Turn the scene-authored sprite cluster (`Building_PreBuilingStage`, 21 tiles) into a reusable prefab, matching the existing "many 1×1 sprite-tile children under one parent" pattern already used for real building art (`Building_Sprites`). Wire it into two places: as an independent sibling GameObject the Homestead's `BuildSign` toggles by stage, and as a new child inside `Building.prefab` that `Building.cs` toggles by state — the latter propagates to all six town buildings automatically since they share one prefab.

**Tech Stack:** Unity 6 (6000.2.14f1), URP 17.2.0, C#, NUnit via Unity Test Framework. This plan is executed with Unity MCP tools available (`Unity_RunCommand` for editor scripting, `Unity_ManageMenuItem` for the project's test-runner bridge, `Unity_SceneView_Capture2DScene` for visual verification) — it is not blocked on manual editor steps the way older plans in this repo were.

## Global Constraints

- **No comments in code** unless explicitly requested (`AGENTS.md`).
- No new `Rules/` extraction: both toggles added here are one-line `SetActive` calls keyed off an existing enum, matching how `BuildSign`'s existing `_stageColors`/`_stageScales` indexing is already inline rather than pulled into `Rules/`. No new EditMode tests are added — see the spec's Testing section.
- `docs/superpowers/` is untracked by convention. Do **not** `git add` anything under it.
- Unity must be **closed** for `-batchmode` CLI test runs — this plan instead uses the project's own `Claude/Run EditMode Tests` and `Claude/Run PlayMode Tests` menu items (`Assets/ClaudeEditorTools/Editor/ClaudeTestRunner.cs`), which work with the editor open via MCP. They write `RUNNING` then a final `passed=X failed=Y skipped=Z inconclusive=W` line to `.superpowers/sdd/2026-07-26-recipe-book-ui/testresult.txt` (the path is a leftover name from an earlier feature; the runner itself is generic and reused as-is).
- Baseline before starting: EditMode 195 passing, PlayMode 67 passing (confirmed this session before this plan was written).
- Spec: `docs/superpowers/specs/2026-07-28-prebuilding-stage-visual-design.md`.

---

### Task 1: Convert the hand-built cluster into a prefab and reposition it as the Homestead's site visual

**Files:**
- Create: `Assets/Prefabs/BuildingPreBuildingStage.prefab` (from the existing scene GameObject)
- Modify: `Assets/Scenes/SampleScene.unity` (via editor script — rename, reposition, connect to new prefab)

**Interfaces:**
- Consumes: the existing scene GameObject `Building_PreBuilingStage` (world position (13, 0), 21 sprite-tile children, no colliders — see spec for full layout).
- Produces: `Assets/Prefabs/BuildingPreBuildingStage.prefab`, plus a scene instance renamed to `BuildingPreBuildingStage` and moved to world (-23, 12) — this places its bottom-left tile (local (0, -5), `Grassland Spring@128x128_105`) exactly on `BuildSign`'s existing world position (-23, 7). Task 2 wires this instance to `BuildSign`.

- [ ] **Step 1: Confirm the cluster is where the spec says it is**

Run via `Unity_RunCommand`:

```csharp
using UnityEngine;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var cluster = GameObject.Find("Building_PreBuilingStage");
        if (cluster == null)
        {
            result.LogError("Building_PreBuilingStage not found in the open scene");
            return;
        }
        result.Log("Found at {0}, {1} children", cluster.transform.position, cluster.transform.childCount);
    }
}
```

Expected: found at (13, 0, 0), 21 children. If not found, stop — the scene has changed since the spec was written, and the rest of this plan's coordinates need re-deriving before continuing.

- [ ] **Step 2: Rename, reposition, and save as a prefab**

Run via `Unity_RunCommand`:

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var cluster = GameObject.Find("Building_PreBuilingStage");
        result.RegisterObjectModification(cluster);

        cluster.name = "BuildingPreBuildingStage";
        cluster.transform.position = new Vector3(-23f, 12f, 0f);

        string prefabPath = "Assets/Prefabs/BuildingPreBuildingStage.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(cluster, prefabPath, InteractionMode.AutomatedAction);
        result.RegisterObjectCreation(prefab);
        result.Log("Saved {0}, scene instance now at {1}", prefabPath, cluster.transform.position);
    }
}
```

`SaveAsPrefabAssetAndConnect` both creates the prefab asset from the current scene object and turns that scene object into a connected `PrefabInstance` in place — no separate drag-to-folder step needed.

- [ ] **Step 3: Verify the corner lands exactly on BuildSign**

Run `Unity_SceneView_Capture2DScene` with `worldX: -23, worldY: 7, worldWidth: 3, worldHeight: 3, pixelsPerUnit: 150`.

Expected: the fence's bottom-left corner tile is visible centered in frame, at the same point `BuildSign`'s small procedural sprite currently occupies. If the corner looks offset, the position math in Step 2 is wrong relative to `BuildSign`'s actual live position — re-run Step 1's style of query on `BuildSign` itself (`Object.FindFirstObjectByType<BuildSign>().transform.position`) before adjusting.

- [ ] **Step 4: Commit**

```bash
git add "Assets/Prefabs/BuildingPreBuildingStage.prefab" "Assets/Prefabs/BuildingPreBuildingStage.prefab.meta" Assets/Scenes/SampleScene.unity
git commit -m "Add the BuildingPreBuildingStage prefab

The hand-built construction-site fence outline, previously a scratch cluster
sitting disconnected from either build system, is now a reusable prefab. The
scene instance is renamed (typo fixed) and repositioned so its bottom-left
corner lands exactly on BuildSign's existing world position."
```

---

### Task 2: Wire BuildSign.cs to the new site visual

**Depends on Task 1.**

**Files:**
- Modify: `Assets/Scripts/BuildSign.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (via editor script — wire the serialized field)

**Interfaces:**
- Consumes: `BuildingPreBuildingStage` scene instance from Task 1.
- Produces: `BuildSign.siteVisual` (private `[SerializeField] GameObject`), active only while `Stage == BuildStage.Site`; the existing procedural `SpriteRenderer` stops rendering during `Site` and resumes for Foundation/Frame/Walls exactly as before.

- [ ] **Step 1: Add the field and the toggle**

In `Assets/Scripts/BuildSign.cs`, change:

```csharp
    [SerializeField] internal Building homesteadBuilding;

    private SpriteRenderer _spriteRenderer;
```

to:

```csharp
    [SerializeField] internal Building homesteadBuilding;
    [SerializeField] private GameObject siteVisual;

    private SpriteRenderer _spriteRenderer;
```

Change:

```csharp
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
    }
```

to:

```csharp
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
        RefreshSiteVisual();
    }
```

Change:

```csharp
    private void AdvanceStage(BuildStage newStage, string toast)
    {
        _stage = newStage;
        if (_spriteRenderer != null && (int)_stage < _stageColors.Length)
            _spriteRenderer.color = _stageColors[(int)_stage];
        if ((int)_stage < _stageScales.Length)
        {
            float s = _stageScales[(int)_stage];
            transform.localScale = new Vector3(s, s, 1f);
            foreach (var c in GetComponents<BoxCollider2D>())
                c.size = new Vector2(0.8f, 1.0f);
        }
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }
```

to:

```csharp
    private void AdvanceStage(BuildStage newStage, string toast)
    {
        _stage = newStage;
        if (_spriteRenderer != null && (int)_stage < _stageColors.Length)
            _spriteRenderer.color = _stageColors[(int)_stage];
        if ((int)_stage < _stageScales.Length)
        {
            float s = _stageScales[(int)_stage];
            transform.localScale = new Vector3(s, s, 1f);
            foreach (var c in GetComponents<BoxCollider2D>())
                c.size = new Vector2(0.8f, 1.0f);
        }
        RefreshSiteVisual();
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void RefreshSiteVisual()
    {
        bool showSiteVisual = _stage == BuildStage.Site;
        if (siteVisual != null)
            siteVisual.SetActive(showSiteVisual);
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !showSiteVisual;
    }
```

Both new `if` guards match the existing null-check style used throughout this file (`_spriteRenderer != null`), which matters because `Assets/Tests/EditMode/BuildSignTests.cs` builds `BuildSign` via `AddComponent` with no fields wired at all — `siteVisual` will be null there, and the guard is what keeps those tests passing unchanged.

- [ ] **Step 2: Wire the scene reference**

Run via `Unity_RunCommand`:

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        var sign = Object.FindFirstObjectByType<BuildSign>();
        var visual = GameObject.Find("BuildingPreBuildingStage");
        if (sign == null || visual == null)
        {
            result.LogError("BuildSign or BuildingPreBuildingStage not found");
            return;
        }

        result.RegisterObjectModification(sign.gameObject);
        var so = new SerializedObject(sign);
        so.FindProperty("siteVisual").objectReferenceValue = visual;
        so.ApplyModifiedProperties();
        result.Log("Wired siteVisual on {0}", sign);
    }
}
```

- [ ] **Step 3: Run both test suites**

Run `Unity_ManageMenuItem` with `Action: Execute, MenuPath: "Claude/Run EditMode Tests"`, then poll `.superpowers/sdd/2026-07-26-recipe-book-ui/testresult.txt` until it stops reading `RUNNING` (check every few seconds; EditMode typically finishes in well under a minute). Repeat with `"Claude/Run PlayMode Tests"`.

Expected: `passed=195 failed=0` for EditMode, `passed=67 failed=0` for PlayMode — identical to the baseline in Global Constraints. This task adds no new tests, so nothing should move.

- [ ] **Step 4: Playtest verification**

Enter Play mode. At game start (`BuildStage.Site`), confirm: the fence-outline construction-site border is visible at the Homestead's camp clearing, the old tan placeholder square is not, and the interact prompt still triggers at the same spot it did before (walking up to where `BuildSign`'s collider sits, i.e. the fence's bottom-left corner). Forage 3 Stone and interact to advance to `Foundation`; confirm the fence-outline disappears and the old tint-and-scale placeholder appears exactly as it did before this change.

**Do not mark this step done without actually entering Play mode** — this is the one behavior no automated test covers.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/BuildSign.cs Assets/Scenes/SampleScene.unity
git commit -m "Show the construction-site fence outline for BuildStage.Site

BuildSign's own procedural sprite now hides while the new siteVisual is
active, and resumes its tint-and-scale placeholder from Foundation onward
exactly as before. The interaction point (BuildSign's own collider) is
untouched — only the art moved to meet it."
```

---

### Task 3: Delete the stray debris object

**Independent of Tasks 1-2** — can run any time, grouped here for narrative order.

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via editor script)

**Interfaces:**
- Consumes: nothing.
- Produces: one fewer stray GameObject in the scene. Confirmed with the user this is unused debris (no sprite, no script) unrelated to either build system.

- [ ] **Step 1: Confirm there's exactly one match before deleting**

Run via `Unity_RunCommand`:

```csharp
using UnityEngine;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        int count = 0;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == "BuildSign" && t.GetComponent<BuildSign>() == null)
            {
                count++;
                result.Log("Candidate at {0}, components: {1}", t.position, string.Join(",", t.gameObject.GetComponents<Component>()));
            }
        }
        result.Log("Total candidates: {0}", count);
    }
}
```

Expected: exactly 1 candidate, at (-14, 2, 0), with only a `Transform` and a `SpriteRenderer` (no `BuildSign`, no `BoxCollider2D`). If the count is anything other than 1, or a candidate has a `BoxCollider2D`, stop — something has changed since the spec was written and this needs re-investigating before deleting anything.

- [ ] **Step 2: Delete it**

Run via `Unity_RunCommand`:

```csharp
using UnityEngine;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == "BuildSign" && t.GetComponent<BuildSign>() == null)
            {
                result.DestroyObject(t.gameObject);
                result.Log("Deleted stray object at {0}", t.position);
            }
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scenes/SampleScene.unity
git commit -m "Remove unused BuildSign-named debris object from SampleScene

No sprite, no script, no collider, unconnected to either build system."
```

---

### Task 4: Wire Building.cs and Building.prefab for the Abandoned-state visual

**Depends on Task 1** (needs `Assets/Prefabs/BuildingPreBuildingStage.prefab` to exist).

**Files:**
- Modify: `Assets/Scripts/Building.cs`
- Modify: `Assets/Prefabs/Building.prefab` (via editor script)

**Interfaces:**
- Consumes: `Assets/Prefabs/BuildingPreBuildingStage.prefab` from Task 1; the existing `Building_Sprites` child inside `Building.prefab` (direct child of the prefab root, holds the real finished facade art).
- Produces: `Building.preBuildVisual` and `Building.buildingSprites` (both private `[SerializeField] GameObject`). `RefreshVisuals()` shows `preBuildVisual` and hides `buildingSprites` when `State == BuildingState.Abandoned`; shows `buildingSprites` and hides `preBuildVisual` otherwise. Because both fields and the new child live inside the shared prefab, this propagates to all six town buildings (and the Homestead's own `Building` component, which is otherwise dormant during construction — see spec) without touching any scene instance.

Note on the discovery that shaped this task: `Square` (the tinted placeholder `facadeRenderer`) is `activeSelf: false` on every building instance in the live scene today, and `RefreshVisuals()` never activates it — only sets `.color`. `Building_Sprites` is unconditionally active regardless of state. So today every building already shows its finished facade even while `Abandoned`. This task is what first makes `Abandoned` actually look unrestored — confirmed with the user as the intended fix, not a preserved-behavior constraint. `Square`'s dead color-tint code is left untouched; it was not part of what was asked.

- [ ] **Step 1: Add the fields and the toggle**

In `Assets/Scripts/Building.cs`, change:

```csharp
    [SerializeField] private SpriteRenderer facadeRenderer;
```

to:

```csharp
    [SerializeField] private SpriteRenderer facadeRenderer;
    [SerializeField] private GameObject preBuildVisual;
    [SerializeField] private GameObject buildingSprites;
```

Change:

```csharp
    private void RefreshVisuals()
    {
        if (windowLights != null)
            foreach (var light in windowLights)
                light.enabled = State == BuildingState.Restored;

        if (facadeRenderer != null)
            facadeRenderer.color = State switch
            {
                BuildingState.Abandoned => new Color(0.55f, 0.45f, 0.65f),
                BuildingState.Purchased => new Color(0.75f, 0.55f, 0.35f),
                BuildingState.Cleared => new Color(0.4f, 0.8f, 0.55f),
                BuildingState.Restored => new Color(1f, 0.85f, 0.4f),
                _ => Color.white
            };
    }
```

to:

```csharp
    private void RefreshVisuals()
    {
        if (windowLights != null)
            foreach (var light in windowLights)
                light.enabled = State == BuildingState.Restored;

        bool isAbandoned = State == BuildingState.Abandoned;
        if (preBuildVisual != null)
            preBuildVisual.SetActive(isAbandoned);
        if (buildingSprites != null)
            buildingSprites.SetActive(!isAbandoned);

        if (facadeRenderer != null)
            facadeRenderer.color = State switch
            {
                BuildingState.Abandoned => new Color(0.55f, 0.45f, 0.65f),
                BuildingState.Purchased => new Color(0.75f, 0.55f, 0.35f),
                BuildingState.Cleared => new Color(0.4f, 0.8f, 0.55f),
                BuildingState.Restored => new Color(1f, 0.85f, 0.4f),
                _ => Color.white
            };
    }
```

`Assets/Tests/PlayMode/BuildingRenovationFlowTests.cs` and `Assets/Tests/EditMode/BuildSignTests.cs` both build `Building` via bare `AddComponent`, so `preBuildVisual`/`buildingSprites` are null there — the guards keep those tests passing unchanged, same reasoning as Task 2.

- [ ] **Step 2: Add the child instance to the prefab and wire it**

Editing a prefab **asset** (not a scene instance) needs `PrefabUtility.LoadPrefabContents`/`SaveAsPrefabAsset` rather than `GameObject.Find`. Run via `Unity_RunCommand`:

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        string path = "Assets/Prefabs/Building.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);

        var buildingComp = root.GetComponent<Building>();
        var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BuildingPreBuildingStage.prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, root.transform);
        instance.transform.localPosition = new Vector3(-2.5f, 2.5f, 0f);
        instance.transform.localScale = Vector3.one;

        var buildingSprites = root.transform.Find("Building_Sprites");
        if (buildingSprites == null)
        {
            result.LogError("Building_Sprites child not found under the Building prefab root");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        var so = new SerializedObject(buildingComp);
        so.FindProperty("preBuildVisual").objectReferenceValue = instance;
        so.FindProperty("buildingSprites").objectReferenceValue = buildingSprites.gameObject;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        result.Log("Added preBuildVisual to Building.prefab and wired both fields");
    }
}
```

- [ ] **Step 3: Run both test suites**

Same procedure as Task 2 Step 3. Expected: still `passed=195 failed=0` EditMode, `passed=67 failed=0` PlayMode.

- [ ] **Step 4: Playtest verification**

Enter Play mode. Find any unpurchased town building (all start `Abandoned`) and confirm it now shows the fence-outline construction-site border instead of its finished facade. Purchase it (or call `BuildingManager.Instance.TryPurchase` on it via `Unity_RunCommand` if reaching one in-game is slow) and confirm the fence-outline disappears and the real facade appears — matching today's `Purchased` appearance exactly, since that state's handling didn't change.

**Do not mark this step done without actually entering Play mode.**

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Building.cs Assets/Prefabs/Building.prefab
git commit -m "Show the construction-site fence outline while a building is Abandoned

Square was already dead code (never activated) and Building_Sprites was
always on regardless of state, so every unpurchased building already showed
its finished facade. This is the first thing to actually hide it: Abandoned
now shows the fence-outline and hides the real facade; Purchased/Cleared/
Restored are unchanged. One prefab edit reaches all six town buildings."
```

---

### Task 5: Update the art-tracking docs

**Depends on Tasks 1-4.**

**Files:**
- Modify: `Assets/Docs/SpriteTracker.md`
- Modify: `Assets/Docs/BuildPlan.md`

**Interfaces:**
- Consumes: nothing.
- Produces: nothing consumed by later tasks — documentation only.

- [ ] **Step 1: Update SpriteTracker.md**

Change the `BuildSign` row in the Scene-Placed Interactables table from:

```
| BuildSign | Tan 16×16 procedural | Needs art | Signpost/foundation marker sprite |
```

to:

```
| BuildSign (Site stage) | `BuildingPreBuildingStage.prefab` fence outline | Done | Foundation/Frame/Walls still use the tan tint-and-scale placeholder |
```

Add a new row directly beneath the `Homestead (Building)` row:

```
| Building (Abandoned state, all 6 town buildings) | `BuildingPreBuildingStage.prefab` fence outline | Done | Purchased/Cleared/Restored show `Building_Sprites` (real facade) unchanged |
```

Add a row to the Tileset Sources table:

```
| Grasslands (fence pieces) | `Assets/Sprite/Grasslands_tileset/Grassland Spring@128x128.png` | BuildingPreBuildingStage.prefab (construction-site border) |
```

- [ ] **Step 2: Update BuildPlan.md Phase 2**

Change:

```
- [ ] BuildSign stages: signpost (Site), stone foundation outline (Foundation), wood frame (Frame), building exterior (Walls) — scale sprites to match 7×8 final building.
```

to:

```
- [ ] BuildSign stages: **Site done 2026-07-28** — `BuildingPreBuildingStage.prefab`, a staked-out fence-outline construction site, replaces the tan placeholder for `BuildStage.Site`. Foundation, Frame, and Walls still use the tint-and-scale placeholder.
- [x] Town building Abandoned state (all 6, via the shared `Building.prefab`): same `BuildingPreBuildingStage.prefab` fence outline now hides the real facade until purchased — done 2026-07-28. Purchased/Cleared/Restored unchanged.
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Docs/SpriteTracker.md Assets/Docs/BuildPlan.md
git commit -m "Record the construction-site fence outline in the art tracker

BuildSign's Site stage and every town building's Abandoned state now use
BuildingPreBuildingStage.prefab; both docs reflect what's actually shipped
versus what's still a placeholder."
```

---

## Self-Review

**Spec coverage.** Decision 1 (scope: Site-only / Abandoned-only) → Tasks 2 and 4. Decision 2 (reusable prefab, typo fixed) → Task 1. Decision 3 (Homestead wiring, independent sibling, bottom-left alignment) → Tasks 1-2. Decision 4 (town building wiring, corrected after the live-scene check to also hide `Building_Sprites`) → Task 4. Decision 5 (delete the stray debris object) → Task 3. Decision 6 (out of scope: no new colliders, no Purchased/Cleared/Restored/Walls art) → not touched by any task, as intended.

**Placeholder scan.** Every code step contains complete, runnable C#, not a description of what to do. The three `Unity_RunCommand` scripts in Task 1/2/3/4 use the tool's required `internal class CommandScript : IRunCommand` shape. No task says "wire the reference" without showing the exact `SerializedObject`/`objectReferenceValue` call.

**Type consistency.** `siteVisual` (Task 2) and `preBuildVisual`/`buildingSprites` (Task 4) keep identical names between the C# field declarations and the `SerializedObject.FindProperty` calls that wire them. `BuildingPreBuildingStage.prefab`'s path (`Assets/Prefabs/BuildingPreBuildingStage.prefab`) is the same literal string in Tasks 1, 2, and 4.

**Known risks.** Task 1 Step 3's visual check is the only guard against the position math being wrong — if `BuildSign`'s live position has drifted from (-23, 7) since the spec was written, Step 1's own verification query catches that before Step 2 commits to a now-wrong offset. Task 4's prefab edit touches every town building at once; Step 3's full test run and Step 4's playtest (checking both an Abandoned and a just-purchased building) are what catch a regression before it reaches all six.
