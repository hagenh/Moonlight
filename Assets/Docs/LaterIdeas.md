# Lamplight — Later Ideas

Ideas that came up during implementation but are deferred to a future phase or design pass. Not tracked in BuildPlan.md until they get their own spec.

## UI

- Tormod exclamation mark over head to guide player to talk to him (nails gift, quest indicators). Requires proper sprite/UI work.
- All UI except the debug menu needs proper UI treatment — current IMGUI panels are functional but not visually polished.

## Homestead Interior

- Interior construction stages for the Homestead (separate from the exterior build). Could gate interior rooms behind materials/crafting.

## Nail Economy

- Nails beyond the initial Tormod gift — buying from General Store, crafting, or scavenging in later phases.

## Bait Notes — parked 2026-07-25 (thread #1), then ruled out of genre the same day

> **Update, 2026-07-25 — do not revive without overturning the genre.** These were parked pending thread #4, on the theory that a night system might want a daylight counterpart. Thread #4 answered that Lamplight has **no mechanical edge anywhere** and added guardrail 7 to protect it. Bait notes are therefore not merely deferred; they belong to a different game. Kept below as a record of the reasoning, not as a queued feature.

Cut from `GameDesign.md` because they were the design's only source of daytime loss and contradicted guardrail 1.

**The mechanic.** Some notes in the request book are bait, written by the law. The tells are in the writing: an odd quantity · a question about *where* you make it · handwriting nobody recognises · payment that is too generous. Filling one hands contraband to a guard who wanted exactly that.

**Why it was attractive.** It returned danger to the stand *without requiring the player to be present*, recovering the Constable's staging ground that the unattended-stand design gave up. The threat is not a person leaning on your counter — it is a piece of paper you judge in the morning, before you commit. And it stays legible: a burned player can always point at the note and name what they missed. No hidden dice.

**Why it was cut.** Every honest consequence of being successfully baited is either a heat meter in costume (evidence banking — already deleted in Phase D, correctly) or a daylight loss of something held (confrontation costing cash or standing; surveillance costing shelf income). Guardrail 1 forbids all of them.

**What would have to be true to revive it.** Guardrail 7 would have to be overturned in writing — a deliberate decision that Lamplight is not a cozy game after all. Short of that, no version works: any cost is either accumulating hidden state or a subtraction from something the player holds, and both are now forbidden game-wide rather than just in daylight.

**Also settles:** `Guard.cs` / `GuardManager` were going to write and collect these notes — the last job anyone had proposed for them. With bait notes out of genre, **they are marked for deletion** in `GameDesign.md`. The Guard sprite survives as Constable Aas.
