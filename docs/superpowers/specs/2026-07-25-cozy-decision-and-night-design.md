# The cozy decision, and what night is for

**Date:** 2026-07-25
**Threads:** tone (new, parent) · #4 "what is night for?"
**Status:** Settled. Folded into `Assets/Docs/GameDesign.md`.

---

## What was asked

Thread #4 was the front of the queue: *what is night for?* `GameDesign.md` listed three candidates
and asserted that answering was blocking — because thread #1 had made daytime unconditionally safe,
which left night as the only place tension could live.

## What was found first

**Night's actual duration had never been written down.** Read from the build:

| | |
|---|---|
| Real time per game hour | ~46 s (`TimeManager.cs:7`, `realSecondsPerGameMinute = 0.77`) |
| Playable day | 08:00 → 24:00 = ~12.3 real minutes |
| Dusk onward (19:00+) | ~3.8 real minutes |
| Genuinely dark (21:00+, `DayNightLighting` intensity ≤ 0.5) | ~2.3 real minutes |
| Midnight | `CurfewReached` forces sleep (`TimeManager.cs:58` → `SleepManager.cs:34`) |
| Voluntary sleep | Permitted from 21:00 (`Bed.cs:9`) |

Night is already a two-to-four minute tail ending in a hard cut. This independently eliminated the
"night keeps covert activity" candidate — there is not enough night to do anything in — and it meant
whatever was chosen would need **no clock retuning**.

## The parent decision — tone

Before choosing among the candidates, the assumption underneath them was tested: *does Lamplight need
a mechanical edge at all, and must it be at night?*

**Answer: no. There is no mechanical edge anywhere, on purpose.**

Lamplight is a restoration game with a criminal skin. The criminality is flavour, fantasy, and story.
It is not a risk system and never becomes one.

### Why this is the right call

The design had spent three separate threads trying to *place* a tension system:

1. Delivery runs — cut, because defanging them to satisfy the guardrails left "getting caught costs a
   fee you can afford"
2. Bait notes — cut, because every honest consequence was either a heat meter in costume or a daylight
   loss the guardrail forbids
3. Night — about to inherit the problem by default

Each cut was individually correct and each was justified locally. Taken together they are one finding:
**the design wants guardrails that no tension system can satisfy.** The contradiction was never in any
one feature; it was in wanting jeopardy and refusing all of its costs. Removing the want dissolves it.

### Consequences, all folded into `GameDesign.md`

- **Guardrail 1 widens** from "day life carries no loss risk" to no loss anywhere, at any hour
- **New guardrail 7 — "cozy is the genre, not a fallback."** Proposals reintroducing jeopardy are
  rejected by default; overturning requires overturning the genre position in writing
- **Weakness ⑤ closed** by decision rather than design. Triage is now sufficient because it is the only
  pressure the game intends to exert
- **The Constable is reframed** from a tension system to a recurring character who costs the player
  nothing. Thread #3 unblocks and shrinks from systems work to writing
- **`Guard.cs` / `GuardManager` resolve to delete** — nothing will ever need patrol or detection code.
  The Guard sprite still becomes Constable Aas
- **Bait notes move from parked to out of genre** (`LaterIdeas.md` annotated)
- **The moral axis loses its "closed doors" resolution** and may not be worth building
- **Thread #5, side activities, is promoted to core** — with no jeopardy, variety is the only retention
  mechanism in hours 3-10

### What it costs, on the record

The game's pull now rests entirely on hook 5 (transformation — the town visibly changing) and hook 6
(the question — the cellar). **Neither has ever been played.** If the slice fails to hold testers, the
cause is almost certainly here, and the fix is to strengthen those two rather than reintroduce danger.

The honest counter-argument, also recorded: cozy games that work usually retain a *pressure gradient*
— a season that ends, a stamina bar, a debt. Lamplight has none. The playtest question is whether days
feel like they have a shape or whether the player drifts. If they drift, the fix is a soft clock, not loss.

## Thread #4 — the answer

**Night is a scene, not an activity block.**

Not a second work shift, not a covert window, not a planning phase. Night is the short warm tail of the
day in which the day's story occasionally lands, followed by sleep. **Most nights nothing happens and
the player simply goes to bed — that is correct, not a shortfall.**

### How a beat reaches the player

**Beats wait at the homestead.** Every day ends by going home to sleep, so beats live where sleep already
lives: someone at your fire, a note under a stone, a lamp lit in a window that was dark yesterday.

This makes beats **unmissable by construction** — satisfying guardrail 4 with no telegraphing, no
appointments, and no scheduling system. It also gives the walk home a reason to exist and reinforces the
homestead-as-permanent-site principle.

### What a beat is made of

Cozy register. Warmth, story, or the cellar mystery — never a threat and never a bill:

- A recruit waiting at your fire because they had nowhere else to go
- A fragment of the old operation's story surfacing
- Someone thanking you for something you did days ago and had forgotten
- The Constable, exactly once and memorably, simply standing in the road

**A beat leaves the player knowing something, or feeling something. It never changes their inventory.**

### The pillar, restated

*"Day = the front, night = the operation"* is **retired.** It described the delivery-run game, which is cut.

**Replaced by:** *day is when you act; night is when the day answers back.* The day/night cycle survives
as pacing and mood rather than two modes of play. The lighting system survives on its own merits — dusk
over a lit street is one of the best images the game has, and Act 0 already proves it works.

## Deferred to implementation

Beat frequency · how beats are authored and triggered (milestone, day count, or hand-placed) · whether
empty nights get a small ambient reward · whether the 21:00 sleep floor (`Bed.cs:9`) should move now that
night has content worth encountering.

## Follow-on work unblocked

| Item | State |
|---|---|
| Thread #3, the Constable | Unblocked, reduced to a writing job. **Next in queue** |
| Thread #5, side activities | Unblocked and promoted to core |
| `BuildPlan.md` audit item 5 | Fully unblocked — stand phase and night/Constable phase can both be scheduled |
| `BuildPlan.md` audit item 7 | Resolved as delete |
| Phase 3 darkness pass | Unblocked, but must be rescoped to the homestead at night and the lit town from the treeline |
| Deep woods | **More pressing** — the cozy decision removed their last candidate justification |
