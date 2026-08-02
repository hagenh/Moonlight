# Thread #1 — The guardrail contradiction: resolution

**Date:** 2026-07-25
**Status:** Settled. Folded into `Assets/Docs/GameDesign.md`.
**Thread:** #1 of the design status board in GameDesign.md Part 4.

---

## The contradiction

`GameDesign.md` guardrail 1 says **"Never punish the player for playing. Day life carries no loss risk."**
`BuildPlan.md` line 102 says the same thing: **"never punish daytime play."**

But GameDesign.md Part 3 required every Constable beat to fail at a cost of **"standing or opportunity"** — and standing is something the player earned, lost in daylight. The document contradicted itself.

Suspicious requests sharpened it rather than easing it. Filling a bait order had to cost *something* or the one tension mechanic in the primary economy was empty theatre.

## The resolution

**Cut bait notes. Keep the guardrail intact.**

Bait notes were the only mechanic in the design that imposed a daytime loss. Removing them dissolves the contradiction at its source instead of negotiating the guardrail down around it.

Three consequent changes:

1. **Guardrail 1 stands, and gains an explicit scope.** Day life costs the player no cash, no goods, no built progress, and no standing. The guardrail is now unconditional — there is no exception anywhere in the design.
2. **Constable beats cost opportunity only.** He can cost a door that never opens — a request that never arrives, a discount never offered, a beat the player does not get. He can never take something the player holds. "Standing" is struck from the design requirements for a beat.
3. **Bait notes are parked, not deleted.** The full design is preserved in `LaterIdeas.md` so the option survives if night turns out to need a daylight counterpart.

## Why cut rather than narrow the guardrail

Two resolutions were on the table before the fiction was pinned down:

- **Narrow the guardrail** to "no loss of cash, goods, or progress" and leave social standing fair game.
- **Constable can only cost upside not yet earned.**

Pinning the fiction killed the first. Bait notes are written by the law, not by careless customers — filling one means handing contraband to a guard who wanted exactly that. Once that is true, the honest consequences are evidence, confrontation, or surveillance, and every one of them is either a heat meter in costume (already deleted in Phase D, correctly) or a daylight loss of something held. None of them survive contact with guardrail 1.

The second resolution is what remains, and it is the one adopted — but with bait notes gone it is no longer a compromise that leaves an awkward edge case. It is simply true.

## What this costs

**The request book now has no edge.** Its only tension is triage: limited ingredients and limited time mean you cannot fill everything. That is real, but it is thin for the primary economy of a twenty-hour game.

This is recorded as a new weakness ⑤ in GameDesign.md Part 2 rather than papered over. It is a deliberate bet that night pays the tension back.

**The Constable loses his last daytime venue.** The unattended-stand design already removed his obvious staging ground; bait notes were what recovered it. Without them he is texture, not a system.

This is the correct order of operations. GameDesign.md already states that *"what is night for?"* is the blocking pillar question and that nothing else should be built until it is answered. Attempting to bolt danger onto the day before answering it is what produced this contradiction in the first place.

## Effect on the dependency chain

| Thread | Before | After |
|---|---|---|
| **#1 guardrail contradiction** | Open, blocking #3 | **Settled** |
| **#3 the Constable** | Blocked on #1 | Blocked on **#4** |
| **#4 what is night for?** | Blocked on nothing | Unchanged — now the front of the queue |

Thread #4 is next.

## Effect on the BuildPlan audit

Audit item note "Resolve thread #1 first — the guardrail wording determines what a Constable phase is allowed to contain" is discharged. The wording is settled: a Constable phase may contain beats that cost opportunity, and may not contain anything that removes cash, goods, progress, or standing.

Audit item 5 ("no phase exists for the stand or the Constable") is unchanged in substance, but the Constable half of it cannot be scheduled until thread #4 closes. The stand half can be scheduled now.

`Guard.cs` / `GuardManager` remain orphaned. Bait notes would have given them a job; parking bait notes parks that too. The decision to delete or repurpose them now waits on thread #4.

## Changes to GameDesign.md

- Part 0 table, "Primary economy" row — drop "and some notes that are bait"
- Part 2 weaknesses — add ⑤, the request book has no edge
- Part 3 economy table — stand/requests "Risk" becomes None
- Part 3 "Tension: suspicious requests" — replaced by "Tension: triage"; records the cut
- Part 3 "Open numbers" — drop suspicious-note frequency and tell-density
- Part 3 "Danger — the Constable and the front" — bait-note venue removed; thread #3 note rewritten; beat requirement 3 becomes "opportunity"
- Part 3 guardrail 1 — explicit scope added
- Part 4 status board — #1 moves to Settled; #3 reblocked onto #4
- Part 4 thread #1 section — replaced with the resolution
- Part 4 audit — closing note updated
- Revision log — entry added
