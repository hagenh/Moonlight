# LAMPLIGHT Redesign — Front-Town Empire (Design Spec)

> **SUPERSEDED 2026-07-25 by `Assets/Docs/GameDesign.md`.**
> This file is retained as the dated record of what was approved on 2026-07-22. Where the two disagree, `GameDesign.md` wins.
> Most notably: delivery runs, routes, patrols, load-outs, and covert forest sockets described below were **cut** on 2026-07-25; danger moved to the front as a social system, and the stand became the primary economy.
> The "What dies (code-level)" and "What's reused" sections below remain accurate as a record of the Phase D migration and were deliberately not carried forward.

Date: 2026-07-22
Status: Approved in brainstorming; supersedes the fantasy/tension/NPC layers of `Assets/Docs/BuildPlan.md` and reskins `Assets/Docs/NarrativeDesign.md`. Production/renovation/day-cycle systems survive unchanged unless listed under "What dies."

## Vision

You are a moonshiner rebuilding a dying town as the perfect front — every business you restore, every neighbor you recruit, every lamppost you raise is both an act of genuine care and a cog in the operation.

The day/night cycle is the axis of the double life:

- **Day — the front.** Production (mash, ferment, bottle), the stand, restoration, neighbors, placing public infrastructure. Fully cozy. No danger exists during the day.
- **Night — the operation.** Load cargo, pick a route, walk it past patrols to a buyer. Opt-in every time.
- **Sleep — the tick.** Production advances, construction progresses, income lands, autosave. No punishment steps.

Design statement: *you light the town and keep the woods dark.*

## Pillar decisions

| Question | Decision |
|---|---|
| Core fantasy | Front-town empire: every building a legitimate business on the surface, an operation node underneath |
| Tension locus | Opt-in delivery runs only; no persistent heat meter; caught = lose carried cargo |
| NPC role | Co-conspirators: day job (front) + operation role (function) + recruitment beat (story) |
| Town agency | Two-layer infrastructure: public street sockets + covert forest sockets |
| Narrative | Reskin the spine: recruitment beats, conspiracy trust, Mill cellar hook preserved |
| Scope stance | Design first; `BuildPlan.md` phases get rewritten around this spec afterward |
| Run mechanics | On-map routes + load-out progression (satchel → handcart → courier) |
| World structure | One connected open-world exterior scene; interiors remain separate small spaces |

## Act 0 — The tent opening

You arrive on foot with a tent, a copper pot, and your grandfather's mostly ruined recipe book — one legible page: berry shine.

- Camp is a fixed clearing in the near forest, ~20 s walk from town.
- Loop: pick berries → wild-ferment in the pot (airborne yeast, slow, small yield) → carry 2 jars to the Roadhouse back door at dusk → Tormod names a price. Tormod recruits *you* — this is the tutorial and his recruitment beat in one.
- Roughly 3 sales buys the derelict homestead at the town's edge: first owned building, proper still, first vat, and the street of boarded windows visible from the porch. The real game begins.
- The tent stays standing afterward — first stash point on future run routes, and a monument to where you started.

**Pacing guardrail (hard):** Act 0 lasts 20–40 real minutes, 2–3 in-game days. The homestead price is shown from the first sale and reachable in ~3 sales. A playtester still in the tent on day 4 means the numbers are wrong.

Reuse notes: foraging verbs persist all game (Ingrid's botanicals come from the same forest); the near forest learned in Act 0 is the same terrain night runs later cross. The tutorial teaches the map.

## The world — one scene, three depths

Single exterior scene, no loading between rings:

1. **Town street** — the existing 60×20 strip; homestead at its edge.
2. **Near forest** — camp clearing, berry/botanical foraging grounds. Safe by day; runs pass through it by night.
3. **Deep woods and far edges** — run destinations, 60–90 s out. Patrol territory.

Interiors stay as separate small spaces (current `InteriorManager` tech). Interiors are required only for the Roadhouse and the homestead; every other building may be facade-only without affecting any system in this spec — final interior count is a production decision for the implementation plan.

## Buildings

Seven purchasable lots plus the homestead. Each: front / operation function / progression track (more shine · new shines · safer routes · better sales).

1. **Roadhouse** — drinks and beds / first and steadiest buyer: sells shine under the counter, in town, no run needed; medium price, capped daily volume so it never obsoletes runs / *better sales*. First-hour arc.
2. **Bakery** — bread / yeast supply (faster fermentation) + bread-cart cover: once Berta is recruited, one daytime bread delivery per day moves 2 jars at zero risk — the gateway before night runs / *safer routes*.
3. **General Store** — dry goods / ingredient discounts, sells jars-sugar-copper; trust tier 2: Signe talks you up, buffing stand traffic and prices / *better sales + supply*.
4. **Smithy & Cooperage** — tools and barrels / hardware track: still upgrades (bigger boiler), second vat, the handcart, charred-oak barrels unlocking aged recipes / *more shine + new shines*.
5. **Apothecary** — remedies (medicinal alcohol was semi-legal: the front writes itself) / botanicals unlocking flavored/premium recipes; buys a small amount openly as medicine base / *new shines*.
6. **Boarding House** — rooms, no questions asked / houses recruits: the courier (route automation) and later a lookout; rent income / *safer routes + automation*.
7. **The Old Mill** — grinds the valley's grain / bulk cheap grain (mash capacity) + the cellar. Endgame building, most expensive, gated by Mrs. Holt. Stage-1 completion reveals the cellar door, locked from the inside. Title card / *more shine + the story*.

**The Constable's office is never purchasable.** It sits on the street, light always on — the one window the player didn't light. Deliberate single exception to "every building aids progression": it anchors the tension.

Quality ladder across the tracks: wild berry shine (Act 0, rough, cheap) → corn/grain shine (homestead + Mill grain) → aged and flavored (Aksel's barrels, Ingrid's botanicals).

## NPCs

| NPC | Day job | Operation role | Recruitment beat |
|---|---|---|---|
| Tormod | Roadhouse keeper | First buyer | He recruits you — tastes the first batch, names a price (Act 0 tutorial) |
| Berta | Baker | Bread-cart cover, yeast | Drowning in bank debt; catches you mid-smuggle and covers for you unprompted — then you talk |
| Signe | Storekeeper | Supply + sales buff | The company store in the next town is bleeding her dry; joining is her quiet revenge. Remains the "world witnesses you" mirror |
| Aksel *(new)* | Smith & cooper | Hardware upgrades | Recognizes the still's coppersmithing — he built its twin decades ago. First thread to the cellar |
| Ingrid *(new)* | Apothecary | Recipes | Entirely willing — needs alcohol for tinctures; proposes the "medicinal" arrangement herself |
| Elias | Boarding house keeper | Houses recruits; repairs | Shelters people the law calls vagrants; you prove you're the same kind of person |
| Mrs. Holt | Owns the deeds | Gatekeeper of the Mill | Contempt→respect arc; she knew the original operation and won't sell the Mill to a fool who'll repeat its ending |
| Constable Aas | The law | Antagonist — not recruitable in the slice | Polite in daylight, methodical after dark. Corrupt-deputy door stays open for the full game |

Binding thread: the town was a moonshine town once; the law ended it and the town began dying the same year. Every recruit knows a piece of that story. Restoring the operation and restoring the town are the same act — that is why the front-town fantasy holds.

## Delivery runs

**Destinations (each an appointment mechanic):**
- **Logging camp** — rowdy crews; payday every Friday doubles demand.
- **River dock** — barge contact, steady prices; later, big standing orders.
- **Crossroads wagon** — the "neighboring village" as a contact wagon appearing at the crossroads on set nights and hours. One prefab instead of a second town.

**Routes:** main road (fast, lit, night checkpoints) · forest trail (slow, dark, sparse patrols) · creek path (safest; locked until the shortcut plank is placed).

**Patrols:** reuse the existing `Guard` vision-cone tech. Fixed, learnable schedules — **no random patrol spawns, ever.** Patrols exist only on the run routes (deep woods and far edges) at night; the town street and near forest are never patrolled, so Act 0's dusk walk to the Roadhouse and all in-town carrying stay safe. Detection remains active only while carrying cargo (current `IsCarryingCrate` behavior). A caught player must always be able to name the mistake they made.

**Getting caught:** carried cargo confiscated, full stop — plus an on-the-spot bribe option to keep it, priced by load size. No meter, no aftermath; each run is its own story.

**Load-outs (risk scales with greed):**
- Satchel — 2 jars, can slip off-path through brush.
- Handcart — 8 jars, path-bound, wider detection profile. Built by Aksel.
- Courier — after 5 clean runs on a route and the Boarding House recruit, that route can auto-resolve for a percentage cut. Friction-to-automation applied to the runs themselves.

**Price ladder:** stand (safe, low) < Roadhouse (safe, capped volume, medium) < runs (risky, high). Bread-cart cover: 2 jars/day at Roadhouse-level price, zero risk, hard-capped.

## Two-layer infrastructure

- **Public (street sockets):** lampposts, plank sidewalks, benches, flower boxes, signs. Pure positives — night light in town, small stand-traffic buff at beauty thresholds, NPC dialogue reacts. Beautification is never punished.
- **Covert (forest sockets):** stash barrels (ditch cargo mid-run when a patrol looms, retrieve later — the bail-out verb), trail markers (faint glints only the player reads, navigation off the lit road), shortcut planks (unlock the creek route). Lookout perch is a stretch goal, not slice-core.

Both layers are placed by the player in predefined sockets — no free placement system.

## Narrative reskin

- All structural systems in `NarrativeDesign.md` survive: `NarrativeFlags`, `MilestoneDetector`, conditional dialogue, one-shots, per-NPC trust.
- Trust becomes **conspiracy trust**: it gates operation-function tiers as well as dialogue lines (e.g., Signe tier 1 = discounts, tier 2 = sales buff).
- Move-in beats become **recruitment beats** on the same hand-scripted coroutine tech.
- The five fragments tell one story: the town's shine-town past, how the law killed it, and the Mill cellar — locked from the inside since that year — as its heart. Sources: found while clearing buildings, gifted by recruits, milestone-triggered.
- Global reputation dies. The visible state of the town is the macro scoreboard.
- Playtest metric preserved: **do they ask what's in the cellar?**

## What dies (code-level)

- Persistent heat meter, suspicion tiers, guard-count scaling: `EconomyRules.GetSuspicionTier`, `GetGuardCountForSuspicion`, `GuardManager` heat-driven spawning.
- Sleep raids and heat decay: `SleepManager.ExecuteRaid`, raid constants in `EconomyRules`, heat step in the sleep pipeline.
- The unused `EconomyRules.ShouldConfiscate` RNG roll.
- The night risky-buyer beat (replaced by run destinations).
- Heat HUD element; heat/suspicion delivery pricing (`GetDeliveryPrice` suspicion branch, `GetSuspicionForDrop`).
- Global rep as a dialogue driver.

## What's reused

- `Guard` patrol + vision cones → route patrols.
- Carry/crate system → run cargo.
- Day/night lighting → the double-life axis.
- Sleep pipeline → the tick, minus punishment steps.
- The stand (Phase 2 rework) → safe retail channel, unchanged.
- Building states, staged renovation, ferment/production → unchanged.
- Move-in coroutine tech → recruitment beats.

## Design guardrails

1. **Never punish the player for playing.** Day life carries zero loss risk. All loss is bounded to cargo the player chose to carry at night.
2. **Legibility over drama.** No hidden dice against the player: patrol schedules are fixed and learnable; every catch traces to a nameable mistake.
3. **Act 0 is a prologue, not an act.** 20–40 minutes, hard.
4. **Appointments create "one more day," not FOMO.** Missed paydays and wagons recur on schedule; nothing is permanently missable.
5. **Beautification is never punished.** Public infrastructure has no downside.

## Next step

Rewrite `Assets/Docs/BuildPlan.md` phases around this spec via an implementation plan (writing-plans). Open production decisions deferred to that plan: interior count beyond Roadhouse + homestead, exact prices/yields, patrol schedule tables, socket counts and placement.
