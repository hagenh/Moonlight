# The Grandfather's Recipe Book — Replacing the Fragment System — Design

**Date:** 2026-08-04
**Status:** Approved in session.

---

## Purpose

`Assets/Docs/NarrativeDesign.md`'s "Collectibles (Fragments)" pillar — five generic collectible letters (Baker's Last Loaf, A Carpenter's Ledger, The Constable's Report, A Merchant's Confession, The Mill Cellar), found by smashing debris or hitting a milestone — is cut. It was never built, and in review it reads as disconnected: generic titles, a delivery mechanism (smash random debris, get a letter) unrelated to anything else the player actually does, and one of its five triggers (`constable_restored`) is now permanently dead — `docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md` cuts the Constable and his office entirely.

Rather than patch the dead trigger, the whole pillar is replaced by something `Assets/Docs/GameDesign.md` already names but never designed: **the grandfather's ruined recipe book** (Part 4, weakness ④ and thread #9) — "mostly destroyed, one legible page," carried from minute 3 of Act 0, seeding the cellar's payoff at hour six. GameDesign.md already flags this as increasingly load-bearing ("higher value... since hook 6 carries more of the game's pull") and never contradicts the fragment system existing alongside it — the two were quietly duplicating the same narrative job. This spec gives the recipe book the architecture the fragments had, and nothing else changes hands.

**What stays untouched:** the NPC Relationships and Milestones pillars — `NarrativeFlags`, `MilestoneDetector`, `DialogueLine`, `DialogueResolver`, per-NPC trust. This spec only replaces Collectibles/Fragments.

## The two pillars, not three

`NarrativeDesign.md`'s "Three Pillars" (NPC Relationships, Milestones, Collectibles) becomes two: **NPC Relationships** and **Milestones**. Milestones keeps its existing job unchanged — `MilestoneDetector` subscribes to `GameEvents`, sets `NarrativeFlags` — it simply no longer feeds a fragment lookup; it now also feeds the recipe book's page-reveal check (see below).

The doc's Philosophy line ("Story comes from people, found items, milestones, and actions — not just from smashing debris... NPCs, world events, and collectibles all feed into a shared journal") is rewritten to drop "collectibles" and "smashing debris" as a category — story now comes from people (dialogue, trust) and from the one found object (the recipe book), not from a general collectible system.

## The recipe book

One object, owned from Act 0 minute 3, mostly ruined — legible pages accumulate over the game instead of new objects being found. Built on the same shape the fragments used, renamed and re-triggered:

| Old (deleted) | New |
|---|---|
| `FragmentDef` | `RecipeBookPageDef` — id, title, ~120-word body (the grandfather's voice/marginalia), `triggerFlag`, page order |
| `FragmentUI` (letter overlay on pickup) | `RecipeBookUI` — a "this page is legible now" overlay on reveal |
| `JournalState` (collected fragments list) | `RecipeBookState` — which pages are legible; listens for `triggerFlag`s via the existing `MilestoneDetector` → `NarrativeFlags` pipe |

No separate collectible objects in the world, no debris-smash trigger type, no journal viewer UI (carries forward the same "data layer only for now" scoping the fragments doc already had).

### Pages for the slice

Five pages — same count as the old fragment table, a tuning number, not fixed. `constable_restored` is not reused anywhere; it is dead.

| # | Trigger | Why |
|---|---|---|
| 1 | Owned from Act 0, minute 3 — no trigger | The one legible page from the start |
| 2 | `bakery_restored` | Reuses an existing flag already referenced elsewhere in `NarrativeDesign.md`'s examples |
| 3 | `smithy_restored` | Replaces the old Constable slot. Ties to Aksel's existing beat — "he built the still's twin" — already the design's first thread to the cellar |
| 4 | Mrs. Holt trust threshold | She "knew the original operation and won't sell to a fool who'll repeat its ending" (`GameDesign.md` Part 3, NPCs). Replaces the old Signe/NPC-gift slot — Holt's existing arc fits a confession-shaped reveal better than Signe's |
| 5 | `mill_stage1_complete` | Unchanged from the old fragment 5 — the cellar door beat |

Boarding House restoration does not get a page — five felt sufficient rather than force-mapping every old fragment 1:1. A sixth page tied to `boarding_house_restored` (or any other milestone) is a straightforward future addition if more are wanted; nothing in this architecture caps the count at five.

Titles and the ~120-word bodies for pages 2-5 are not written yet (page 1's content is also open — candidate: the Berry Shine recipe itself, tying the player's starting ferment to the grandfather, but this is not decided). Same treatment the roster spec gave the miller's name: left as a placeholder, not a blocker.

## NPC dialogue tier cleanup

Elias's dialogue-tier entry currently reads "Story: Fragment about the old village" — becomes a plain dialogue line instead of a fragment reference (fits "story comes from people" without a separate system). The Aas (Constable) dialogue tier is removed outright — he's cut. Berta's and Signe's heat-gated example lines (`heat > 40`) are removed — the heat system was deleted 2026-07-25 (`Assets/Docs/BuildPlan.md` Phase D) and these are stale regardless of the fragment cut.

## What this does NOT do

- Does not touch `NarrativeFlags`, `MilestoneDetector`, `DialogueLine`, `DialogueResolver`, or per-NPC trust — all unchanged.
- Does not add a journal/recipe-book viewer UI — same scoping the old fragment system had ("no journal viewer UI, data layer only for now").
- Does not write the actual page titles/bodies — content authoring is future work, same as the fragments were (five table rows, no prose, before this spec).
- Does not change how recipes are mechanically unlocked (barrels from the Smithy, botanicals from the Apothecary, etc.) — the recipe book is narrative only, not a second recipe-gating system.

## Implementation follow-ups (for the plan, not this spec)

1. `Assets/Docs/NarrativeDesign.md`: replace the "Collectibles (Fragments)" pillar with the recipe book throughout — Philosophy intro, the Three Pillars → Two Pillars list, the Architecture section (`FragmentDef`/`FragmentUI`/`JournalState` → `RecipeBookPageDef`/`RecipeBookUI`/`RecipeBookState`), the Data Flow diagram, the fragment content table → the page table above, the NPC Dialogue Tiers section (drop Aas, drop heat-gated lines, rewrite Elias's fragment reference), "What NOT to Build" (fragment-specific bullets → recipe-book equivalents), and the Implementation Order list.
2. `Assets/Docs/GameDesign.md`: weakness ④ and thread #9 (Part 4) currently describe the recipe book only as an unbuilt idea — add a pointer to this spec now that it has an architecture, without duplicating the design there.
