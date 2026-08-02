# Recipe Book UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the grandfather's recipe book as a uGUI two-page spread the player flips through, readable anywhere and brewable at a vat.

**Architecture:** Pagination and page-status decisions go into `Assets/Scripts/Rules/RecipeBookRules.cs` as pure C#, fully tested in EditMode without a scene. The view is two focused uGUI MonoBehaviours — `RecipeBookUI` owns spread index, mode and events; `RecipeBookPageView` renders exactly one page. A new `Menus` input action map opens the book; the existing vat event opens it in brew mode.

**Tech Stack:** Unity 6 (6000.2.14f1), URP 17.2.0, C#, uGUI + TextMeshPro, Unity Input System, NUnit via Unity Test Framework (`Lamplight.EditModeTests`, `Lamplight.PlayModeTests`, `Lamplight.TestSupport`).

## Global Constraints

- **No comments in code** unless explicitly requested (`AGENTS.md`). Existing `Rules/` files carry XML doc comments explaining *why* — match that, add no inline `//` narration.
- **`Assets/Scripts/Rules/` must be pure C#.** No `UnityEngine` types except `Mathf`.
- **No direct cross-manager calls.** Communicate via `GameEvents`.
- **No frameworks, no ScriptableObjects, no DI.** Content hardcoded in `ContentDb`.
- **Art stays placeholder.** `BuildPlan.md` Phase 2 owns real art. Flat colours only.
- **`AGENTS.md:181` says all UI is IMGUI.** This plan deliberately breaks that for this one panel and amends the rule in Task 5. `SellUI`, `RequestBookUI` and `DialogueUI` stay IMGUI and are **not** converted.
- `docs/superpowers/` is untracked by convention. Do **not** `git add` anything under it.

**Naming caution:** this is the *grandfather's recipe book*. `RequestBook` / `RequestBookUI` is the stand's book of customer orders — a completely different thing. Never swap the names.

## Running tests

Unity must be **closed** for batchmode, or it fails with "already open in another instance":

```bash
"C:/Program Files/Unity/Hub/Editor/6000.2.14f1/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults editmode.xml -logFile editmode.log
```

If the editor must stay open, drive `UnityEditor.TestTools.TestRunner.Api.TestRunnerApi` from an `[InitializeOnLoad]` editor script instead — a callback created from a dynamically compiled assembly does **not** survive the play-mode domain reload, so PlayMode results will be silently lost otherwise.

**Baseline before starting: EditMode 172 passing, PlayMode 67 passing.**

## File Structure

| File | Fate | Responsibility |
|---|---|---|
| `Assets/Scripts/Rules/RecipeBookRules.cs` | Modify | Add `BookSpread`, `CompileSpreads`, `ClampSpreadIndex`, `LockReason`, `PageStatus`, `StatusOf` |
| `Assets/Scripts/GameEvents.cs` | Modify | Add `RecipeBookRequested` |
| `Assets/Scripts/Input/InputSystem_Actions.inputactions` | Modify | Add the `Menus` map with a `RecipeBook` action |
| `Assets/Scripts/UI/RecipeBookPageView.cs` | Create | Render one page |
| `Assets/Scripts/UI/RecipeBookUI.cs` | Create | Spread index, mode, events, navigation |
| `Assets/Scripts/UI/RecipeSelectUI.cs` | **Delete** | Replaced |
| `Assets/Tests/EditMode/RecipeBookSpreadTests.cs` | Create | Pagination and clamping |
| `Assets/Tests/EditMode/RecipeBookPageStatusTests.cs` | Create | Lock and affordability |
| `AGENTS.md` | Modify | Record the uGUI exception |

---

### Task 1: Spread pagination

Pure domain. No Unity, no scene.

**Files:**
- Modify: `Assets/Scripts/Rules/RecipeBookRules.cs`
- Test: `Assets/Tests/EditMode/RecipeBookSpreadTests.cs`

**Interfaces:**
- Consumes: `BookPage` (already in `RecipeBookRules.cs` — has `PageNumber`, `Recipe`, `IsLegible`), `RecipeBookRules.CompilePages`.
- Produces:
  - `readonly struct BookSpread` with `BookPage Left`, `BookPage Right`, `bool HasRight`, `bool IsBurnedSection`
  - `RecipeBookRules.CompileSpreads(IReadOnlyList<BookPage>) -> List<BookSpread>`
  - `RecipeBookRules.ClampSpreadIndex(int index, int spreadCount) -> int`

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/RecipeBookSpreadTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class RecipeBookSpreadTests
{
    private static List<BookPage> Pages(int count)
    {
        var pages = new List<BookPage>();
        for (int i = 0; i < count; i++)
            pages.Add(new BookPage(i + 1, new RecipeData($"R{i + 1}", 3, 2, ContentDb.BerryShine)));
        return pages;
    }

    [Test]
    public void FourPages_ProduceTwoSpreadsPlusTheBurnedSection()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.AreEqual(3, spreads.Count);
    }

    [Test]
    public void EvenPageCount_EverySpreadHasBothPages()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.IsTrue(spreads[0].HasRight);
        Assert.IsTrue(spreads[1].HasRight);
    }

    [Test]
    public void OddPageCount_LastRecipeSpreadHasNoRightPage()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(5));

        Assert.AreEqual(4, spreads.Count);
        Assert.IsFalse(spreads[2].HasRight);
        Assert.AreEqual(5, spreads[2].Left.PageNumber);
    }

    [Test]
    public void BurnedSpread_IsAlwaysLast()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(5));

        Assert.IsTrue(spreads[spreads.Count - 1].IsBurnedSection);
        for (int i = 0; i < spreads.Count - 1; i++)
            Assert.IsFalse(spreads[i].IsBurnedSection);
    }

    [Test]
    public void BurnedSpread_IsPresentEvenWithNoRecipes()
    {
        var spreads = RecipeBookRules.CompileSpreads(new List<BookPage>());

        Assert.AreEqual(1, spreads.Count);
        Assert.IsTrue(spreads[0].IsBurnedSection);
    }

    [Test]
    public void CompileSpreads_ToleratesNull()
    {
        var spreads = RecipeBookRules.CompileSpreads(null);

        Assert.AreEqual(1, spreads.Count);
        Assert.IsTrue(spreads[0].IsBurnedSection);
    }

    [Test]
    public void PageNumbers_RunInOrderAcrossSpreads()
    {
        var spreads = RecipeBookRules.CompileSpreads(Pages(4));

        Assert.AreEqual(1, spreads[0].Left.PageNumber);
        Assert.AreEqual(2, spreads[0].Right.PageNumber);
        Assert.AreEqual(3, spreads[1].Left.PageNumber);
        Assert.AreEqual(4, spreads[1].Right.PageNumber);
    }

    [Test]
    public void ATornPage_HoldsItsSlotRatherThanCollapsing()
    {
        var pages = new List<BookPage>
        {
            new BookPage(1, new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)),
            new BookPage(2, null),
            new BookPage(3, new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine))
        };

        var spreads = RecipeBookRules.CompileSpreads(pages);

        Assert.AreEqual(2, spreads[0].Right.PageNumber);
        Assert.IsFalse(spreads[0].Right.IsLegible);
        Assert.AreEqual(3, spreads[1].Left.PageNumber);
    }

    [Test]
    public void ClampSpreadIndex_BelowZero_ReturnsZero()
    {
        Assert.AreEqual(0, RecipeBookRules.ClampSpreadIndex(-3, 4));
    }

    [Test]
    public void ClampSpreadIndex_PastTheEnd_ReturnsLastSpread()
    {
        Assert.AreEqual(3, RecipeBookRules.ClampSpreadIndex(99, 4));
    }

    [Test]
    public void ClampSpreadIndex_InRange_IsUnchanged()
    {
        Assert.AreEqual(2, RecipeBookRules.ClampSpreadIndex(2, 4));
    }

    [Test]
    public void ClampSpreadIndex_EmptyBook_ReturnsZero()
    {
        Assert.AreEqual(0, RecipeBookRules.ClampSpreadIndex(5, 0));
    }
}
```

- [ ] **Step 2: Run the tests and watch them fail**

Run the EditMode command above.
Expected: compile failure — `BookSpread`, `CompileSpreads` and `ClampSpreadIndex` do not exist. That is the correct first failure.

- [ ] **Step 3: Add `BookSpread` and the two functions**

Append to `Assets/Scripts/Rules/RecipeBookRules.cs`, inside the file but outside the existing `RecipeBookRules` class for the struct, and inside the class for the methods.

Add above `public static class RecipeBookRules`:

```csharp
/// <summary>
/// Two facing pages of the book, or the burned back section.
///
/// The burned spread carries no pages at all — it is the end of the book, not a
/// page of it, which is why <see cref="RecipeBookRules.CompilePages"/> never
/// emits it and <see cref="RecipeBookRules.CompileSpreads"/> always appends it.
/// </summary>
public readonly struct BookSpread
{
    public readonly BookPage Left;
    public readonly BookPage Right;
    public readonly bool HasRight;
    public readonly bool IsBurnedSection;

    public BookSpread(BookPage left, BookPage right, bool hasRight, bool isBurnedSection)
    {
        Left = left;
        Right = right;
        HasRight = hasRight;
        IsBurnedSection = isBurnedSection;
    }

    public static BookSpread Burned() => new BookSpread(default, default, false, true);
}
```

Add inside `RecipeBookRules`:

```csharp
    /// <summary>
    /// Groups pages into facing pairs and appends the burned section as the final
    /// spread. An odd page count leaves the last right-hand page blank rather than
    /// pulling the burned section forward — a book does not reflow.
    /// </summary>
    public static List<BookSpread> CompileSpreads(IReadOnlyList<BookPage> pages)
    {
        var spreads = new List<BookSpread>();

        if (pages != null)
        {
            for (int i = 0; i < pages.Count; i += 2)
            {
                bool hasRight = i + 1 < pages.Count;
                spreads.Add(new BookSpread(
                    pages[i],
                    hasRight ? pages[i + 1] : default,
                    hasRight,
                    false));
            }
        }

        spreads.Add(BookSpread.Burned());
        return spreads;
    }

    public static int ClampSpreadIndex(int index, int spreadCount)
    {
        if (spreadCount <= 0) return 0;
        if (index < 0) return 0;
        if (index >= spreadCount) return spreadCount - 1;
        return index;
    }
```

- [ ] **Step 4: Run the tests and watch them pass**

Run the EditMode command.
Expected: all 12 `RecipeBookSpreadTests` pass, and the 9 pre-existing `RecipeBookRulesTests` still pass — in particular `BurnedSection_IsNotARecipe_SoNoUnlockCanEverRestoreIt`, which asserts `CompilePages` never emits the burned section as a page. Total EditMode 184.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/RecipeBookRules.cs \
        Assets/Tests/EditMode/RecipeBookSpreadTests.cs Assets/Tests/EditMode/RecipeBookSpreadTests.cs.meta
git commit -m "Group recipe book pages into facing spreads

The burned section is appended as the final spread rather than emitted as a
page, so it stays unreachable by discovery. An odd page count leaves the last
right-hand page blank because a book does not reflow."
```

---

### Task 2: Page status

**Depends on nothing in Task 1** — independent, but lives in the same file, so run after Task 1 to avoid a merge conflict.

**Files:**
- Modify: `Assets/Scripts/Rules/RecipeBookRules.cs`
- Test: `Assets/Tests/EditMode/RecipeBookPageStatusTests.cs`

**Interfaces:**
- Consumes: `BookPage`; `RecipeData` (has `recipeName`, `fermentationHours`, `outputCount`, `outputItem`, `unlockedByBuildingId`, `minReputation`, `Costs`); `ItemDef`.
- Produces:
  - `enum LockReason { None, RequiresBuilding, RequiresReputation }`
  - `readonly struct PageStatus` with `IsTorn`, `IsUnlocked`, `CanAfford`, `Reason`, `RequiredBuildingId`, `RequiredReputation`, `CanBrew`
  - `RecipeBookRules.StatusOf(BookPage, Func<RecipeData,bool> isUnlocked, Func<ItemDef,int> getCount) -> PageStatus`

**Refinement of the spec:** the spec's `PageStatus` comment said `RequiredBuildingId` is null unless `Reason == RequiresBuilding`. That would lose information, because a recipe may gate on **both** a building and reputation, and the current `DrawLegiblePage` shows both hints. This plan populates **both** requirement fields whenever the recipe declares them, and `Reason` names the primary one, with building taking precedence. No hint is lost.

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/RecipeBookPageStatusTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class RecipeBookPageStatusTests
{
    private static RecipeData Simple() =>
        new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine)
            .AddIngredient(ContentDb.Berry, 3);

    private static RecipeData GatedByBuilding() =>
        new RecipeData("Sweet Batch", 6, 4, ContentDb.SweetMoonshine, "Bakery")
            .AddIngredient(ContentDb.Sugar, 2);

    private static RecipeData GatedByReputation() =>
        new RecipeData("Aged Reserve", 12, 3, ContentDb.AgedReserve, null, 50)
            .AddIngredient(ContentDb.Grain, 2);

    private static RecipeData GatedByBoth() =>
        new RecipeData("Highland Mash", 8, 5, ContentDb.HighlandMoonshine, "Mill", 20)
            .AddIngredient(ContentDb.Grain, 4);

    private static PageStatus Status(RecipeData recipe, bool unlocked, int stock) =>
        RecipeBookRules.StatusOf(new BookPage(1, recipe), _ => unlocked, _ => stock);

    [Test]
    public void TornPage_IsTornAndCannotBrew()
    {
        var status = RecipeBookRules.StatusOf(new BookPage(2, null), _ => true, _ => 99);

        Assert.IsTrue(status.IsTorn);
        Assert.IsFalse(status.CanBrew);
    }

    [Test]
    public void UnlockedAndStocked_CanBrew()
    {
        var status = Status(Simple(), unlocked: true, stock: 10);

        Assert.IsTrue(status.IsUnlocked);
        Assert.IsTrue(status.CanAfford);
        Assert.IsTrue(status.CanBrew);
        Assert.AreEqual(LockReason.None, status.Reason);
    }

    [Test]
    public void UnlockedButShortOfIngredients_CannotBrew()
    {
        var status = Status(Simple(), unlocked: true, stock: 1);

        Assert.IsTrue(status.IsUnlocked);
        Assert.IsFalse(status.CanAfford);
        Assert.IsFalse(status.CanBrew);
    }

    [Test]
    public void LockedByBuilding_ReportsTheBuilding()
    {
        var status = Status(GatedByBuilding(), unlocked: false, stock: 99);

        Assert.IsFalse(status.IsUnlocked);
        Assert.IsFalse(status.CanBrew);
        Assert.AreEqual(LockReason.RequiresBuilding, status.Reason);
        Assert.AreEqual("Bakery", status.RequiredBuildingId);
    }

    [Test]
    public void LockedByReputation_ReportsTheThreshold()
    {
        var status = Status(GatedByReputation(), unlocked: false, stock: 99);

        Assert.AreEqual(LockReason.RequiresReputation, status.Reason);
        Assert.AreEqual(50, status.RequiredReputation);
    }

    [Test]
    public void LockedByBoth_PrefersTheBuilding_ButKeepsBothRequirements()
    {
        var status = Status(GatedByBoth(), unlocked: false, stock: 99);

        Assert.AreEqual(LockReason.RequiresBuilding, status.Reason);
        Assert.AreEqual("Mill", status.RequiredBuildingId);
        Assert.AreEqual(20, status.RequiredReputation);
    }

    [Test]
    public void ALockedPage_StillReportsAffordabilityHonestly()
    {
        var status = Status(GatedByBuilding(), unlocked: false, stock: 0);

        Assert.IsFalse(status.CanAfford);
    }

    [Test]
    public void StatusOf_ToleratesNullDelegates()
    {
        var status = RecipeBookRules.StatusOf(new BookPage(1, Simple()), null, null);

        Assert.IsFalse(status.CanBrew);
    }
}
```

- [ ] **Step 2: Run the tests and watch them fail**

Run the EditMode command.
Expected: compile failure — `LockReason`, `PageStatus` and `StatusOf` do not exist.

- [ ] **Step 3: Add `LockReason`, `PageStatus` and `StatusOf`**

Add above `public static class RecipeBookRules`:

```csharp
public enum LockReason { None, RequiresBuilding, RequiresReputation }

/// <summary>
/// Everything the view needs to decide how one page reads, as data rather than
/// formatted text. A torn page reports nothing but <see cref="IsTorn"/>, so there
/// is no path by which an undiscovered recipe's name or costs reach the view.
///
/// A recipe may gate on both a building and reputation. <see cref="Reason"/> names
/// the primary gate, but both requirement fields stay populated so the view can
/// show either without asking the recipe again.
/// </summary>
public readonly struct PageStatus
{
    public readonly bool IsTorn;
    public readonly bool IsUnlocked;
    public readonly bool CanAfford;
    public readonly LockReason Reason;
    public readonly string RequiredBuildingId;
    public readonly int RequiredReputation;

    public bool CanBrew => !IsTorn && IsUnlocked && CanAfford;

    public PageStatus(bool isTorn, bool isUnlocked, bool canAfford,
        LockReason reason, string requiredBuildingId, int requiredReputation)
    {
        IsTorn = isTorn;
        IsUnlocked = isUnlocked;
        CanAfford = canAfford;
        Reason = reason;
        RequiredBuildingId = requiredBuildingId;
        RequiredReputation = requiredReputation;
    }

    public static PageStatus Torn() =>
        new PageStatus(true, false, false, LockReason.None, null, 0);
}
```

Add inside `RecipeBookRules`:

```csharp
    public static PageStatus StatusOf(BookPage page,
        Func<RecipeData, bool> isUnlocked,
        Func<ItemDef, int> getCount)
    {
        if (!page.IsLegible) return PageStatus.Torn();

        var recipe = page.Recipe;
        bool unlocked = isUnlocked != null && isUnlocked(recipe);
        bool canAfford = getCount != null;

        if (canAfford)
        {
            foreach (var cost in recipe.Costs)
            {
                if (getCount(cost.Key) < cost.Value)
                {
                    canAfford = false;
                    break;
                }
            }
        }

        var reason = LockReason.None;
        if (!unlocked)
        {
            if (!string.IsNullOrEmpty(recipe.unlockedByBuildingId))
                reason = LockReason.RequiresBuilding;
            else if (recipe.minReputation > 0)
                reason = LockReason.RequiresReputation;
        }

        return new PageStatus(false, unlocked, canAfford, reason,
            recipe.unlockedByBuildingId, recipe.minReputation);
    }
```

`System` must be in the file's usings for `Func<>` — it is already there.

- [ ] **Step 4: Run the tests and watch them pass**

Run the EditMode command.
Expected: all 8 `RecipeBookPageStatusTests` pass. Total EditMode 192.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/RecipeBookRules.cs \
        Assets/Tests/EditMode/RecipeBookPageStatusTests.cs Assets/Tests/EditMode/RecipeBookPageStatusTests.cs.meta
git commit -m "Move recipe page lock and affordability into Rules

This logic lived untested inside the draw code. It is now a pure PageStatus
with the lock reason carried as data, so the view formats and Rules decides,
and a torn page still reports nothing that could leak a recipe name."
```

---

### Task 3: The event and the input map

**Depends on nothing.** Can run before or after Tasks 1-2.

**Files:**
- Modify: `Assets/Scripts/GameEvents.cs`
- Modify: `Assets/Scripts/Input/InputSystem_Actions.inputactions`
- Test: `Assets/Tests/EditMode/RecipeBookEventTests.cs`

**Interfaces:**
- Produces: `GameEvents.RecipeBookRequested` / `GameEvents.OnRecipeBookRequested()`; a `Menus` action map exposing `RecipeBook`, surfaced by Unity's code generator as `InputSystem_Actions.MenusActions` and `IMenusActions`.

- [ ] **Step 1: Add the event**

In `Assets/Scripts/GameEvents.cs`, add after the `RequestBookRequested` declaration:

```csharp
    public static event System.Action RecipeBookRequested;
```

And after the `OnRequestBookRequested` invoker:

```csharp
    public static void OnRecipeBookRequested()
        => RecipeBookRequested?.Invoke();
```

`GameEventsReset.ClearAll()` reflects over every static delegate field, so it needs no edit.

- [ ] **Step 2: Write the failing test**

Create `Assets/Tests/EditMode/RecipeBookEventTests.cs`:

```csharp
using Lamplight.TestSupport;
using NUnit.Framework;

public class RecipeBookEventTests
{
    [SetUp]
    public void SetUp() => GameEventsReset.ClearAll();

    [TearDown]
    public void TearDown() => GameEventsReset.ClearAll();

    [Test]
    public void OnRecipeBookRequested_NotifiesSubscribers()
    {
        int calls = 0;
        GameEvents.RecipeBookRequested += () => calls++;

        GameEvents.OnRecipeBookRequested();

        Assert.AreEqual(1, calls);
    }

    [Test]
    public void OnRecipeBookRequested_WithNoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameEvents.OnRecipeBookRequested());
    }

    [Test]
    public void ClearAll_RemovesRecipeBookSubscribers()
    {
        int calls = 0;
        GameEvents.RecipeBookRequested += () => calls++;

        GameEventsReset.ClearAll();
        GameEvents.OnRecipeBookRequested();

        Assert.AreEqual(0, calls);
    }
}
```

- [ ] **Step 3: Run the tests and watch them pass**

Run the EditMode command.
Expected: 3 new tests pass. Total EditMode 195.

Note this task's test is written after the implementation because the event is a one-line declaration with no behaviour to drive out — the test locks in that `ClearAll` reaches it, which is the part that could silently break.

- [ ] **Step 4: Add the `Menus` action map**

`Assets/Scripts/Input/InputSystem_Actions.inputactions` is JSON with top-level keys `version`, `name`, `maps`, `controlSchemes`. Each map has `name`, `id`, `actions`, `bindings`.

Append this object to the `maps` array, after the `UI` map:

```json
{
    "name": "Menus",
    "id": "3d1f6c2a-7b48-4e91-9c05-8a2d5e6f7b31",
    "actions": [
        {
            "name": "RecipeBook",
            "type": "Button",
            "id": "9e4a1b73-2c56-48df-a0e1-5b7c9d3f2a64",
            "expectedControlType": "Button",
            "processors": "",
            "interactions": "",
            "initialStateCheck": false
        }
    ],
    "bindings": [
        {
            "name": "",
            "id": "c7b25d81-4f39-4a62-b8d0-1e6a9c4f5b27",
            "path": "<Keyboard>/b",
            "interactions": "",
            "processors": "",
            "groups": "Keyboard&Mouse",
            "action": "RecipeBook",
            "isComposite": false,
            "isPartOfComposite": false
        },
        {
            "name": "",
            "id": "f2a68e50-9d13-4c7b-85af-3b1d7e2c6a49",
            "path": "<Gamepad>/select",
            "interactions": "",
            "processors": "",
            "groups": "Gamepad",
            "action": "RecipeBook",
            "isComposite": false,
            "isPartOfComposite": false
        }
    ]
}
```

`B` was chosen because `P` is taken by `DebugMenu` and `E` by dialogue advance and `Interact`.

- [ ] **Step 5: Verify Unity regenerated the C# wrapper**

Focus the Unity editor (or run `AssetDatabase.Refresh`) and confirm `Assets/Scripts/Input/InputSystem_Actions.cs` now contains a `MenusActions` struct and an `IMenusActions` interface.

Run: `grep -n "MenusActions\|IMenusActions\|m_Menus_RecipeBook" Assets/Scripts/Input/InputSystem_Actions.cs`
Expected: several matches. If there are none, the JSON is malformed — validate it before continuing.

**Do not hand-edit `InputSystem_Actions.cs`.** It is generated; edits are lost on the next import.

- [ ] **Step 6: Confirm `IPlayerActions` did not change**

Run: `git diff --stat Assets/Scripts/Input/InputSystem_Actions.cs`

The diff must be **additive only**. If `IPlayerActions` gained or lost a member, the new action landed in the wrong map — move it to `Menus` and re-import. A changed `IPlayerActions` breaks `PlayerController`, which implements it.

- [ ] **Step 7: Run both suites**

Expected: EditMode 195, PlayMode 67. Both green.

- [ ] **Step 8: Commit**

```bash
git add Assets/Scripts/GameEvents.cs \
        Assets/Scripts/Input/InputSystem_Actions.inputactions \
        Assets/Scripts/Input/InputSystem_Actions.cs \
        Assets/Tests/EditMode/RecipeBookEventTests.cs Assets/Tests/EditMode/RecipeBookEventTests.cs.meta
git commit -m "Add the recipe book open event and a Menus input map

The book gets its own action map rather than joining Player, so the key can
close the book as well as open it — PlayerController gates on IsMenuOpen and
would swallow the second press. It also keeps a menu concern out of the
locomotion controller's IPlayerActions."
```

---

### Task 4: The view

**Depends on Tasks 1, 2 and 3.**

**Files:**
- Create: `Assets/Scripts/UI/RecipeBookPageView.cs`
- Create: `Assets/Scripts/UI/RecipeBookUI.cs`

**Interfaces:**
- Consumes: `BookSpread`, `PageStatus`, `LockReason`, `RecipeBookRules.CompilePages/CompileSpreads/ClampSpreadIndex/StatusOf/BurnedScraps`; `GameEvents.RecipeSelectionRequested`, `GameEvents.RecipeBookRequested`, `GameEvents.MenuCloseRequested`; `FermentManager.Instance` (`Recipes`, `IsRecipeDiscovered`, `IsRecipeUnlocked`, `TryStartBatch`); `InventoryManager.Instance.GetCount`; `PlayerController.Instance.IsMenuOpen`; `InputSystem_Actions`.
- Produces: `RecipeBookUI` and `RecipeBookPageView` MonoBehaviours whose serialized fields Task 5 wires in the prefab.

There are no automated tests for this task — the project has no uGUI test harness, exactly as it has none for IMGUI. Its verification is Task 5's playtest. Everything decidable was already pushed into `Rules/` and tested in Tasks 1 and 2.

- [ ] **Step 1: Write `RecipeBookPageView`**

Create `Assets/Scripts/UI/RecipeBookPageView.cs`:

```csharp
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One page of the book. Holds no game state and asks no manager anything — it
/// renders exactly what it is handed, so the page states stay decidable in Rules.
/// </summary>
public class RecipeBookPageView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text pageNumberLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text footnoteLabel;
    [SerializeField] private Button brewButton;

    private readonly StringBuilder _builder = new();

    public Button BrewButton => brewButton;

    public void ShowBlank()
    {
        if (root != null) root.SetActive(false);
    }

    public void Render(BookPage page, PageStatus status, bool showBrew,
        System.Func<ItemDef, int> getCount)
    {
        if (root != null) root.SetActive(true);

        if (pageNumberLabel != null)
            pageNumberLabel.text = page.PageNumber > 0 ? page.PageNumber.ToString() : "";

        if (status.IsTorn)
        {
            RenderTorn();
            return;
        }

        var recipe = page.Recipe;
        if (titleLabel != null) titleLabel.text = recipe.recipeName;
        if (bodyLabel != null) bodyLabel.text = BuildBody(recipe, getCount);
        if (footnoteLabel != null) footnoteLabel.text = BuildFootnote(recipe, status);

        if (brewButton != null)
        {
            brewButton.gameObject.SetActive(showBrew);
            brewButton.interactable = status.CanBrew;
        }
    }

    private void RenderTorn()
    {
        if (titleLabel != null) titleLabel.text = "";
        if (bodyLabel != null) bodyLabel.text = "(torn out)";
        if (footnoteLabel != null) footnoteLabel.text = "";
        if (brewButton != null) brewButton.gameObject.SetActive(false);
    }

    private string BuildBody(RecipeData recipe, System.Func<ItemDef, int> getCount)
    {
        _builder.Clear();
        foreach (var cost in recipe.Costs)
        {
            int have = getCount != null ? getCount(cost.Key) : 0;
            _builder.AppendLine($"{cost.Key.displayName} x{cost.Value}   (have {have})");
        }
        return _builder.ToString();
    }

    private string BuildFootnote(RecipeData recipe, PageStatus status)
    {
        if (status.Reason == LockReason.RequiresBuilding)
            return $"Restore the {status.RequiredBuildingId} to read this.";
        if (status.Reason == LockReason.RequiresReputation)
            return $"Requires standing {status.RequiredReputation}+.";

        string output = recipe.outputItem != null ? recipe.outputItem.displayName : "???";
        return $"{recipe.fermentationHours}h  ->  {recipe.outputCount} {output}";
    }
}
```

- [ ] **Step 2: Write `RecipeBookUI`**

Create `Assets/Scripts/UI/RecipeBookUI.cs`:

```csharp
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The grandfather's recipe book. Not the stand's request book — see
/// <see cref="RequestBookUI"/>, which is a different thing entirely.
///
/// Read mode opens from anywhere and shows no brew buttons, so the player can
/// meet the burned back section long before they own a vat. Brew mode opens from
/// a vat and is the only mode that can act.
/// </summary>
public class RecipeBookUI : MonoBehaviour
{
    private enum Mode { Read, Brew }

    [SerializeField] private GameObject root;
    [SerializeField] private RecipeBookPageView leftPage;
    [SerializeField] private RecipeBookPageView rightPage;
    [SerializeField] private GameObject burnedPanel;
    [SerializeField] private TMP_Text burnedLabel;
    [SerializeField] private TMP_Text spreadLabel;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private InputSystem_Actions _input;
    private FermentVat _targetVat;
    private Mode _mode = Mode.Read;
    private int _spreadIndex;
    private List<BookSpread> _spreads = new();

    private void Awake()
    {
        _input = new InputSystem_Actions();
        if (root != null) root.SetActive(false);

        if (prevButton != null) prevButton.onClick.AddListener(PreviousSpread);
        if (nextButton != null) nextButton.onClick.AddListener(NextSpread);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (leftPage != null && leftPage.BrewButton != null)
            leftPage.BrewButton.onClick.AddListener(() => BrewFrom(true));
        if (rightPage != null && rightPage.BrewButton != null)
            rightPage.BrewButton.onClick.AddListener(() => BrewFrom(false));
    }

    private void OnEnable()
    {
        GameEvents.RecipeBookRequested += OpenForReading;
        GameEvents.RecipeSelectionRequested += OpenForBrewing;
        GameEvents.MenuCloseRequested += Close;

        _input.Menus.Enable();
        _input.Menus.RecipeBook.performed += OnRecipeBookKey;
    }

    private void OnDisable()
    {
        GameEvents.RecipeBookRequested -= OpenForReading;
        GameEvents.RecipeSelectionRequested -= OpenForBrewing;
        GameEvents.MenuCloseRequested -= Close;

        _input.Menus.RecipeBook.performed -= OnRecipeBookKey;
        _input.Menus.Disable();
    }

    private void OnRecipeBookKey(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (IsOpen) Close();
        else OpenForReading();
    }

    private bool IsOpen => root != null && root.activeSelf;

    private void OpenForReading()
    {
        _targetVat = null;
        Open(Mode.Read);
    }

    private void OpenForBrewing(FermentVat vat)
    {
        _targetVat = vat;
        Open(Mode.Brew);
    }

    private void Open(Mode mode)
    {
        if (FermentManager.Instance == null) return;

        _mode = mode;
        _spreadIndex = 0;
        Rebuild();

        if (root != null) root.SetActive(true);
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
    }

    private void Close()
    {
        if (!IsOpen) return;

        root.SetActive(false);
        _targetVat = null;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void Rebuild()
    {
        var pages = RecipeBookRules.CompilePages(
            FermentManager.Instance.Recipes,
            FermentManager.Instance.IsRecipeDiscovered);

        _spreads = RecipeBookRules.CompileSpreads(pages);
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex, _spreads.Count);
        RenderCurrentSpread();
    }

    private void PreviousSpread()
    {
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex - 1, _spreads.Count);
        RenderCurrentSpread();
    }

    private void NextSpread()
    {
        _spreadIndex = RecipeBookRules.ClampSpreadIndex(_spreadIndex + 1, _spreads.Count);
        RenderCurrentSpread();
    }

    private void RenderCurrentSpread()
    {
        if (_spreads.Count == 0) return;

        var spread = _spreads[_spreadIndex];

        if (spreadLabel != null)
            spreadLabel.text = $"Spread {_spreadIndex + 1} of {_spreads.Count}";
        if (prevButton != null) prevButton.interactable = _spreadIndex > 0;
        if (nextButton != null) nextButton.interactable = _spreadIndex < _spreads.Count - 1;

        if (burnedPanel != null) burnedPanel.SetActive(spread.IsBurnedSection);

        if (spread.IsBurnedSection)
        {
            if (leftPage != null) leftPage.ShowBlank();
            if (rightPage != null) rightPage.ShowBlank();
            if (burnedLabel != null) burnedLabel.text = BuildBurnedText();
            return;
        }

        bool showBrew = _mode == Mode.Brew;

        if (leftPage != null)
            leftPage.Render(spread.Left, StatusFor(spread.Left), showBrew, GetCount);

        if (rightPage != null)
        {
            if (spread.HasRight)
                rightPage.Render(spread.Right, StatusFor(spread.Right), showBrew, GetCount);
            else
                rightPage.ShowBlank();
        }
    }

    private static string BuildBurnedText()
    {
        var builder = new StringBuilder();
        foreach (var scrap in RecipeBookRules.BurnedScraps)
            builder.AppendLine(scrap);
        return builder.ToString();
    }

    private PageStatus StatusFor(BookPage page) =>
        RecipeBookRules.StatusOf(page, FermentManager.Instance.IsRecipeUnlocked, GetCount);

    private static int GetCount(ItemDef item) =>
        InventoryManager.Instance != null ? InventoryManager.Instance.GetCount(item) : 0;

    private void BrewFrom(bool left)
    {
        if (_mode != Mode.Brew || _targetVat == null) return;
        if (_spreads.Count == 0) return;

        var spread = _spreads[_spreadIndex];
        if (spread.IsBurnedSection) return;

        var page = left ? spread.Left : spread.Right;
        if (!left && !spread.HasRight) return;
        if (!StatusFor(page).CanBrew) return;

        FermentManager.Instance.TryStartBatch(_targetVat, page.Recipe);
        Close();
    }
}
```

- [ ] **Step 3: Confirm it compiles**

Refresh Unity and check the console for `error CS`.
Expected: none. If `_input.Menus` does not resolve, Task 3 Step 5 did not actually regenerate the wrapper — go back and fix that before continuing.

Note: Escape is handled by `GameEvents.MenuCloseRequested`, which this panel already subscribes to, so unlike the IMGUI panels it needs no `Keyboard.current` check of its own.

- [ ] **Step 4: Run both suites**

Expected: EditMode 195, PlayMode 67. Nothing new passes or fails — this task adds no tests — but a compile break would take every suite down, so this catches it.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/RecipeBookPageView.cs Assets/Scripts/UI/RecipeBookPageView.cs.meta \
        Assets/Scripts/UI/RecipeBookUI.cs Assets/Scripts/UI/RecipeBookUI.cs.meta
git commit -m "Add the uGUI recipe book view

RecipeBookUI owns the spread index, mode and events; RecipeBookPageView renders
exactly one page and asks no manager anything. Every decision the pages depend
on already lives in Rules and is tested there."
```

---

### Task 5: Prefab, scene wiring, and the playtest

**Depends on Task 4.** This task is Unity editor work and **cannot be completed headless.** If you are an agent without editor access, stop and hand back after Step 1.

**Files:**
- Create: `Assets/Prefabs/RecipeBookCanvas.prefab` (via the editor)
- Modify: `Assets/Scenes/SampleScene.unity` (via the editor)
- Delete: `Assets/Scripts/UI/RecipeSelectUI.cs`
- Modify: `AGENTS.md`

- [ ] **Step 1: Amend the UI convention**

In `AGENTS.md`, replace the line:

```
- **UI is IMGUI.** New panels use `OnGUI`, not uGUI Canvas or UI Toolkit.
```

with:

```
- **UI is IMGUI**, with one exception. New panels use `OnGUI`, not uGUI Canvas or UI Toolkit. The exception is the recipe book (`RecipeBookUI`, `RecipeBookPageView`), which is uGUI + TextMeshPro because it needs a book-spread layout and art-ready slots — see `docs/superpowers/specs/2026-07-26-recipe-book-ui-design.md`. `SellUI`, `RequestBookUI` and `DialogueUI` remain IMGUI; do not convert them.
```

Also update the UI row of the architecture table near the top of the file, from `IMGUI (OnGUI) panels` to `IMGUI (OnGUI) panels; recipe book is uGUI`.

- [ ] **Step 2: Build the prefab**

In the Unity editor, create `Assets/Prefabs/RecipeBookCanvas.prefab`:

```
RecipeBookCanvas          Canvas (Screen Space Overlay, Sort Order 10)
                          CanvasScaler: Scale With Screen Size, reference 1920x1080
                          GraphicRaycaster
                          RecipeBookUI
  Dimmer                  Image, stretch to full rect, colour (0,0,0,0.6)
  Book                    Image, colour (0.91,0.86,0.75,1), centred ~1100x700
    LeftPage              RecipeBookPageView
      PageNumber          TMP_Text
      Title               TMP_Text
      Body                TMP_Text
      Footnote            TMP_Text
      BrewButton          Button + TMP_Text child reading "Brew"
    RightPage             RecipeBookPageView  (same five children)
    BurnedPanel           Image, colour (0.15,0.11,0.07,1), covers both pages
      BurnedLabel         TMP_Text
    PrevButton            Button, left edge
    NextButton            Button, right edge
    SpreadLabel           TMP_Text, bottom centre
    CloseButton           Button, top right
```

Wire every `[SerializeField]`:

- On each `RecipeBookPageView`: `root` = that page's own GameObject, plus its four TMP labels and `brewButton`.
- On `RecipeBookUI`: `root` = the `Book` GameObject, `leftPage`, `rightPage`, `burnedPanel`, `burnedLabel`, `spreadLabel`, `prevButton`, `nextButton`, `closeButton`.

Sort Order 10 puts it above `HUDCanvas`, which sits at 0.

Colours are placeholders. Phase 2 replaces them.

- [ ] **Step 3: Swap the panel in the scene**

Load `Assets/Scenes/SampleScene.unity`.

1. Select `HUDCanvas` and **remove the `RecipeSelectUI` component**.
2. Drag `RecipeBookCanvas.prefab` into the scene root.
3. Save the scene.

- [ ] **Step 4: Delete the old panel**

```bash
git rm Assets/Scripts/UI/RecipeSelectUI.cs Assets/Scripts/UI/RecipeSelectUI.cs.meta
```

Do this only after Step 3, or the scene will log a missing-script error on the `HUDCanvas` object.

- [ ] **Step 5: Run both suites**

Expected: EditMode 195, PlayMode 67. Deleting `RecipeSelectUI` must break nothing — no test references it.

- [ ] **Step 6: Playtest**

Enter play mode and confirm every line:

- `B` opens the book anywhere, and `B` again closes it
- Escape closes it, and movement returns afterwards
- The arrows flip spreads, and clamp — Prev is disabled on the first spread, Next on the last
- Page 1 is legible; the other pages read "(torn out)" and name nothing
- The burned spread is the last thing in the book and shows all three scraps
- **No Brew buttons appear in read mode**
- Interacting with a vat opens the same book *with* Brew buttons
- A recipe you cannot afford has Brew visible but disabled
- A locked recipe shows its footnote reason
- Brewing an affordable unlocked recipe starts the batch and closes the panel

**Do not mark this step done on a compile check.** If you cannot run the editor, stop and hand back.

- [ ] **Step 7: Commit**

```bash
git add AGENTS.md Assets/Prefabs/RecipeBookCanvas.prefab Assets/Prefabs/RecipeBookCanvas.prefab.meta \
        Assets/Scenes/SampleScene.unity
git commit -m "Replace the IMGUI recipe panel with the uGUI book

The book is now a two-page spread the player flips through, readable anywhere
via the Menus map and brewable only from a vat. AGENTS.md records the uGUI
exception and its scope: no other panel converts."
```

---

## Self-Review

**Spec coverage.** Decision 1 (uGUI + TextMeshPro) → Tasks 4 and 5. Decision 2 (readable anywhere, brewable at a vat) → Task 3's event and Task 4's two modes. Decision 3 (spread, flip through) → Task 1's `CompileSpreads` and Task 4's navigation. Decision 4 (`Menus` map) → Task 3. Decision 5 (placeholder art) → Task 5 Step 2 colours. The `AGENTS.md` amendment → Task 5 Step 1. `Rules` additions → Tasks 1 and 2. The prefab tree → Task 5 Step 2. Deleting `RecipeSelectUI` → Task 5 Step 4. Every test named in the spec's testing section appears as real code in Task 1 or Task 2.

**Deliberate deviation from the spec, flagged.** The spec said `PageStatus.RequiredBuildingId` is null unless `Reason == RequiresBuilding`. Task 2 populates both requirement fields whenever the recipe declares them, because a recipe can gate on a building *and* reputation and the current draw code shows both hints. Stricter adherence would have lost information. Called out in Task 2's body.

**Placeholder scan.** Every code step contains real, complete code. Task 3 Step 4 contains the actual JSON with concrete GUIDs rather than "add an action". Task 5 Step 2 names every serialized field to wire rather than "wire the references". Three steps tell the implementer to *verify* rather than assume — that the wrapper regenerated, that `IPlayerActions` is unchanged, and that the playtest is not a compile check — each with a named command or explicit checklist.

**Type consistency.** `BookSpread` fields (`Left`, `Right`, `HasRight`, `IsBurnedSection`) are used identically in Tasks 1, 4 and 5. `PageStatus` members (`IsTorn`, `IsUnlocked`, `CanAfford`, `Reason`, `RequiredBuildingId`, `RequiredReputation`, `CanBrew`) match between Task 2's definition and Task 4's consumption. `StatusOf`, `CompileSpreads`, `ClampSpreadIndex` keep their signatures across Tasks 1, 2 and 4. `RecipeBookPageView.Render(BookPage, PageStatus, bool, Func<ItemDef,int>)` matches every call site in `RecipeBookUI`. `BookPage` and `CompilePages` are consumed exactly as the existing file defines them.

**Known risks.** Task 3 Step 4 hand-edits generated-adjacent JSON; Steps 5 and 6 exist to catch a malformed edit and a misplaced action before they reach `PlayerController`. Task 5 needs a human at the editor, and says so.
