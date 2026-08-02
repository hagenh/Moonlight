# Act 0 corrections — the recipe book and Tormod's hours

**Date:** 2026-07-25
**Status:** Approved, not implemented.
**Covers:** items 1 and 2 of the player-order queue. Both land in the first fifteen minutes of the game.
**Related:** `Assets/Docs/GameDesign.md` thread #9, weakness ④, audit item 12.

Work is ordered by **when the player meets it**, not by size or dependency.

---

## Item 1 — The recipe book (thread #9)

### The idea

**Pages are recipes.** The book is not a lore prop sitting alongside the recipe system; it *is* the recipe system, given a diegetic frame. The player opens it every day to decide what to brew, and it has been damaged since before the game started.

The mystery is therefore not seeded by a note read once at minute three. It is seeded by **a gap in a menu the player uses constantly** — which is what makes it worth doing at all. Weakness ④ says a free setup is going unbuilt; this is the version that uses it.

### Current state

| Fact | Where |
|---|---|
| The player starts knowing exactly one recipe, Berry Shine | `FermentManager.cs:13` — `_discoveredRecipes = new() { "Berry Shine" }` |
| Five recipes exist, gated by building or reputation | `FermentManager.cs:29-51` |
| **Undiscovered recipes are hidden entirely** | `RecipeSelectUI.cs:74` — `if (!IsRecipeDiscovered(recipe)) continue;` |
| Discovered-but-locked recipes show a name and an unlock hint | `RecipeSelectUI.cs:79-93` |
| The book itself does not exist — no object, no asset, no fiction | Confirmed by search 2026-07-25 |

The "one legible page" is already in the code. It has no fiction attached to it.

### Design — three page states

| State | What the player sees | Becomes legible when |
|---|---|---|
| **Legible** | Full recipe: name, ingredients, time, yield. Current behaviour, unchanged | — (already discovered) |
| **Torn** | Damage only. **No name, no unlock hint** — you can see a page was there, not what was on it | Discovered through the existing mechanism |
| **Burned** | A block at the back of the book. A few legible scraps, nothing more. Visually distinct from torn | **Never, by any normal unlock.** This is the cellar's seed |

**Torn pages replace the current `continue`.** Undiscovered recipes stop being invisible and start being *visibly missing*.

Critically, a torn page shows **presence, not identity**. Revealing "Highland Mash — restore the Mill" at minute three would spend hook 3 (discovery) to buy hook 6 (the question). Showing four ruined pages spends nothing: the player learns the book is incomplete without learning what completes it.

### The burned section

Fixed content, present from minute zero, never restored by building anything. It is not a recipe and must never be mistaken for one — if a player thinks it is a locked recipe, they will wait for an unlock that isn't coming.

First-draft scraps, to be revised by whoever writes the fragments:

> `…the copper wants a slower fire than…`
> `…and we took the rest below, because…`
> `…if you are reading this then they did not…`

Job: name a place (*below*), imply people (*we*), imply an ending (*did not*). Three scraps, no more. The cellar pays this off; that payoff is **out of scope here** — the Mill is endgame and Holt-gated.

### Scope

**In:** the three page states, the burned section and its text, the recipe window reframed as the book.

**Out:** the cellar payoff · a physical book object in the tent or inventory · any art treatment beyond what IMGUI can express.

On that last point — `RecipeSelectUI` is IMGUI, and `LaterIdeas.md` already records that all non-debug UI needs a proper pass. **Ship the states in text now** (`~~~~ torn ~~~~`, a bordered burned block) and let the real treatment ride along with the UI pass. The design value is in the gap existing, not in how pretty it is yet.

The book as a carryable object is deliberately deferred. All the value is in the daily menu; the object adds fiction the menu already carries.

### Tests

- A new game shows one legible page and four torn ones
- A torn page leaks neither name nor unlock hint
- Discovering a recipe turns its torn page legible
- The burned section is present at minute zero and unaffected by every discovery and every building restore
- Existing `RecipeDiscoveryTests` still pass

---

## Item 2 — Tormod's hours

### Current state — worse than the design doc records

`GameDesign.md` records `tormodLeaveHour = -1` as the bug. The real problem is one level down:

| Fact | Where |
|---|---|
| `OnHourChanged` **is an empty stub** | `SellManager.cs:56-58` |
| Tormod is spawned unconditionally at startup and never removed | `SellManager.cs:31-35`, `SpawnTormod` |
| `tormodArriveHour = 8` — morning, not dusk | `SellManager.cs:11` |
| `tormodLeaveHour = -1` — never leaves | `SellManager.cs:12` |
| **`SpawnCart()` is never called from anywhere** | `SellManager.cs:60` — dead. `cartArriveHour` / `cartLeaveHour` unused |
| `RemoveCart` / `RemoveTormod` fire `OnSellerLeft` even when nothing was there | `SellManager.cs:74-75`, `111-112` |

Hour-based seller scheduling was never implemented. The `Start()` spawn is standing in for it. So this is not a two-constant edit — the scheduler has to be written.

### Design

- **`tormodArriveHour = 18`, `tormodLeaveHour = 6`.** `DayNightLighting` starts warming at 17:00 and is deep gold by 19:00, so 18:00 sits inside visible dusk — "dusk" is a thing the player can see, not a number

  > Originally specified as 17. Changed to 18 after reading the scene: `SampleScene.unity` **already serializes `tormodArriveHour: 18, tormodLeaveHour: 6`**. The designer values were never wrong — they were dead data waiting for a scheduler that was never written. Matching them avoids a pointless scene edit. The code defaults (`8` / `-1`) were the stale half, and are now aligned
- **Implement `OnHourChanged`** to spawn and remove sellers on their windows. Remove the `Start()` spawn
- **Fix the spurious `OnSellerLeft`** — fire it only when something was actually removed
- **Make the absence legible.** The back door is visible and shut all day; a lantern lights at 17:00

### Why this is worth doing now

The always-on Tormod is not merely off-spec — **it is eating the prologue's best moment.** If you can sell at 09:00, there is no reason to walk to town on day 1, and `GameDesign.md` calls that dark street of boarded windows *"the strongest single image in the game."* The bug costs Act 0 the thing it is built around.

### The teaching concern, and why the schedule survives it

Raised during design: if nobody is there when the first ferment finishes, will the player learn how selling works?

The clock says the risk is small. `realSecondsPerGameMinute = 0.77`, so a game hour is ~46 real seconds and the 08:00→24:00 day is ~12.3 real minutes.

| Game time | Real minute | Event |
|---|---|---|
| 08:00 | 0 | Wake |
| ~08:30 | ~0.4 | Ferment started |
| ~11:30 | ~2.7 | Jars in hand |
| 17:00 | ~6.9 | Light starts going golden |
| 18:00 | ~7.7 | Tormod arrives |
| 21:00 | ~10 | Earliest sleep permitted — `Bed.cs:9` |
| 24:00 | 12.3 | Curfew |

Two existing behaviours protect the player. `Bed.cs:9` refuses sleep before 21:00, so they **cannot skip past dusk** — they are awake through Tormod's window every day whether they intend to be or not. And that window is ~5.4 real minutes before curfew, the first ~3 of which are unskippable.

The ~4 real minutes between jars and dusk is not dead time. It is the doc's minute 5-15 exploration beat, and it is where the boarded windows land.

So the residual risk is **signposting, not scheduling**: does the player know to come back? That is what the shut door and the lantern are for. An empty spot teaches nothing; a closed door with a light that comes on teaches *"come back at X"* — hook 1, the game's proven hook, taught by a person instead of a jar.

### Deferred, deliberately

- **A day-1-only softener** (Tormod available all of day 1, dusk-only thereafter) is one flag away. **Do not build it before a playtester actually flounders.** Building it pre-emptively would re-break the exploration beat to solve a problem that may not exist
- **The traveling cart.** `SpawnCart` is dead and the cart appears nowhere in `GameDesign.md`'s economy table — it predates the redesign. Revive or delete is a real decision and it is **not made here.** This spec leaves it dead and flags it

### Tests

- Tormod is absent at 08:00 on a new game
- Tormod is present at 17:00 and at 23:00
- Tormod is absent at 07:00 the following morning
- `OnSellerLeft` does not fire for a seller that was never present
- Existing `TormodNailsGrantTests` still pass

---

## Not in this spec

Items 3-5 of the queue — the stand and request book, thread #4 (what is night for), thread #3 (the Constable). They come later in player order and each needs its own design pass.
