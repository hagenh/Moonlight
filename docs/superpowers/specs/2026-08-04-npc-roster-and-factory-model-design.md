# NPC Roster & the Factory Model — Design

**Date:** 2026-08-04
**Status:** Approved in session. **Not yet folded into `Assets/Docs/GameDesign.md`** — the fold-in is the first implementation task. Until then, where this spec and GameDesign.md disagree, **this spec wins**.

---

## Purpose

An audit of every named NPC against two new rules, which this spec establishes:

1. **Every finished building has at least one named NPC.**
2. **Every named NPC's mechanic aids the player** — where aid counts as any of: tangible resources, useful information, services, or access/unlocks.

The audit found four failures (Tormod gave nothing after Act 0, Elias's operation role was vacant, the restored Mill had no resident, Constable Aas aided nothing by design). Working through them produced a larger structural change — the factory model — plus one cut and one new character.

## The two structural rules (new)

**Buildings produce; people automate.**

- **A factory is player-operated from the moment it is restored.** Restoring it unlocks hand-production of its goods for the player. **No resource is ever gated behind a person** — "build a house and get a person to unlock yeast" is exactly the shape this forbids.
- **People are the upgrade, not the key.** Hiring a named NPC automates their factory so it produces without the player present.

**The people ladder** (fills the previously empty *automation* unlock category):

> restore factory → produce goods yourself → build the operator's house → they move in → hire them → the factory runs without you

- **The house is the hiring gate.** No house, no hire. Houses are player-built on new lots, using the homestead build-from-scratch tech (staged builds from gathered materials).
- **Hiring is named-NPCs-only** (reaffirms 2026-08-03; randoms stay rejected).
- **Wages are reduced gain:** a hired operator keeps a share of the factory's output. The player never loses anything held, so guardrails 1 and 7 are satisfied by construction (shape already sanctioned in `LaterIdeas.md`). The share is a playtest number.

## Building taxonomy

| Kind | Buildings | What it does |
|---|---|---|
| **Factory** | Bakery, Smithy & Cooperage, Apothecary, Old Mill | Restore → the player produces its goods by hand. Hire its operator → automation |
| **Shop** | General Store, Roadhouse | NPC-run services (see roster) |
| **Housing** | Boarding House | Elias lodges **newcomers** until their house is built — the town's front door for new named characters. Established townsfolk are around from day 1 |
| **NPC houses** | 4 new player-built lots: Berta's, Aksel's, Ingrid's, the miller's | The people ladder. Each house's named NPC is its owner; its aid is the hire it enables |
| **Player sites** | Homestead; the Mill once bought | **The Homestead is only ever the player's — no other resident, ever.** The Mill becomes the player's second site at endgame; its cellar is the endgame base |

Factory goods: Bakery = yeast (faster ferments) · Smithy = nails, barrels, still parts (this answers the parked Nail Economy item) · Apothecary = botanical extracts → flavored recipes · Mill = bulk grain.

## The roster — verdicts

Nobody existing is replaced; replacement of design-row characters is just renaming. Verdicts:

| NPC | Verdict | Building(s) | Aid |
|---|---|---|---|
| **Tormod** | Keep, reworked | Roadhouse (lives above it) | Act 0 greeter + 3 Nails gift, unchanged. Restored Roadhouse adds: **rare traveler-brought ingredients** (rotating stock Signe never carries) + **gossip feeding the request book** — hints at descriptive requests, advance word of market days and visiting buyers. Never a selling channel |
| **Berta** | Keep, sharpened | Bakery operator; player-built house | Yeast via the Bakery factory; hire → automated yeast. Thread #8 candidate beat (not settled): she finds the player botching a yeast batch and fixes it unprompted — "catches you and helps," zero jeopardy |
| **Signe** | Keep as-is | General Store (lives above it) | Staple ingredients, trust-tier discounts, stand-traffic buff. The "world witnesses you" mirror |
| **Aksel** | Keep, sharpened | Smithy & Cooperage operator; player-built house | Nails, barrels, still parts via the Smithy; hire → automation. Recruitment beat unchanged (he built the still's twin — first cellar thread) |
| **Ingrid** | Keep, one fix | Apothecary operator; player-built house | Botanical extracts via the Apothecary; hire → automation. **Her "buys openly as medicine base" stops being a channel** — her demand becomes signed notes in the request book (the book wins, 2026-08-03) |
| **Elias** | Keep, refilled | Boarding House (lives there) | **Recruitment housing:** newcomers lodge with him until their house is built. Fills the vacancy left by the courier |
| **Mrs. Holt** | Keep, anchored | Her own fine house on the street — pre-existing, never player-touched | **The deed-holder: every lot and house purchase goes through her** (access aid, all game). Gates the Mill last; keeper of the cellar's story; contempt→respect arc unchanged |
| **The miller** (new; placeholder name **Runa**) | Create | Arrives via the Boarding House; player-built house; works the player's Mill | The endgame hire: bulk grain from the Mill the player owns. First proof of Elias's newcomer pipeline |
| **Constable Aas** | **Cut** | — | See below |
| Homestead character | Considered in-session, **cancelled** | — | The Homestead is player-only |

**Coverage check:** all 8 town buildings + 4 houses have a named NPC; every NPC's mechanic aids the player through at least one sanctioned aid type. The Homestead's named resident is the player.

## The police cut

**The police are removed from all aspects of the game, for now.** Constable Aas, his office (the "one dark window"), his daylight beats, and his single night appearance are all cut. This deliberately overturns the 2026-07-25 decision that kept him as a recurring character.

- Recorded as an **after-the-demo item: revisit a police antagonist.** This is a "for now" cut with acknowledged uncertainty ("might need it later"), not a settled genre position.
- Thread #3 closes for now. Phase N shrinks to night beats only. Fragment 3 ("The Constable's Report", `NarrativeDesign.md`) needs replacing. The Guard-sprite-as-Aas reuse is moot.
- **Cost, stated plainly:** the game loses its only antagonist and its last unease channel; the criminal fantasy currently has no law anywhere in it. The pull rests entirely on hooks 5 and 6 (transformation, the cellar). If the demo feels flat, this cut is the first place to look.

## Other costs, stated plainly

- **Scope add:** four house lots, four factory production interfaces, a hiring system, an automation system, one new character. `BuildPlan.md` needs new phases.
- The shipped Berta move-in sequence retargets from the Bakery to her house (tech survives; destination changes).

## Open playtest questions (deliberately unsettled)

- Operator wage share (reduced-gain %), production rates per factory, house build costs/stages, hiring trust thresholds
- Whether hand-production is a menu or a small physical activity at the factory
- Whether factory goods can feed the request book directly (selling nails/extracts vs. moonshine only)
- The miller's name (Runa is a vetoable placeholder) and characterization
- Where Berta, Aksel, and Ingrid are seen in town before their factories are restored (scene detail)

## Implementation follow-ups (for the plan, not this spec)

1. Fold this spec into `GameDesign.md`: NPC table, Buildings table, Part 3 Constable section → after-demo note, guardrail/Part 0 references to the Constable, revision log entry.
2. `LaterIdeas.md`: mark Nail Economy answered (Aksel); rescope hired-help as absorbed into the people ladder; add the after-demo police item.
3. `NarrativeDesign.md`: replace fragment 3; remove Aas dialogue tier (also stale heat references).
4. `BuildPlan.md`: new phases for factories, houses, hiring/automation; Phase N rescope; retarget Berta move-in.
