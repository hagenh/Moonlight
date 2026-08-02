# The Recipe Book UI — Design

**Date:** 2026-07-26
**Status:** Approved, ready for an implementation plan
**Subject:** Rebuilding the grandfather's recipe book (`RecipeSelectUI`) as a real uGUI panel

> **Naming caution.** This is the *grandfather's recipe book* — the inherited, damaged book of brewing recipes. It is **not** the stand's *request book* (`RequestBook`, `RequestBookUI`), which holds written customer orders. Never use "RequestBook" for this, and never use "RecipeBook" for that.

## Goal

Turn the recipe book from a scrolling list of IMGUI labels into a book the player can hold: a two-page spread they flip through, where torn pages and the burned back section are things they travel past rather than list items they read.

## Why now

`GameDesign.md` thread #9 frames the book as *"the grandfather's ruined recipe book — mostly destroyed, one legible page — would let the player carry a mysterious damaged object from minute zero, seeding hour six's payoff at minute three."* It is rated **small, self-contained, cheap** and **the earliest thing the player would meet**.

`Rules/RecipeBookRules.cs` already carries that design — numbered pages that stay in place as they become legible, torn pages that deliberately reveal nothing, and three burned scraps seeding the cellar. The current `OnGUI` panel expresses almost none of it, and only opens when the player is standing at a vat.

**State at time of writing:** `Rules/RecipeBookRules.cs` and `Tests/EditMode/RecipeBookRulesTests.cs` are untracked, and `UI/RecipeSelectUI.cs` is modified. This feature is uncommitted work in progress.

## Decisions

Each was put to the designer and chosen explicitly.

| # | Decision | Rationale |
|---|---|---|
| 1 | **uGUI + TextMeshPro**, not IMGUI | Real fonts, a book-spread layout, and art-ready slots. Precedent exists (`GameHUD` uses TextMeshPro), so no new dependency. |
| 2 | **Readable anywhere, brewable at a vat** | Serves "carry a mysterious damaged object from minute zero" — the player meets the burned back at minute 3 without owning a vat. |
| 3 | **Layout B: true spread, flip through** | Two facing pages, arrows to turn. The player travels *past* the torn gaps to reach the burned back, so the damage is experienced rather than listed. Chosen over an index+page layout, which scans faster but makes the book a menu. |
| 4 | **A dedicated `Menus` input map** | See "Input" below. |
| 5 | **Art stays placeholder** | `BuildPlan.md` Phase 2 is the "replace all placeholders" pass. Build art-ready with flat colours; real parchment and type are Phase 2's job. |

### Convention amendment

`AGENTS.md:181` currently reads: *"**UI is IMGUI.** New panels use `OnGUI`, not uGUI Canvas or UI Toolkit."*

This design deliberately breaks that rule. The implementation must amend `AGENTS.md` to record the exception and its scope: the recipe book is uGUI; other panels (`SellUI`, `RequestBookUI`, `DialogueUI`) remain IMGUI and are **not** in scope for conversion.

## Architecture

The project's existing split holds: decisions live in `Rules/` as pure C# and are tested without a scene; the view stays dumb.

### Rules layer — `Assets/Scripts/Rules/RecipeBookRules.cs`

Two additions to the existing file. Both are pure C# (no `UnityEngine` except `Mathf`, per the standing rule).

**Spread pagination.** `CompilePages` already returns ordered `BookPage`s. A new `CompileSpreads` groups them into facing pairs and always appends one final burned spread:

```csharp
public readonly struct BookSpread
{
    public readonly BookPage Left;
    public readonly BookPage Right;
    public readonly bool HasRight;        // false when the book has an odd page count
    public readonly bool IsBurnedSection; // the final spread, always present
}
```

`CompileSpreads(IReadOnlyList<BookPage> pages) -> List<BookSpread>`

The burned section occupies its own final spread across both pages — "the back of the book is burned through". It is always appended, even when there are zero recipes, because it is the cellar's seed and never depends on discovery.

**Page status.** The "is it locked / can I afford it / why not" logic currently sits untested inside `RecipeSelectUI.DrawLegiblePage`. It moves into a pure status type:

```csharp
public enum LockReason { None, RequiresBuilding, RequiresReputation }

public readonly struct PageStatus
{
    public readonly bool IsTorn;
    public readonly bool IsUnlocked;
    public readonly bool CanAfford;
    public readonly LockReason Reason;
    public readonly string RequiredBuildingId;  // null unless Reason == RequiresBuilding
    public readonly int RequiredReputation;     // 0 unless Reason == RequiresReputation
    public bool CanBrew => !IsTorn && IsUnlocked && CanAfford;
}
```

Built by:

```csharp
PageStatus StatusOf(BookPage page,
                    Func<RecipeData, bool> isUnlocked,
                    Func<ItemDef, int> getCount)
```

matching how `CompilePages` already takes `Func<RecipeData, bool> isDiscovered`.

The lock reason is carried as **data, not a formatted string** — `Rules` decides, the view formats. A torn page returns `IsTorn == true` and nothing else meaningful, so there is no path by which an undiscovered recipe's name or costs can reach the view.

### View layer — `Assets/Scripts/UI/`

Two focused files replace one file that currently does window chrome, layout, and three page states at once.

- **`RecipeBookUI.cs`** — owns the spread index, the mode, event subscription, and navigation. Knows nothing about how a page looks.
- **`RecipeBookPageView.cs`** — owns exactly one page. Serialized TextMeshPro references and a Brew button, with a single entry point:
  `Render(BookPage page, PageStatus status, bool showBrew)`

### Events — `Assets/Scripts/GameEvents.cs`

| Door | Event | Mode |
|---|---|---|
| Vat interaction (already exists) | `RecipeSelectionRequested(FermentVat)` | Brew |
| Key press (new) | `RecipeBookRequested()` | Read |

One new event. No manager calls the UI directly — the standing `GameEvents` rule is unchanged.

### Input — `Assets/Scripts/Input/InputSystem_Actions.inputactions`

A **new third action map, `Menus`**, containing a `RecipeBook` button action.

Rejected alternatives, with reasons:

- **Direct `Keyboard.current` read.** Matches the existing sloppy precedent (`DebugMenu` uses `pKey`; every panel does its own Escape check) but was explicitly rejected by the designer in favour of doing input properly.
- **Add to the `Player` map.** `PlayerController` implements `InputSystem_Actions.IPlayerActions`, so a new action forces a new callback into the locomotion controller — a menu concern in the wrong class. Worse, `PlayerController` gates on `IsMenuOpen`, so the key could open the book but never close it.

The `Menus` map stays enabled while a menu is open, so the key **toggles** — pressing it while the book is open closes it, in either mode. Escape also closes, matching every other panel.

**Default binding: `B`.** Chosen because `P` is taken by `DebugMenu` and `E` by dialogue advance. The binding lives in the asset and is trivial to change.

**Known inconsistency, deliberately not fixed here:** the other panels still read Escape directly rather than through the `UI/Cancel` action. Converting them is out of scope.

## Prefab structure

Its own canvas, not a child of `HUDCanvas`. `HUDCanvas` is Constant Pixel Size at an 800×600 reference; a full-screen modal book wants Scale With Screen Size and its own sorting order so it draws above the HUD.

```
RecipeBookCanvas          Canvas (sortingOrder above HUD), CanvasScaler, GraphicRaycaster
  Dimmer                  Image, full-screen, semi-transparent
  Book                    Image — the book background
    LeftPage              RecipeBookPageView
    RightPage             RecipeBookPageView
    BurnedPanel           shown only on the final spread
    PrevButton
    NextButton
    SpreadLabel           TMP — counts spreads, not pages: "Spread 2 of 4"
    CloseButton
```

## The two modes

| | Read (key) | Brew (vat) |
|---|---|---|
| Brew buttons | absent entirely | shown |
| Brewable page | reads as a page | button enabled |
| Locked or unaffordable | reads as a page | button disabled, with reason |
| On brew | — | `FermentManager.TryStartBatch`, then close |

Torn pages and the burned section render identically in both modes. They never carry an action.

Opening in either mode sets `PlayerController.Instance.IsMenuOpen = true`, and closing clears it — unchanged from current behaviour.

## What is deleted

`Assets/Scripts/UI/RecipeSelectUI.cs` is removed once `RecipeBookUI` replaces it.

## Testing

**EditMode — real coverage, no scene required:**

- `CompileSpreads`: pair grouping; odd page count leaves a trailing blank (`HasRight == false`); the burned spread is always last and always present, including with zero recipes; page numbers never reorder; a torn page holds its slot rather than collapsing.
- `PageStatus`: locked by building; locked by reputation; unlocked but unaffordable; fully brewable. **This logic has never been tested** — it is currently inline in the draw code.
- Navigation clamping: next-at-end stays put; prev-at-zero stays put.

**Not tested:** the uGUI rendering itself, consistent with how the IMGUI panels were treated. Verified by playtest.

**Playtest checklist:** the key opens and toggles the book anywhere · arrows flip spreads and clamp at both ends · a torn page shows damage and names nothing · the burned spread is reachable and is the last thing in the book · Brew buttons are absent in read mode · at a vat, an affordable unlocked recipe brews and closes the panel · Escape closes and restores movement.

## Out of scope

Each is separately justified, not merely deferred:

- **Converting other panels to uGUI.** `SellUI`, `RequestBookUI`, and `DialogueUI` stay IMGUI. This spec amends the convention narrowly rather than opening a project-wide migration.
- **Real art.** Phase 2 owns it.
- **Wiring the book to the cellar.** Thread #9's second half. The burned scraps are the seed; what they unlock is a later design.
- **Page-turn animation.** Spreads change instantly. Animation is polish and can be added without changing this architecture.
- **Routing existing panels' Escape through the `UI/Cancel` action.**

## Work requiring the Unity editor

The implementation cannot finish headless. A human is needed to:

1. Add the `Menus` action map and `RecipeBook` binding to the `.inputactions` asset.
2. Build the `RecipeBookCanvas` prefab and wire the serialized references.
3. Remove the `RecipeSelectUI` component from `HUDCanvas` and place the new prefab.
4. Run the playtest checklist.
