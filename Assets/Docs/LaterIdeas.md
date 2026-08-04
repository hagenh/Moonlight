# Lamplight — Later Ideas

Ideas that came up during implementation but are deferred to a future phase or design pass. Not tracked in BuildPlan.md until they get their own spec.

## UI

- Tormod exclamation mark over head to guide player to talk to him (nails gift, quest indicators). Requires proper sprite/UI work.
- All UI except the debug menu needs proper UI treatment — current IMGUI panels are functional but not visually polished.

## Homestead Interior

- Interior construction stages for the Homestead (separate from the exterior build). Could gate interior rooms behind materials/crafting.

## Nail Economy

- ~~Nails beyond the initial Tormod gift — buying from General Store, crafting, or scavenging in later phases.~~ **Answered 2026-08-04** — nails are a Smithy & Cooperage factory good (Aksel). See `Assets/Docs/GameDesign.md` Part 3, Buildings, and `docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md`.

## Hired help — absorbed into the people ladder, 2026-08-04

> **Update, 2026-08-04.** This idea is no longer parked — it's built into the factory model as the people ladder (`Assets/Docs/GameDesign.md` Part 3, Buildings; full record `docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md`). The reduced-gain guardrail below became the wage model verbatim. Kept here as the original reasoning, not as a queued feature — the Boarding House's operation role is now settled separately as Elias's recruitment housing, not hired help itself.

Candidate for two open slots at once: the **Boarding House's vacant operation role** (it lost the courier to the runs cut) and **thread #5's variety problem**. Also feeds the automation and people unlock categories.

**The idea.** The player can hire help — minding the stand, hauling, later more. **Hiring is relationship-gated and named-only: you hire people you know.** An earlier draft added cheap anonymous randoms with character flaws as a second path; **rejected 2026-08-03** — every hireable is a known, named character.

**Personality still does the work.** A named hire brings who they are — quirks and strengths the player learns by talking to them and hearing town gossip *before* any hiring happens. Nothing about a hire is a surprise you couldn't have learned over a conversation; the legibility guardrail is satisfied by construction.

**The guardrail boundary, for whenever this is designed.** Any quirk that subtracts held goods (skimming stock) violates guardrails 1 and 7. Cozy-compatible shapes: **reduced gain** (a quirk eats part of what the worker produces — you never had it, so nothing is subtracted), **opportunity cost** (a slower or capped channel), or **pure character color** in dialogue.

**What it serves:** hook 4 (planning — who minds what while you do something else), the town-as-people fantasy, and possibly the Boarding House: Elias lodges newcomers, which is how new named hireables enter the town and get known.

## Bait Notes — parked 2026-07-25 (thread #1), then ruled out of genre the same day

> **Update, 2026-07-25 — do not revive without overturning the genre.** These were parked pending thread #4, on the theory that a night system might want a daylight counterpart. Thread #4 answered that Lamplight has **no mechanical edge anywhere** and added guardrail 7 to protect it. Bait notes are therefore not merely deferred; they belong to a different game. Kept below as a record of the reasoning, not as a queued feature.

Cut from `GameDesign.md` because they were the design's only source of daytime loss and contradicted guardrail 1.

**The mechanic.** Some notes in the request book are bait, written by the law. The tells are in the writing: an odd quantity · a question about *where* you make it · handwriting nobody recognises · payment that is too generous. Filling one hands contraband to a guard who wanted exactly that.

**Why it was attractive.** It returned danger to the stand *without requiring the player to be present*, recovering the Constable's staging ground that the unattended-stand design gave up. The threat is not a person leaning on your counter — it is a piece of paper you judge in the morning, before you commit. And it stays legible: a burned player can always point at the note and name what they missed. No hidden dice.

**Why it was cut.** Every honest consequence of being successfully baited is either a heat meter in costume (evidence banking — already deleted in Phase D, correctly) or a daylight loss of something held (confrontation costing cash or standing; surveillance costing shelf income). Guardrail 1 forbids all of them.

**What would have to be true to revive it.** Guardrail 7 would have to be overturned in writing — a deliberate decision that Lamplight is not a cozy game after all. Short of that, no version works: any cost is either accumulating hidden state or a subtraction from something the player holds, and both are now forbidden game-wide rather than just in daylight.

**Also settles:** `Guard.cs` / `GuardManager` were going to write and collect these notes — the last job anyone had proposed for them. With bait notes out of genre, **they are marked for deletion** in `GameDesign.md`. The Guard sprite survives as Constable Aas.

## Police antagonist — after-demo item, parked 2026-08-04

Constable Aas and the police were cut game-wide ("the police cut", `docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md`). Revisit only after the demo, and only if the demo feels flat with no antagonist and no unease channel — this is a "for now" cut with acknowledged uncertainty, not a settled genre position.
