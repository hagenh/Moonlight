# Roadside Stand and Request Book Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the roadside stand and its request book — the primary economy — so that a player reads a book of written orders each morning, brews against it, and is paid a premium over passive shelf trade.

**Architecture:** Four pure-C# domain types in `Assets/Scripts/Rules/` carry all the logic (a request, the book of slots, pricing, arrival generation). A single `StandManager` singleton owns the book and bridges it to the clock and inventory through `GameEvents`. A `Stand` interactable opens an IMGUI panel. Nothing in `Rules/` touches UnityEngine, so the whole economy is testable in EditMode without a scene.

**Tech Stack:** Unity 6 (6000.2.14f1), URP 17.2.0, C#, NUnit via Unity Test Framework (`Lamplight.EditModeTests`, `Lamplight.PlayModeTests`, `Lamplight.TestSupport`).

## Global Constraints

- **No comments in code** unless explicitly requested (`AGENTS.md`). The existing `Rules/` files carry XML doc comments explaining *why* — match that, but add no inline `//` narration.
- **`Assets/Scripts/Rules/` must be pure C#.** No `UnityEngine` types except `Mathf`.
- **No direct cross-manager calls.** `StandManager` publishes via `GameEvents`; UI subscribes. Managers may call their own domain objects directly.
- **No frameworks, no ScriptableObjects, no DI.** Content is hardcoded in `ContentDb`; randomness goes through `IRng`.
- **UI is IMGUI** (`OnGUI` + `GUI.Window`), following `Assets/Scripts/UI/SellUI.cs`.
- Design guardrails in force (`GameDesign.md` Part 3): no loss anywhere at any hour · no hidden dice · appointments recur · **cozy is the genre, not a fallback**. Nothing in this plan may take cash, goods, or progress from the player.
- `docs/superpowers/` is untracked by convention. Do **not** `git add` anything under it.

## The settled numbers

From `GameDesign.md` Part 3, "The numbers — settled 2026-07-26". These are decided; do not re-derive them.

| | Value |
|---|---|
| Arrival rate | 2 notes per night, 3 once slot count reaches 5 |
| Request size | 1-3 batches' worth of the product |
| Active slots | 3 → 5 (stand upgrade) → 8 (storefront) |
| Premium | shelf 1.0× · exact request **1.8×** · descriptive request **2.2×** |
| Expiry | **None.** Declining frees the slot |

**One number is not in the design doc:** how often a request is descriptive rather than exact. `GameDesign.md` says only "a minority are descriptive." This plan uses **1 in 4**, as a named constant `RequestArrivalRules.DescriptiveInN`, so tuning is a one-line change. Flag it to the designer at review; do not silently treat it as settled design.

## Scope

**In scope:** the request domain, the book of slots, arrival at night, filling, declining, pricing, passive shelf trade, the stand interactable, and the book UI.

**Deliberately out of scope** — each is its own later plan:
- Replies and the correspondence voice (*"Better than the last batch."*)
- Customer-mix progression (strangers → mixed → named residents)
- Slot upgrades beyond the starting 3 (the API supports it; no upgrade grants it yet)
- The town storefront channel
- Stand art — placeholder sprite only, per the Phase 2 art rule

## File Structure

| File | Fate | Responsibility |
|---|---|---|
| `Assets/Scripts/Rules/StandRequest.cs` | Create | One immutable written order |
| `Assets/Scripts/Rules/RequestBookRules.cs` | Create | Pricing and fill-eligibility. Pure functions |
| `Assets/Scripts/Rules/RequestBook.cs` | Create | The slots and what occupies them |
| `Assets/Scripts/Rules/RequestArrivalRules.cs` | Create | How many notes arrive, and generating one |
| `Assets/Scripts/StandManager.cs` | Create | Singleton; owns the book, bridges clock and inventory |
| `Assets/Scripts/Stand.cs` | Create | `IInteractable`; opens the book |
| `Assets/Scripts/UI/RequestBookUI.cs` | Create | IMGUI panel |
| `Assets/Scripts/GameEvents.cs` | Modify | Five new events |
| `Assets/Tests/EditMode/RequestBookRulesTests.cs` | Create | Pricing and eligibility |
| `Assets/Tests/EditMode/RequestBookTests.cs` | Create | Slot behaviour |
| `Assets/Tests/EditMode/RequestArrivalRulesTests.cs` | Create | Arrival counts and generation |
| `Assets/Tests/PlayMode/StandFlowTests.cs` | Create | Manager wiring end to end |

**Naming caution:** `Assets/Scripts/Rules/RecipeBookRules.cs` already exists and is about the *grandfather's recipe book* — a different thing entirely. Never use "RecipeBook" for the stand's request book, and never use "RequestBook" for the recipe book.

---

### Task 1: The request and its pricing

Pure domain. No Unity, no scene, no manager.

**Files:**
- Create: `Assets/Scripts/Rules/StandRequest.cs`
- Create: `Assets/Scripts/Rules/RequestBookRules.cs`
- Test: `Assets/Tests/EditMode/RequestBookRulesTests.cs`

**Interfaces:**
- Consumes: `ItemDef` (`Assets/Scripts/ItemDef.cs`) — has `id`, `displayName`, `isIngredient`, `basePrice`, `isBottle`.
- Produces:
  - `enum RequestKind { Exact, Descriptive }`
  - `sealed class StandRequest` with `string Id`, `RequestKind Kind`, `IReadOnlyList<ItemDef> Accepts`, `int Units`, `string Signature`, `string Text`
  - `RequestBookRules.Payment(StandRequest, ItemDef delivered) -> int`
  - `RequestBookRules.Accepts(StandRequest, ItemDef) -> bool`
  - `RequestBookRules.ExactMultiplier = 1.8f`, `DescriptiveMultiplier = 2.2f`

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/RequestBookRulesTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class RequestBookRulesTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    private static readonly ItemDef Sweet = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);

    private static StandRequest Exact(ItemDef item, int units) =>
        new StandRequest("r1", RequestKind.Exact, new List<ItemDef> { item }, units, "A carter", "Four jars, if you have them.");

    private static StandRequest Descriptive(IEnumerable<ItemDef> items, int units) =>
        new StandRequest("r2", RequestKind.Descriptive, new List<ItemDef>(items), units, "Berta", "Something strong. It's for a wedding.");

    [Test]
    public void Payment_ExactRequest_AppliesExactMultiplier()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(108, RequestBookRules.Payment(request, Shine));
    }

    [Test]
    public void Payment_DescriptiveRequest_AppliesDescriptiveMultiplier()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.AreEqual(176, RequestBookRules.Payment(request, Sweet));
    }

    [Test]
    public void Payment_DescriptiveRequest_PricesWhatWasActuallyDelivered()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.AreEqual(66, RequestBookRules.Payment(request, Shine));
    }

    [Test]
    public void Payment_ItemNotAccepted_ReturnsZero()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(0, RequestBookRules.Payment(request, Sweet));
    }

    [Test]
    public void Payment_NullItem_ReturnsZero()
    {
        var request = Exact(Shine, 4);

        Assert.AreEqual(0, RequestBookRules.Payment(request, null));
    }

    [Test]
    public void Payment_AlwaysBeatsShelfPrice()
    {
        var request = Exact(Shine, 4);
        int shelf = Shine.basePrice * 4;

        Assert.Greater(RequestBookRules.Payment(request, Shine), shelf);
    }

    [Test]
    public void Accepts_ExactRequest_OnlyTheNamedItem()
    {
        var request = Exact(Shine, 4);

        Assert.IsTrue(RequestBookRules.Accepts(request, Shine));
        Assert.IsFalse(RequestBookRules.Accepts(request, Sweet));
    }

    [Test]
    public void Accepts_DescriptiveRequest_AnyListedItem()
    {
        var request = Descriptive(new[] { Shine, Sweet }, 2);

        Assert.IsTrue(RequestBookRules.Accepts(request, Shine));
        Assert.IsTrue(RequestBookRules.Accepts(request, Sweet));
    }

    [Test]
    public void Accepts_NullRequest_ReturnsFalse()
    {
        Assert.IsFalse(RequestBookRules.Accepts(null, Shine));
    }
}
```

The expected numbers: 15 × 4 × 1.8 = 108. 40 × 2 × 2.2 = 176. 15 × 2 × 2.2 = 66.

- [ ] **Step 2: Run the tests and watch them fail**

Run: `"C:/Program Files/Unity/Hub/Editor/6000.2.14f1/Editor/Unity.exe" -runTests -batchmode -projectPath . -testPlatform EditMode -testResults editmode.xml -logFile editmode.log`

Expected: compile failure — `StandRequest` and `RequestBookRules` do not exist. That is the correct first failure.

- [ ] **Step 3: Write `StandRequest`**

Create `Assets/Scripts/Rules/StandRequest.cs`:

```csharp
using System.Collections.Generic;

/// <summary>
/// Exact requests name one product. Descriptive requests — "something strong,
/// it's for a wedding" — name several the player may choose between, which is
/// what makes knowing your own recipes worth something.
/// </summary>
public enum RequestKind { Exact, Descriptive }

/// <summary>
/// One written order in the stand's book. Immutable: a note says what it says,
/// and the player either fills it, declines it, or leaves it sitting there.
///
/// There is no deadline field and there never will be. Requests do not expire —
/// the occupied slot is the whole of their cost. See GameDesign.md Part 3,
/// "Requests never expire — the slot is the cost".
/// </summary>
public sealed class StandRequest
{
    public readonly string Id;
    public readonly RequestKind Kind;
    public readonly IReadOnlyList<ItemDef> Accepts;
    public readonly int Units;
    public readonly string Signature;
    public readonly string Text;

    public StandRequest(string id, RequestKind kind, IReadOnlyList<ItemDef> accepts,
        int units, string signature, string text)
    {
        Id = id;
        Kind = kind;
        Accepts = accepts ?? new List<ItemDef>();
        Units = units;
        Signature = signature;
        Text = text;
    }
}
```

- [ ] **Step 4: Write `RequestBookRules`**

Create `Assets/Scripts/Rules/RequestBookRules.cs`:

```csharp
using UnityEngine;

/// <summary>
/// What a filled request pays, and what may fill it.
///
/// The premium over shelf price is the reason the book is the primary economy
/// rather than a side channel: passive shelf trade always works and always pays
/// 1.0×, so a request has to be worth the planning it costs.
/// </summary>
public static class RequestBookRules
{
    public const float ExactMultiplier = 1.8f;
    public const float DescriptiveMultiplier = 2.2f;

    public static bool Accepts(StandRequest request, ItemDef item)
    {
        if (request == null || item == null) return false;

        for (int i = 0; i < request.Accepts.Count; i++)
            if (request.Accepts[i] == item) return true;

        return false;
    }

    /// <summary>
    /// Prices what was actually delivered, not what was asked for. A descriptive
    /// request filled with the cheaper valid answer pays less — the player chose
    /// to spend less effort and is paid accordingly, with nothing taken away.
    /// </summary>
    public static int Payment(StandRequest request, ItemDef delivered)
    {
        if (!Accepts(request, delivered)) return 0;

        float multiplier = request.Kind == RequestKind.Descriptive
            ? DescriptiveMultiplier
            : ExactMultiplier;

        return Mathf.RoundToInt(delivered.basePrice * request.Units * multiplier);
    }
}
```

`Mathf` is the one UnityEngine type `Rules/` may use — see `EconomyRules.cs`, which already does this.

- [ ] **Step 5: Run the tests and watch them pass**

Run the same command as Step 2.
Expected: all 9 `RequestBookRulesTests` pass, and every pre-existing test still passes (baseline is EditMode 141/141).

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Rules/StandRequest.cs Assets/Scripts/Rules/StandRequest.cs.meta \
        Assets/Scripts/Rules/RequestBookRules.cs Assets/Scripts/Rules/RequestBookRules.cs.meta \
        Assets/Tests/EditMode/RequestBookRulesTests.cs Assets/Tests/EditMode/RequestBookRulesTests.cs.meta
git commit -m "Add the stand request type and its pricing

Exact requests pay 1.8x base, descriptive 2.2x, both against what was actually
delivered rather than what was asked for. Shelf trade stays at 1.0x, so the
premium is the reason to plan against the book at all."
```

---

### Task 2: The book of slots

**Depends on Task 1** for `StandRequest`.

**Files:**
- Create: `Assets/Scripts/Rules/RequestBook.cs`
- Test: `Assets/Tests/EditMode/RequestBookTests.cs`

**Interfaces:**
- Consumes: `StandRequest` from Task 1.
- Produces:
  - `sealed class RequestBook` with `int SlotCount { get; }`, `int FreeSlots { get; }`, `IReadOnlyList<StandRequest> Active { get; }`
  - `RequestBook(int slotCount)`
  - `bool TryPost(StandRequest)` — false when full or duplicate id
  - `StandRequest Take(string id)` — removes and returns, null if absent. Used for both filling and declining
  - `void SetSlotCount(int)` — for later upgrade tiers; never drops posted requests

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/RequestBookTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;

public class RequestBookTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);

    private static StandRequest Note(string id) =>
        new StandRequest(id, RequestKind.Exact, new List<ItemDef> { Shine }, 2, "A carter", "Two jars.");

    [Test]
    public void NewBook_StartsEmpty()
    {
        var book = new RequestBook(3);

        Assert.AreEqual(3, book.SlotCount);
        Assert.AreEqual(3, book.FreeSlots);
        Assert.AreEqual(0, book.Active.Count);
    }

    [Test]
    public void TryPost_IntoFreeSlot_Succeeds()
    {
        var book = new RequestBook(3);

        Assert.IsTrue(book.TryPost(Note("a")));
        Assert.AreEqual(2, book.FreeSlots);
    }

    [Test]
    public void TryPost_WhenFull_ReturnsFalse()
    {
        var book = new RequestBook(2);
        book.TryPost(Note("a"));
        book.TryPost(Note("b"));

        Assert.IsFalse(book.TryPost(Note("c")));
        Assert.AreEqual(2, book.Active.Count);
    }

    [Test]
    public void TryPost_DuplicateId_ReturnsFalse()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));

        Assert.IsFalse(book.TryPost(Note("a")));
        Assert.AreEqual(1, book.Active.Count);
    }

    [Test]
    public void TryPost_Null_ReturnsFalse()
    {
        var book = new RequestBook(3);

        Assert.IsFalse(book.TryPost(null));
    }

    [Test]
    public void Take_RemovesAndReturnsRequest()
    {
        var book = new RequestBook(3);
        var note = Note("a");
        book.TryPost(note);

        Assert.AreSame(note, book.Take("a"));
        Assert.AreEqual(0, book.Active.Count);
        Assert.AreEqual(3, book.FreeSlots);
    }

    [Test]
    public void Take_UnknownId_ReturnsNull()
    {
        var book = new RequestBook(3);

        Assert.IsNull(book.Take("nope"));
    }

    [Test]
    public void Take_FreesTheSlotForANewNote()
    {
        var book = new RequestBook(1);
        book.TryPost(Note("a"));
        Assert.IsFalse(book.TryPost(Note("b")));

        book.Take("a");

        Assert.IsTrue(book.TryPost(Note("b")));
    }

    [Test]
    public void SetSlotCount_Grows_AddsFreeSlots()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));

        book.SetSlotCount(5);

        Assert.AreEqual(5, book.SlotCount);
        Assert.AreEqual(4, book.FreeSlots);
    }

    [Test]
    public void SetSlotCount_Shrinking_NeverDiscardsPostedRequests()
    {
        var book = new RequestBook(3);
        book.TryPost(Note("a"));
        book.TryPost(Note("b"));
        book.TryPost(Note("c"));

        book.SetSlotCount(1);

        Assert.AreEqual(3, book.Active.Count);
        Assert.AreEqual(0, book.FreeSlots);
    }
}
```

That last test is guardrail 1 in test form: nothing the player was offered is ever taken back, even by a shrinking book.

- [ ] **Step 2: Run the tests and watch them fail**

Run: the EditMode command from Task 1 Step 2.
Expected: compile failure — `RequestBook` does not exist.

- [ ] **Step 3: Write `RequestBook`**

Create `Assets/Scripts/Rules/RequestBook.cs`:

```csharp
using System.Collections.Generic;

/// <summary>
/// The notes currently pinned in the book, and how many may be pinned at once.
///
/// The slot count is the game's only source of pressure on the request economy:
/// notes never expire, so a request the player will not fill occupies a slot
/// that no new note can use. Ignoring a request costs the demand you did not get
/// to see instead — never anything the player already had.
/// </summary>
public sealed class RequestBook
{
    private readonly List<StandRequest> _active = new();
    private int _slotCount;

    public RequestBook(int slotCount)
    {
        _slotCount = slotCount < 0 ? 0 : slotCount;
    }

    public int SlotCount => _slotCount;

    public IReadOnlyList<StandRequest> Active => _active;

    /// <summary>
    /// Never negative. A shrunk book reports zero free slots rather than a
    /// deficit, because the overhang is resolved by the player filling notes.
    /// </summary>
    public int FreeSlots
    {
        get
        {
            int free = _slotCount - _active.Count;
            return free < 0 ? 0 : free;
        }
    }

    public bool TryPost(StandRequest request)
    {
        if (request == null) return false;
        if (FreeSlots <= 0) return false;
        if (Find(request.Id) != null) return false;

        _active.Add(request);
        return true;
    }

    public StandRequest Take(string id)
    {
        var found = Find(id);
        if (found == null) return null;

        _active.Remove(found);
        return found;
    }

    /// <summary>
    /// Shrinking never discards a posted note. Guardrail 1 is unconditional, and
    /// a note already offered is something the player has.
    /// </summary>
    public void SetSlotCount(int slotCount)
    {
        _slotCount = slotCount < 0 ? 0 : slotCount;
    }

    private StandRequest Find(string id)
    {
        if (id == null) return null;

        for (int i = 0; i < _active.Count; i++)
            if (_active[i].Id == id) return _active[i];

        return null;
    }
}
```

- [ ] **Step 4: Run the tests and watch them pass**

Run: the EditMode command.
Expected: all 10 `RequestBookTests` pass, plus Task 1's 9, plus the pre-existing 141.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/RequestBook.cs Assets/Scripts/Rules/RequestBook.cs.meta \
        Assets/Tests/EditMode/RequestBookTests.cs Assets/Tests/EditMode/RequestBookTests.cs.meta
git commit -m "Add the request book's slots

Notes never expire, so the occupied slot is the cost of ignoring one: new notes
arrive only into free slots. Shrinking the book never discards a posted note,
which is guardrail 1 in test form."
```

---

### Task 3: Arrival

**Depends on Tasks 1 and 2.**

**Files:**
- Create: `Assets/Scripts/Rules/RequestArrivalRules.cs`
- Test: `Assets/Tests/EditMode/RequestArrivalRulesTests.cs`

**Interfaces:**
- Consumes: `StandRequest`, `RequestKind` (Task 1); `RecipeData` (`Assets/Scripts/RecipeData.cs`) which has `recipeName`, `fermentationHours`, `outputCount`, `outputItem`; `IRng` (`Assets/Scripts/Rules/IRng.cs`) with `Value01()`, `Range(int,int)`, `Range(float,float)`.
- Produces:
  - `RequestArrivalRules.NotesPerNight(int slotCount) -> int`
  - `RequestArrivalRules.Generate(IReadOnlyList<RecipeData> available, IRng rng, string id) -> StandRequest`
  - Constants `MinBatches = 1`, `MaxBatches = 3`, `DescriptiveInN = 4`

**Test RNG note:** `StubRng` returns queued values and defaults to 0. `Range(int minInclusive, int maxExclusive)` returning 0 means it does not respect the min — check `Assets/Tests/Shared/` for the exact stub behaviour before writing expectations, and prefer `SeededRng` where a specific draw matters less than determinism.

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/RequestArrivalRulesTests.cs`:

```csharp
using System.Collections.Generic;
using Lamplight.TestSupport;
using NUnit.Framework;

public class RequestArrivalRulesTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    private static readonly ItemDef Sweet = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);

    private static List<RecipeData> TwoRecipes() => new()
    {
        new RecipeData("Berry Shine", 3, 2, Shine),
        new RecipeData("Sweet Batch", 6, 4, Sweet)
    };

    [Test]
    public void NotesPerNight_ThreeSlots_ReturnsTwo()
    {
        Assert.AreEqual(2, RequestArrivalRules.NotesPerNight(3));
    }

    [Test]
    public void NotesPerNight_FiveSlots_ReturnsThree()
    {
        Assert.AreEqual(3, RequestArrivalRules.NotesPerNight(5));
    }

    [Test]
    public void NotesPerNight_EightSlots_ReturnsThree()
    {
        Assert.AreEqual(3, RequestArrivalRules.NotesPerNight(8));
    }

    [Test]
    public void Generate_NoRecipes_ReturnsNull()
    {
        Assert.IsNull(RequestArrivalRules.Generate(new List<RecipeData>(), new SeededRng(1), "r1"));
    }

    [Test]
    public void Generate_NullRecipes_ReturnsNull()
    {
        Assert.IsNull(RequestArrivalRules.Generate(null, new SeededRng(1), "r1"));
    }

    [Test]
    public void Generate_UnitsAreAWholeNumberOfBatches()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");

            int outputCount = request.Accepts[0] == Shine ? 2 : 4;
            Assert.AreEqual(0, request.Units % outputCount,
                $"seed {seed}: {request.Units} units is not a whole number of {outputCount}-jar batches");

            int batches = request.Units / outputCount;
            Assert.GreaterOrEqual(batches, RequestArrivalRules.MinBatches);
            Assert.LessOrEqual(batches, RequestArrivalRules.MaxBatches);
        }
    }

    [Test]
    public void Generate_ExactRequest_AcceptsExactlyOneProduct()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");
            if (request.Kind != RequestKind.Exact) continue;

            Assert.AreEqual(1, request.Accepts.Count);
        }
    }

    [Test]
    public void Generate_DescriptiveRequest_AcceptsMoreThanOneProduct()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");
            if (request.Kind != RequestKind.Descriptive) continue;

            Assert.Greater(request.Accepts.Count, 1);
        }
    }

    [Test]
    public void Generate_SingleRecipe_NeverProducesADescriptiveRequest()
    {
        var one = new List<RecipeData> { new RecipeData("Berry Shine", 3, 2, Shine) };

        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(one, new SeededRng(seed), $"r{seed}");

            Assert.AreEqual(RequestKind.Exact, request.Kind);
        }
    }

    [Test]
    public void Generate_UsesTheGivenId()
    {
        var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(1), "day3-note0");

        Assert.AreEqual("day3-note0", request.Id);
    }

    [Test]
    public void Generate_AlwaysCarriesSignatureAndText()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");

            Assert.IsNotEmpty(request.Signature);
            Assert.IsNotEmpty(request.Text);
        }
    }

    [Test]
    public void Generate_IsDeterministicForASeed()
    {
        var a = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(7), "r");
        var b = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(7), "r");

        Assert.AreEqual(a.Kind, b.Kind);
        Assert.AreEqual(a.Units, b.Units);
        Assert.AreEqual(a.Signature, b.Signature);
    }
}
```

- [ ] **Step 2: Run the tests and watch them fail**

Run: the EditMode command.
Expected: compile failure — `RequestArrivalRules` does not exist.

- [ ] **Step 3: Write `RequestArrivalRules`**

Create `Assets/Scripts/Rules/RequestArrivalRules.cs`:

```csharp
using System.Collections.Generic;

/// <summary>
/// What arrives in the book overnight.
///
/// Notes are written while the player sleeps and read in the morning, which is
/// the mechanical half of "day is when you act, night is when the day answers
/// back". Generation only ever draws on recipes the player can already make, so
/// the book never asks for something unreachable — descriptive requests point at
/// the next unlock, and that is a later plan's job, not this one's.
/// </summary>
public static class RequestArrivalRules
{
    public const int MinBatches = 1;
    public const int MaxBatches = 3;

    /// <summary>
    /// One request in this many is descriptive. GameDesign.md says only "a
    /// minority are descriptive" and does not fix the fraction; this is the
    /// tuning knob, not a settled design number.
    /// </summary>
    public const int DescriptiveInN = 4;

    private static readonly string[] Signatures =
    {
        "A carter", "A traveller", "Someone passing", "A woman from the valley road",
        "No name given", "A man who did not stay"
    };

    private static readonly string[] ExactTexts =
    {
        "Leave them under the bench. I'll settle up.",
        "The same again, if you have it.",
        "I'll be back this way inside the week.",
        "For my brother. He asked where I got the last."
    };

    private static readonly string[] DescriptiveTexts =
    {
        "Something strong. It's for a wedding.",
        "Whatever you'd drink yourself.",
        "Something to keep the cold out.",
        "Your best. It's an apology."
    };

    public static int NotesPerNight(int slotCount) => slotCount >= 5 ? 3 : 2;

    public static StandRequest Generate(IReadOnlyList<RecipeData> available, IRng rng, string id)
    {
        if (available == null || available.Count == 0 || rng == null) return null;

        bool descriptive = available.Count > 1 && rng.Range(0, DescriptiveInN) == 0;
        int batches = rng.Range(MinBatches, MaxBatches + 1);

        if (descriptive)
        {
            var accepts = new List<ItemDef>();
            for (int i = 0; i < available.Count; i++)
                if (available[i]?.outputItem != null) accepts.Add(available[i].outputItem);

            if (accepts.Count < 2) return ExactRequest(available, rng, id, batches);

            int units = batches * available[0].outputCount;

            return new StandRequest(id, RequestKind.Descriptive, accepts, units,
                Pick(Signatures, rng), Pick(DescriptiveTexts, rng));
        }

        return ExactRequest(available, rng, id, batches);
    }

    private static StandRequest ExactRequest(IReadOnlyList<RecipeData> available, IRng rng, string id, int batches)
    {
        var recipe = available[rng.Range(0, available.Count)];
        int units = batches * recipe.outputCount;

        return new StandRequest(id, RequestKind.Exact,
            new List<ItemDef> { recipe.outputItem }, units,
            Pick(Signatures, rng), Pick(ExactTexts, rng));
    }

    private static string Pick(string[] pool, IRng rng) => pool[rng.Range(0, pool.Length)];
}
```

**Watch out:** the descriptive branch sizes units from `available[0].outputCount`, so a descriptive request is a whole number of batches of the *first* recipe. That is what `Generate_UnitsAreAWholeNumberOfBatches` checks via `request.Accepts[0]`. If you restructure the branch, keep those two consistent or the test lies.

- [ ] **Step 4: Run the tests and watch them pass**

Run: the EditMode command.
Expected: all 12 `RequestArrivalRulesTests` pass. If `SeededRng` does not exist under `Assets/Tests/Shared/`, stop and check what the project actually provides before inventing one.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Rules/RequestArrivalRules.cs Assets/Scripts/Rules/RequestArrivalRules.cs.meta \
        Assets/Tests/EditMode/RequestArrivalRulesTests.cs Assets/Tests/EditMode/RequestArrivalRulesTests.cs.meta
git commit -m "Add overnight request arrival

Two notes a night, three once the book grows to five slots. Requests are sized
in whole batches so they stay honest as recipes get slower, and one in four is
descriptive. Generation only draws on recipes the player can already make."
```

---

### Task 4: `StandManager` and the events

**Depends on Tasks 1-3.** This is where the domain meets the game.

**Files:**
- Create: `Assets/Scripts/StandManager.cs`
- Modify: `Assets/Scripts/GameEvents.cs`
- Test: `Assets/Tests/PlayMode/StandFlowTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 1-3; `InventoryManager.Instance` (`GetCount`, `TryAdd`, `TryRemove`); `GameManager.Instance.AddCash(int)`; `FermentManager` for the discovered-recipe list.
- Produces:
  - `StandManager.Instance`
  - `StandManager.Book -> RequestBook`
  - `bool TryFill(string requestId, ItemDef with)`
  - `bool Decline(string requestId)`
  - `void StockShelf(ItemDef, int)` / `int ShelfCount(ItemDef)`
  - `internal void SetRng(IRng)` — matches `SellManager.SetRng`
  - New `GameEvents`: `RequestPosted(StandRequest)`, `RequestFilled(StandRequest, int)`, `RequestDeclined(StandRequest)`, `ShelfSold(ItemDef, int, int)`, `RequestBookRequested()`

- [ ] **Step 1: Add the events**

In `Assets/Scripts/GameEvents.cs`, add after the `DeliveryMade` declaration:

```csharp
    public static event System.Action<StandRequest> RequestPosted;
    public static event System.Action<StandRequest, int> RequestFilled;
    public static event System.Action<StandRequest> RequestDeclined;
    public static event System.Action<ItemDef, int, int> ShelfSold;
    public static event System.Action RequestBookRequested;
```

And after the `OnDeliveryMade` invoker:

```csharp
    public static void OnRequestPosted(StandRequest request)
        => RequestPosted?.Invoke(request);

    public static void OnRequestFilled(StandRequest request, int payment)
        => RequestFilled?.Invoke(request, payment);

    public static void OnRequestDeclined(StandRequest request)
        => RequestDeclined?.Invoke(request);

    public static void OnShelfSold(ItemDef item, int count, int payment)
        => ShelfSold?.Invoke(item, count, payment);

    public static void OnRequestBookRequested()
        => RequestBookRequested?.Invoke();
```

`GameEventsReset.ClearAll()` reflects over every static field, so it needs no edit — verify with `grep -rn "Request\|Shelf" Assets/Tests/Shared/` returning nothing.

- [ ] **Step 2: Write the failing tests**

Create `Assets/Tests/PlayMode/StandFlowTests.cs`:

```csharp
using System.Collections;
using System.Collections.Generic;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class StandFlowTests
{
    private InventoryManager _inventory;
    private GameManager _game;
    private StandManager _stand;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _game = TestBootstrap.CreateSingleton<GameManager>();
        _stand = TestBootstrap.CreateSingleton<StandManager>();
        _stand.SetRng(new SeededRng(1));
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    private static StandRequest Note(string id, ItemDef item, int units) =>
        new StandRequest(id, RequestKind.Exact, new List<ItemDef> { item }, units, "A carter", "Two jars.");

    [UnityTest]
    public IEnumerator Book_StartsWithThreeSlots()
    {
        Assert.AreEqual(3, _stand.Book.SlotCount);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DayEnded_PostsTwoNotes()
    {
        GameEvents.OnDayEnded(1);

        Assert.AreEqual(2, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DayEnded_NeverOverfillsTheBook()
    {
        GameEvents.OnDayEnded(1);
        GameEvents.OnDayEnded(2);
        GameEvents.OnDayEnded(3);

        Assert.AreEqual(3, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WithEnoughStock_PaysThePremiumAndClearsTheSlot()
    {
        var note = Note("a", ContentDb.BerryShine, 4);
        _stand.Book.TryPost(note);
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;

        bool filled = _stand.TryFill("a", ContentDb.BerryShine);

        Assert.IsTrue(filled);
        Assert.AreEqual(cashBefore + 108, _game.Cash);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(0, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WithoutEnoughStock_TakesNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 4));
        _inventory.TryAdd(ContentDb.BerryShine, 2);
        int cashBefore = _game.Cash;

        bool filled = _stand.TryFill("a", ContentDb.BerryShine);

        Assert.IsFalse(filled);
        Assert.AreEqual(cashBefore, _game.Cash);
        Assert.AreEqual(2, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(1, _stand.Book.Active.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TryFill_WrongProduct_TakesNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 2));
        _inventory.TryAdd(ContentDb.SweetMoonshine, 10);

        Assert.IsFalse(_stand.TryFill("a", ContentDb.SweetMoonshine));
        Assert.AreEqual(10, _inventory.GetCount(ContentDb.SweetMoonshine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Decline_FreesTheSlotAndCostsNothing()
    {
        _stand.Book.TryPost(Note("a", ContentDb.BerryShine, 4));
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;

        bool declined = _stand.Decline("a");

        Assert.IsTrue(declined);
        Assert.AreEqual(0, _stand.Book.Active.Count);
        Assert.AreEqual(cashBefore, _game.Cash);
        Assert.AreEqual(4, _inventory.GetCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Shelf_SellsStockAtBasePriceOnDayEnd()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 3);
        _stand.StockShelf(ContentDb.BerryShine, 3);
        int cashBefore = _game.Cash;

        GameEvents.OnDayEnded(1);

        Assert.AreEqual(cashBefore + 45, _game.Cash);
        Assert.AreEqual(0, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Shelf_PaysLessThanTheSameGoodsAgainstARequest()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 4);
        _stand.StockShelf(ContentDb.BerryShine, 4);
        int cashBefore = _game.Cash;
        GameEvents.OnDayEnded(1);
        int shelfEarnings = _game.Cash - cashBefore;

        Assert.Less(shelfEarnings, 108);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StockShelf_MovesGoodsOutOfInventory()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 5);

        _stand.StockShelf(ContentDb.BerryShine, 2);

        Assert.AreEqual(3, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(2, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }

    [UnityTest]
    public IEnumerator StockShelf_MoreThanHeld_StocksNothing()
    {
        _inventory.TryAdd(ContentDb.BerryShine, 1);

        _stand.StockShelf(ContentDb.BerryShine, 5);

        Assert.AreEqual(1, _inventory.GetCount(ContentDb.BerryShine));
        Assert.AreEqual(0, _stand.ShelfCount(ContentDb.BerryShine));
        yield return null;
    }
}
```

Expected numbers: 15 × 4 × 1.8 = 108 for the request, 15 × 3 = 45 for the shelf.

- [ ] **Step 3: Run the tests and watch them fail**

Run: `"C:/Program Files/Unity/Hub/Editor/6000.2.14f1/Editor/Unity.exe" -runTests -batchmode -projectPath . -testPlatform PlayMode -testResults playmode.xml -logFile playmode.log`

Expected: compile failure — `StandManager` does not exist.

- [ ] **Step 4: Write `StandManager`**

Create `Assets/Scripts/StandManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The roadside stand: a shelf that sells whatever is left on it, and a book of
/// written orders that pay a premium for making what was asked.
///
/// The player is never summoned here. Shelf trade resolves itself overnight and
/// notes wait indefinitely, so the stand fits Act 0's proven shape — start
/// something, go do something else.
/// </summary>
public class StandManager : MonoBehaviour
{
    public static StandManager Instance { get; private set; }

    [SerializeField] private int startingSlots = 3;

    private readonly Dictionary<ItemDef, int> _shelf = new();
    private RequestBook _book;
    private IRng _rng = UnityRng.Instance;
    private int _noteSequence;

    public RequestBook Book => _book;

    internal void SetRng(IRng rng) => _rng = rng;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _book = new RequestBook(startingSlots);
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
        SellShelf();
        PostNightNotes(day);
    }

    private void SellShelf()
    {
        if (GameManager.Instance == null || _shelf.Count == 0) return;

        var sold = new List<KeyValuePair<ItemDef, int>>(_shelf);
        _shelf.Clear();

        foreach (var entry in sold)
        {
            int payment = entry.Key.basePrice * entry.Value;
            GameManager.Instance.AddCash(payment);
            GameEvents.OnShelfSold(entry.Key, entry.Value, payment);
        }
    }

    private void PostNightNotes(int day)
    {
        var available = AvailableRecipes();
        if (available.Count == 0) return;

        int wanted = RequestArrivalRules.NotesPerNight(_book.SlotCount);

        for (int i = 0; i < wanted; i++)
        {
            if (_book.FreeSlots <= 0) return;

            var request = RequestArrivalRules.Generate(available, _rng, $"day{day}-note{_noteSequence++}");
            if (request == null) return;

            if (_book.TryPost(request))
                GameEvents.OnRequestPosted(request);
        }
    }

    private List<RecipeData> AvailableRecipes()
    {
        var list = new List<RecipeData>();
        if (FermentManager.Instance == null)
        {
            list.Add(new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine));
            return list;
        }

        foreach (var recipe in FermentManager.Instance.DiscoveredRecipes)
            if (recipe?.outputItem != null) list.Add(recipe);

        return list;
    }

    public bool TryFill(string requestId, ItemDef with)
    {
        if (InventoryManager.Instance == null || GameManager.Instance == null) return false;

        var request = FindActive(requestId);
        if (request == null) return false;
        if (!RequestBookRules.Accepts(request, with)) return false;
        if (!InventoryManager.Instance.Has(with, request.Units)) return false;

        if (!InventoryManager.Instance.TryRemove(with, request.Units)) return false;

        _book.Take(requestId);
        int payment = RequestBookRules.Payment(request, with);
        GameManager.Instance.AddCash(payment);
        GameEvents.OnRequestFilled(request, payment);
        return true;
    }

    /// <summary>
    /// Declining costs nothing and is the intended way to clear a note the player
    /// cannot or will not fill. It exists so an unfillable request can never
    /// occupy a slot permanently.
    /// </summary>
    public bool Decline(string requestId)
    {
        var request = _book.Take(requestId);
        if (request == null) return false;

        GameEvents.OnRequestDeclined(request);
        return true;
    }

    public void StockShelf(ItemDef item, int count)
    {
        if (item == null || count <= 0 || InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.TryRemove(item, count)) return;

        _shelf[item] = ShelfCount(item) + count;
    }

    public int ShelfCount(ItemDef item)
    {
        if (item == null) return 0;
        return _shelf.GetValueOrDefault(item, 0);
    }

    private StandRequest FindActive(string id)
    {
        foreach (var request in _book.Active)
            if (request.Id == id) return request;

        return null;
    }
}
```

**`FermentManager.DiscoveredRecipes` may not exist under that name.** Before writing this, run `grep -n "public" Assets/Scripts/FermentManager.cs` and use whatever the discovered-recipe accessor actually is. If there is none, add a minimal read-only one rather than reaching into private state.

- [ ] **Step 5: Run the tests and watch them pass**

Run: the PlayMode command, then the EditMode command.
Expected: all 11 `StandFlowTests` pass; EditMode still fully green.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/StandManager.cs Assets/Scripts/StandManager.cs.meta \
        Assets/Scripts/GameEvents.cs \
        Assets/Tests/PlayMode/StandFlowTests.cs Assets/Tests/PlayMode/StandFlowTests.cs.meta
git commit -m "Add StandManager: shelf trade and the request book

Shelf stock clears overnight at base price; notes arrive the same moment and
only into free slots. Filling pays the premium and takes exactly the units
asked for. Declining is free and takes nothing, which is what stops an
unfillable note occupying a slot forever."
```

---

### Task 5: The stand interactable and the book UI

**Depends on Task 4.**

**Files:**
- Create: `Assets/Scripts/Stand.cs`
- Create: `Assets/Scripts/UI/RequestBookUI.cs`
- Modify: `Assets/Scripts/InteractType.cs` — add a `Stand` member if the enum does not already have a fitting one (check first; `Forage`, `Build` and others exist)

**Interfaces:**
- Consumes: `StandManager.Instance`, `GameEvents.RequestBookRequested`, `IInteractable`.
- Produces: a `Stand` component with a `Create(Vector3)` factory, matching `BerryBush.Create`.

- [ ] **Step 1: Check the interact enum**

Run: `cat Assets/Scripts/InteractType.cs`. If there is no member that fits a stand, add `Stand`. Do not rename existing members.

- [ ] **Step 2: Write `Stand`**

Create `Assets/Scripts/Stand.cs`:

```csharp
using UnityEngine;

public class Stand : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Stand;

    public void Interact()
    {
        GameEvents.OnRequestBookRequested();
    }

    public static Stand Create(Vector3 position)
    {
        var go = new GameObject("Stand");
        go.transform.position = position;

        var tex = new Texture2D(16, 16);
        var pixels = new Color32[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        sr.color = new Color(0.75f, 0.6f, 0.35f);
        sr.sortingOrder = 5;

        var solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(1.2f, 0.6f);

        var trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.6f, 1.2f);

        go.layer = LayerMask.NameToLayer("Interactable");
        go.AddComponent<Stand>();

        return go.GetComponent<Stand>();
    }
}
```

The white-texture-plus-tint is the established placeholder pattern (`BerryBush.Create`) and Phase 2 replaces it. Do not commission art here.

- [ ] **Step 3: Write `RequestBookUI`**

Create `Assets/Scripts/UI/RequestBookUI.cs`, following `SellUI.cs` exactly for window handling:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class RequestBookUI : MonoBehaviour
{
    private bool _visible;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 420, 460);

    private void OnEnable()
    {
        GameEvents.RequestBookRequested += OnRequestBookRequested;
        GameEvents.MenuCloseRequested += Close;
    }

    private void OnDisable()
    {
        GameEvents.RequestBookRequested -= OnRequestBookRequested;
        GameEvents.MenuCloseRequested -= Close;
    }

    private void OnRequestBookRequested()
    {
        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
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
        _windowRect = GUI.Window(3, _windowRect, DrawWindow, "The Request Book");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (StandManager.Instance == null) return;

        var book = StandManager.Instance.Book;
        GUILayout.Label($"{book.Active.Count} of {book.SlotCount} slots used");
        GUILayout.Space(6);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(360));

        if (book.Active.Count == 0)
            GUILayout.Label("Nothing yet. Notes arrive overnight.");

        for (int i = book.Active.Count - 1; i >= 0; i--)
        {
            var request = book.Active[i];
            DrawRequest(request);
            GUILayout.Space(10);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }

    private void DrawRequest(StandRequest request)
    {
        GUILayout.Label($"\"{request.Text}\"");
        GUILayout.Label($"— {request.Signature}");

        foreach (var item in request.Accepts)
        {
            int have = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetCount(item) : 0;
            int payment = RequestBookRules.Payment(request, item);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{request.Units}x {item.displayName}  ({payment}g)  have: {have}",
                GUILayout.Width(280));

            GUI.enabled = have >= request.Units;
            if (GUILayout.Button("Fill", GUILayout.Width(60)))
                StandManager.Instance.TryFill(request.Id, item);
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Decline", GUILayout.Width(80)))
            StandManager.Instance.Decline(request.Id);
    }
}
```

Iterating backwards matters: `TryFill` and `Decline` mutate `book.Active` during the loop.

`GUI.Window` id `3` — `SellUI` uses `2`. Check `DialogueUI` and `RecipeSelectUI` for collisions before settling on a number.

- [ ] **Step 4: Verify by playing, not by test**

There is no automated test for IMGUI in this project and this plan does not add one. Open the scene, place a `Stand` near the homestead, sleep once, and confirm: two notes appear · the book shows text, signature, price and stock · Fill is disabled without stock and works with it · Decline clears the note · cash moves by the amount shown.

**Do not mark this step done on a compile check.** If you cannot run the editor, stop and hand back.

- [ ] **Step 5: Run both suites**

Run the EditMode and PlayMode commands.
Expected: everything green. The UI is untested by design, but it must not break anything.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Stand.cs Assets/Scripts/Stand.cs.meta \
        Assets/Scripts/UI/RequestBookUI.cs Assets/Scripts/UI/RequestBookUI.cs.meta \
        Assets/Scripts/InteractType.cs
git commit -m "Add the stand interactable and the request book panel

Placeholder art per the Phase 2 rule. The panel shows each note's text and
signature, what it pays, and whether the player can fill it, with Decline
always available."
```

---

### Task 6: Place the stand and record the state

**Depends on Task 5.** Editor work plus documentation.

**Files:**
- Modify: `Assets/Scenes/SampleScene.unity` (via the Unity editor)
- Modify: `Assets/Docs/BuildPlan.md`

- [ ] **Step 1: Place the stand in the scene**

In the Unity editor, add a `Stand` and a `StandManager` to `SampleScene`. Put the stand on the camp clearing, roadside, near the homestead build site — `GameDesign.md` Part 3, "Placement". Add a `RequestBookUI` component alongside the other UI panels.

Save the scene.

- [ ] **Step 2: Tick off what Phase S actually delivered**

In `Assets/Docs/BuildPlan.md` under `## Phase S`, change to `[x]` only the bullets this plan genuinely completed: the stand on the homestead site, passive shelf trade, the request book with written notes, exact-and-descriptive requests, no expiry, and the settled numbers.

Leave unticked and untouched: replies and signed correspondence beyond the placeholder strings, the customer-mix progression, capacity upgrades, the storefront, and appointments. Add a line recording that they are deferred, so the next reader knows Phase S is partly done rather than done.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scenes/SampleScene.unity Assets/Docs/BuildPlan.md
git commit -m "Place the stand in SampleScene and mark Phase S partly done

Shelf trade, the request book, arrival, filling and declining are in. Replies,
the customer-mix progression, slot upgrades and the storefront are not, and
Phase S is annotated to say so."
```

---

## Self-Review

**Spec coverage.** Against `GameDesign.md` Part 3, "The stand and the request book": Placement → Task 6. Attendance/passive shelf → Task 4. Requests as written notes, no queue → Tasks 3 and 5. Exact with descriptive spikes → Tasks 1 and 3. No expiry, decline frees the slot → Tasks 2 and 4. Capacity → `RequestBook.SetSlotCount` exists in Task 2, unused until an upgrade grants it, flagged as out of scope. The numbers → Task 3 constants and Task 1 multipliers.

**Deliberate gaps, all declared in Scope:** the correspondence voice (placeholder strings only), the strangers→residents progression, the storefront, appointments as demand events. Each needs writing and content systems this plan does not build, and each is worth its own plan.

**Invented number, flagged:** `DescriptiveInN = 4`. The design says "a minority" and no more. It is a named constant and the plan tells the reviewer it is a guess.

**Placeholder scan.** Every code step contains real code. Three steps deliberately tell the implementer to *check before writing* rather than assume — `FermentManager`'s discovered-recipe accessor, `InteractType`'s members, and `GUI.Window` id collisions. Those are verification instructions with a named command, not placeholders.

**Type consistency.** `StandRequest` fields (`Id`, `Kind`, `Accepts`, `Units`, `Signature`, `Text`) are used identically in Tasks 1, 2, 3, 4 and 5. `RequestBook.Take` is the single removal path for both filling and declining, used under that name in Tasks 2 and 4. `RequestBookRules.Payment` and `.Accepts` keep their signatures across Tasks 1, 4 and 5.

**Known risk.** Task 6 Step 1 is manual Unity-editor work and cannot be done by an agent; the plan says to hand back. Task 5 Step 4 likewise needs a human at the editor, because this project has no IMGUI test harness.
