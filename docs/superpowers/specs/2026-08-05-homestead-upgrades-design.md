# Homestead Site Upgrades — Design

**Date:** 2026-08-05
**Status:** Approved in session. Folded into `Assets/Docs/GameDesign.md` (Part 3, "The homestead site") and `Assets/Docs/BuildPlan.md` (Phase U) in the same commit. Where this spec and GameDesign.md disagree, GameDesign.md wins — this is the dated record.

---

## Purpose

Settles design thread #6: which homestead upgrades exist, in what order, at what cost. GameDesign.md promised "stand, vats, storage, rooms, eventually a cellar of your own" without designing any of it, and thread #7 (unlock cadence) was blocked on this content existing.

## Decisions taken in session

| Question | Decision |
|---|---|
| Scope | **Full slice tree** — shell completion through endgame |
| Cost model | **Materials only.** Homestead growth is built, never bought; cash stays exclusively Mrs. Holt's deeds, so the two progression ladders never compete |
| Triage arc | **Choice stays alive.** Demand co-scales with supply for the whole slice; extra vats buy parallelism across recipe speeds, not surplus |
| The player's own cellar | **Yes, mid-game, before the Mill reveal.** The player learns firsthand what a moonshiner's cellar is for, so the Mill's door locked from the inside lands harder |
| Interiors | **In the tree, decoration included** — functional rooms and pure-expression upgrades both |
| Structure | **Physical projects** (option A) over linear levels or an upgrade menu |

## Principles

1. **A project is a physical spot on the homestead plot** with its own staged build, exactly like the shell: walk up, spend materials, the thing visibly advances. Reuses the shipped staged-build tech (`Homestead.cs` stage pattern).
2. **Materials only, and the material list *is* the gate.** Stone and Wood are foraged from day one. Nails, Barrels, and Still Parts exist only once the Smithy & Cooperage is restored (its hand-production goods, per the factory model). Any stage costing those implicitly waits for the Smithy — no locks, no level requirements, just a recipe the player can read.
3. **A project can be started before it can be finished.** Early stages cost foraged materials; later stages cost factory goods. A half-built brewing shed standing on the plot *is* the signpost pointing at the Smithy purchase — the town and the homestead pull each other forward. This is the central fusion (restoring the town and growing the operation are the same action) expressed in crafting costs.
4. **Capacity upgrades arrive paired with demand growth**, so the morning what-do-I-brew decision never dies. See "Keeping the choice alive" below.

## The tree — five tracks, seven projects

Costs are material *types*; exact quantities are an implementation tuning pass, as the shell's numbers were.

### Track: The Stand *(sell more)*

| Project | Stages | Effect |
|---|---|---|
| **Stand II — shelves & awning** | 1. Shelves (Wood) → 2. Awning & board (Wood + Nails) | Stage 2 raises request-book slots 3→5 and note arrivals 2→3/night — the already-settled "first stand upgrade" numbers (2026-07-26) get their home. Closes the dangling `RequestBook.SetSlotCount` item in Phase S |

### Track: Production *(brew more at once)*

| Project | Stages | Effect |
|---|---|---|
| **Brewing shed** | 1. Footing (Stone) → 2. Frame (Wood) → 3. Fit-out (Nails + Still Parts) | Stage 3 brings **vat #2** online: parallel ferments, so a slow batch never blocks berry shine |
| **Vat #3** | 1. Shed extension (Wood + Still Parts) | Late-game, after the cellar: keeps daily recipes flowing while aged batches occupy the pipeline for days |

### Track: Aging *(the cellar — the mid-game centerpiece)*

| Project | Stages | Effect |
|---|---|---|
| **The cellar** | 1. Dig & line (Stone, lots) → 2. Shore & stairs (Wood + Nails) → 3. Racks (Wood + Barrels) | Unlocks **barrel aging** — the quality ladder's "aged" rung gets its home. Deliberately lands *before* the Mill reveal |

### Track: Storage *(plan deeper)*

| Project | Stages | Effect |
|---|---|---|
| **Storeroom** | 1. Crates (Wood) → 2. Shelving (Wood + Nails) | Home stash capacity — stockpile ingredients for bigger brewing plans. Stage 1 is the cheapest project post-shell, buildable the same day |

### Track: The hearth *(expression + where night lands)*

| Project | Stages | Effect |
|---|---|---|
| **Porch & fire** | 1. Fire ring (Stone) → 2. Bench (Wood) → 3. Lamp (Nails) | No mechanics. Night beats already stage at the homestead (someone at your fire, a note under a stone) — the player is building the furniture their story arrives on |
| **Furnished rooms** | 2-3 stages (Wood, Nails, later town goods) | Pure expression, interior. No mechanical effect, by decision |

### Deliberately not in the tree

- **A garden** — competes with the foraging loop and the Apothecary. Goes to `LaterIdeas.md` unexamined, per the build rules.
- **The town storefront** — a town channel unlock, not a homestead project.
- **Any bed/sleep mechanics** — the hearth track touches none of them; the 21:00 sleep floor stays an open item under thread #4.

## Soft phasing — what the costs produce with no gates anywhere

- **Right after the shell (~min 40):** Storeroom crates, fire ring, shed footing + frame, stand shelves — all foraged-material stages, all startable immediately. New content lands exactly in the 40-75 minute window (weakness ②).
- **After the Smithy is restored:** everything nail-gated finishes — vat #2, Stand II complete, the cellar becomes diggable.
- **Late:** vat #3, furnishings, the last cellar racks.

## Keeping the choice alive — demand co-scaling

**The invariant: at every state of the tree, the average day leaves at least one order in the request book unfilled.** Shipped today that is standing demand of 3-9 batches against 4-5 batches of daily production. Every capacity project is paired with a demand movement preserving roughly that ratio.

1. **Vat #2 arrives with Stand II, automatically.** Both are nail-gated, so they land within days of each other with no scripting: production roughly doubles (8-10 batches/day) at the same moment the book grows to 5 slots and 3 notes a night. The intended tuning knob as slots grow is **request size drifting upward** (later notes ask for 2-3 batches more often than 1).
2. **The cellar creates a demand type, not just supply.** Aging is a pipeline: brew in a vat (hours), transfer to a barrel on the racks (2-3 in-game days ≈ half an hour of real play). The cellar never adds daily batches; it adds a slow lane. Once aging is unlocked, aged requests appear in the book — consuming barrel-days, a resource the racks strictly limit (2-4 barrels, tuning). Before the cellar exists, descriptive requests for "something aged" point at it, per the existing descriptive-request design.
3. **Vat #3 is the relief valve, not an escalation.** Arrivals and sizes don't move for it; it restores parallelism once aged batches regularly park in the pipeline.

This protects the settled line that "a constant squeeze reads as pressure, a constant surplus reads as nothing" — the variance stays.

**Left open as implementation tuning:** exact material quantities per stage · aged ferment duration (2-3 game days is the proposal) · rack capacity · the request-size drift curve · the furnished-rooms stage count (pure expression, so content volume rather than structure). All numbers, no structure.

## Cadence check — the 2-3-visible rule walked through the tree

- **Minute 40 (shell done):** Storeroom crates (cheap, today) · fire ring (cheap, today) · brewing shed (mid, startable, visibly unfinishable without nails) · stand shelves (mid). 3-4 visible; the half-built shed points at the town.
- **First town buildings restored:** shed fit-out → vat #2 (mid) · Stand II awning (mid) · the cellar dig (aspirational, named, big) · storeroom shelving (cheap). The cellar is the "aspirational but named" slot for the whole mid-game.
- **Post-cellar:** vat #3 (mid) · furnished rooms (cheap-mid, expression) · last cellar racks (mid) · town-side, the storefront. Cadence holds to the slice's end.

This walkthrough is thread #7's first test on paper; the thread itself stays open until tested against the full unlock set (recipes, town buildings, people).

## Ripples

1. **Thread #6 closes.** GameDesign.md status board updated; Part 3 gains "The homestead site."
2. **Thread #7 (unlock cadence) unblocks** — it has real content to test against.
3. **Thread #5 (side activities) loses a candidate.** "Decorating and furnishing interiors" is absorbed into the hearth track; the candidate list shrinks to five.
4. **BuildPlan.md gains Phase U** (homestead site projects), depending on Phase S for the stand and on Phase 7/F for factory-good stages. The dangling `RequestBook.SetSlotCount` item moves into Stand II.
5. **A garden project goes to `LaterIdeas.md`** unexamined.
6. **Coordination note for thread #9 (recipe book):** completing the cellar dig is a natural candidate milestone for a recipe-book page becoming legible. Suggested, not settled — thread #9 owns its page sources.
