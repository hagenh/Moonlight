# Merge BuildSign Into Homestead Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate the BuildSign proxy by moving all construction-stage logic into Homestead, so a half-built homestead is just a Homestead in a different state.

**Architecture:** Merge BuildSign's fields, BuildStage enum, Interact() logic, stage visuals, and AdvanceStage() directly into Homestead. Delete BuildSign.cs entirely. The Homestead GameObject starts active in the scene (not deactivated) with its trigger collider disabled; as stages advance, it enables the trigger and swaps sprites. GameHUD switches from `is BuildSign` to `is Homestead` for its prompt.

**Tech Stack:** C#, Unity 6, NUnit

## Global Constraints

- No comments in code unless explicitly requested
- No ScriptableObjects for game data
- Rules/ layer must be pure C# (no UnityEngine except Mathf)
- UI is IMGUI (GameHUD is the exception using TextMeshPro)
- Test naming: `Method_Condition_Expected`
- SetUp: `GameEventsReset.ClearAll()` then `TestBootstrap`
- TearDown: `TestBootstrap.DestroyAll()` then `GameEventsReset.ClearAll()`

---

## File Structure

| File | Action | Responsibility |
|------|--------|---------------|
| `Assets/Scripts/BuildSign.cs` | Delete | Removed entirely |
| `Assets/Scripts/Homestead.cs` | Modify | Absorbs BuildStage, stage progression, resource checks, stage visuals |
| `Assets/Scripts/UI/GameHUD.cs` | Modify | Replace `is BuildSign` branch with `is Homestead` |
| `Assets/Tests/EditMode/BuildSignTests.cs` | Delete | Removed entirely |
| `Assets/Tests/EditMode/HomesteadTests.cs` | Modify | Absorbs all BuildSign test cases |
| `Assets/Scenes/SampleScene.unity` | Modify (editor) | Remove BuildSign GameObject, activate Homestead, wire references |

---

### Task 1: Move BuildStage and stage logic into Homestead

**Files:**
- Modify: `Assets/Scripts/Homestead.cs`
- Modify: `Assets/Scripts/BuildSign.cs` (delete afterward)
- Test: `Assets/Tests/EditMode/HomesteadTests.cs`

**Interfaces:**
- Consumes: `BuildStage` enum (currently in BuildSign.cs), `GameEvents.OnHomesteadBuildStageChanged(int)`, `GameEvents.OnToastRequested(string)`, `InventoryManager.Instance.Has/TryRemove`, `ContentDb.Stone/Wood/Nails`
- Produces: `Homestead.Stage` (BuildStage), `Homestead.CanInteract` always true, `Homestead.Interact()` handles resource checks and stage advancement, `BuildStage` enum moved to Homestead.cs

- [ ] **Step 1: Write failing tests in HomesteadTests.cs**

Add the following test cases to `Assets/Tests/EditMode/HomesteadTests.cs`. These mirror the existing BuildSignTests but operate on Homestead directly. The SetUp needs an InventoryManager, and the Homestead needs a SpriteRenderer already (existing SetUp adds one).

```csharp
private InventoryManager _inventory;

// Add to existing SetUp, after _homestead assignment:
_inventory = TestBootstrap.CreateSingleton<InventoryManager>();
```

```csharp
[Test]
public void Interact_NoStone_StaysAtSite()
{
    _homestead.Interact();
    Assert.AreEqual(BuildStage.Site, _homestead.Stage);
}

[Test]
public void Interact_WithStone_AdvancesToFoundation()
{
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    Assert.AreEqual(BuildStage.Foundation, _homestead.Stage);
    Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
}

[Test]
public void Interact_WithWood_AdvancesToFrame()
{
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 3);
    _homestead.Interact();
    Assert.AreEqual(BuildStage.Frame, _homestead.Stage);
    Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
}

[Test]
public void Interact_WithWoodAndNails_AdvancesToWalls()
{
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 2);
    _inventory.TryAdd(ContentDb.Nails, 3);
    _homestead.Interact();
    Assert.AreEqual(BuildStage.Walls, _homestead.Stage);
    Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
    Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));
}

[Test]
public void Interact_FrameWithoutNails_StaysAtFrame()
{
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 2);
    _homestead.Interact();
    Assert.AreEqual(BuildStage.Frame, _homestead.Stage);
}

[Test]
public void Interact_CompleteBuild_SetsIsBuilt()
{
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 3);
    _homestead.Interact();
    _inventory.TryAdd(ContentDb.Wood, 2);
    _inventory.TryAdd(ContentDb.Nails, 3);
    _homestead.Interact();
    Assert.IsTrue(_homestead.IsBuilt);
}

[Test]
public void HomesteadBuildStageChanged_FiresOnAdvance()
{
    int firedStage = -1;
    GameEvents.HomesteadBuildStageChanged += s => firedStage = s;
    _inventory.TryAdd(ContentDb.Stone, 3);
    _homestead.Interact();
    Assert.AreEqual(1, firedStage);
}

[Test]
public void CanInteract_IsAlwaysTrue()
{
    Assert.IsTrue(_homestead.CanInteract);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: EditMode tests in `Lamplight.EditModeTests` assembly, filtered to `HomesteadTests`
Expected: FAIL — `Homestead` does not have `Stage` property, `BuildStage` is not in scope, `CanInteract` returns false when not built

- [ ] **Step 3: Implement the merge in Homestead.cs**

Replace the entire `Assets/Scripts/Homestead.cs` with:

```csharp
using UnityEngine;

public enum BuildStage
{
    Site = 0,
    Foundation = 1,
    Frame = 2,
    Walls = 3
}

public class Homestead : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite builtSprite;
    [SerializeField] private GameObject siteVisual;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _triggerCollider;
    private BuildStage _stage;

    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };
    private static readonly float[] _stageScales = { 1f, 2f, 6f, 6f };

    public bool IsBuilt { get; private set; }
    public BuildStage Stage => _stage;
    public InteractType InteractType => InteractType.Building;
    public bool CanInteract => true;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
            {
                _triggerCollider = col;
                break;
            }
        }
        if (!IsBuilt && _triggerCollider != null)
            _triggerCollider.enabled = false;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
        RefreshSiteVisual();
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

    private void CompleteBuild()
    {
        IsBuilt = true;
        if (_triggerCollider != null)
            _triggerCollider.enabled = true;
        if (_spriteRenderer != null && builtSprite != null)
            _spriteRenderer.sprite = builtSprite;
    }

    public void SetBuiltSpriteForTest(Sprite sprite)
    {
        builtSprite = sprite;
    }
}
```

Key changes from original Homestead:
- `BuildStage` enum moved here from BuildSign.cs
- Added `_stage`, `_stageColors`, `_stageScales`, `siteVisual` fields
- `CanInteract` changed from `=> IsBuilt` to `=> true`
- `Interact()` now contains the full resource-check and stage-advance logic from BuildSign
- `CompleteBuild()` inlines the old `Homestead.SetBuilt()` plus enabling the trigger collider and swapping sprite (no longer activates a separate Homestead GameObject)
- `AdvanceStage()` and `RefreshSiteVisual()` moved from BuildSign
- `SetBuilt()` removed (replaced by `CompleteBuild()` which is private)
- `SetBuiltSpriteForTest()` kept for existing test compatibility

- [ ] **Step 4: Delete BuildSign.cs**

Delete `Assets/Scripts/BuildSign.cs` and its `.meta` file.

- [ ] **Step 5: Run tests to verify they pass**

Run: EditMode tests in `Lamplight.EditModeTests` assembly
Expected: All HomesteadTests pass, no compilation errors referencing BuildSign

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Homestead.cs Assets/Scripts/BuildSign.cs Assets/Scripts/BuildSign.cs.meta Assets/Tests/EditMode/HomesteadTests.cs
git commit -m "feat: merge BuildSign logic into Homestead, delete BuildSign"
```

---

### Task 2: Update GameHUD prompt from BuildSign to Homestead

**Files:**
- Modify: `Assets/Scripts/UI/GameHUD.cs`

**Interfaces:**
- Consumes: `Homestead.Stage` (BuildStage enum now in Homestead.cs)
- Produces: GameHUD no longer references BuildSign

- [ ] **Step 1: Update the GameHUD interactable prompt branch**

In `Assets/Scripts/UI/GameHUD.cs`, replace the `else if (interactable is BuildSign sign)` block (lines 194-204) with:

```csharp
else if (interactable is Homestead h && !h.IsBuilt)
{
    promptText.text = h.Stage switch
    {
        BuildStage.Site => $"[E] Homestead Site (need 3 Stone)",
        BuildStage.Foundation => $"[E] Build Frame (need 3 Wood)",
        BuildStage.Frame => $"[E] Build Walls (need 2 Wood, 3 Nails)",
        BuildStage.Walls => $"[E] Homestead",
        _ => $"[E] Homestead Site"
    };
}
```

The `&& !h.IsBuilt` guard ensures this branch only triggers during construction. Once built, the built Homestead falls through to whatever other handling exists for it (or no prompt, matching current behavior where `Homestead.Interact()` is empty when built).

- [ ] **Step 2: Verify no other BuildSign references remain**

Run: `rg "BuildSign" Assets/Scripts/` — expected: zero matches

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/GameHUD.cs
git commit -m "feat: update GameHUD prompt to use Homestead instead of BuildSign"
```

---

### Task 3: Delete BuildSignTests and clean up scene

**Files:**
- Delete: `Assets/Tests/EditMode/BuildSignTests.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (via Unity editor)

**Interfaces:**
- Consumes: None
- Produces: Clean codebase with no BuildSign references

- [ ] **Step 1: Delete BuildSignTests.cs**

Delete `Assets/Tests/EditMode/BuildSignTests.cs` and its `.meta` file.

- [ ] **Step 2: Verify all tests still pass**

Run: EditMode tests in `Lamplight.EditModeTests` assembly
Expected: All pass, no BuildSign references anywhere

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/BuildSignTests.cs Assets/Tests/EditMode/BuildSignTests.cs.meta
git commit -m "chore: delete BuildSignTests (absorbed into HomesteadTests)"
```

- [ ] **Step 4: Update the scene in Unity editor**

This must be done manually in the Unity editor (scene files should not be hand-edited for structural changes):

1. Open SampleScene
2. Delete the BuildSign GameObject (currently at position -23, 7, 0)
3. Find the Homestead prefab instance (currently inactive) and activate it
4. Position the Homestead at (-23, 7, 0) — same position the BuildSign was at
5. Ensure the Homestead has both a solid BoxCollider2D and a trigger BoxCollider2D
6. Ensure the Homestead's SpriteRenderer has the site-stage color (0.7, 0.6, 0.4) and is at scale (1, 1, 1)
7. Wire up the `siteVisual` and `builtSprite` fields on the Homestead component if appropriate child objects or sprites exist
8. Set the Homestead's layer to "Interactable"

No commit needed for this step — it's an editor-only change.

---

### Task 4: Final verification

- [ ] **Step 1: Grep for any remaining BuildSign references**

Run: `rg "BuildSign" Assets/` — expected: zero matches (excluding .meta files that may linger)

- [ ] **Step 2: Run full test suite**

Run: All EditMode and PlayMode tests
Expected: All pass

- [ ] **Step 3: Play-test in editor**

Enter Play mode, walk to the Homestead site, verify:
- Interaction prompts appear at each build stage
- Resources are consumed correctly
- Stage visuals change (color, scale)
- After final stage, Homestead is marked as built and trigger collider is enabled
