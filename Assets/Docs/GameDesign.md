# LAMPLIGHT — Game Design

**Status:** Living document. Last major revision 2026-07-25.

**This document supersedes** `docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md`. That spec remains as the dated record of what was approved on 2026-07-22; where the two disagree, this document wins.

**What this document owns:** the player's experience — what they do, why they keep doing it, and where the design is weak.

**What it does not own:**

- `Assets/Docs/BuildPlan.md` — build order and phase status. **Warning: BuildPlan Phases 3-5 now contradict this document.** See Part 4.
- `Assets/Docs/NarrativeDesign.md` — narrative implementation architecture (flags, fragments, dialogue resolution). Still valid as tech.
- `AGENTS.md` — code conventions.

**How to use it:** when an idea arrives, ask which hook it serves and which part of the journey it lands in. If neither answer is clear, it goes in `LaterIdeas.md`.

---

## Part 0 — Vision

You are a moonshiner rebuilding a dying town as the perfect front. Every business you restore, every neighbor you recruit, every lamppost you raise is both an act of genuine care and a cog in the operation.

**Design statement:** *you light the town, and the town covers for you.*

> This replaces the earlier statement, *"you light the town and keep the woods dark."* The woods stopped being where the game happens — see Part 4, "The runs decision."

| Question | Decision |
|---|---|
| Core fantasy | Front-town empire: every building a legitimate business on the surface, an operation node underneath |
| Where danger lives | **The front, socially.** The Constable works in daylight, in the cozy part. Not a stealth system |
| What protects you | **The town.** Neighbors who like you lie for you. Restoration is literally defensive |
| Primary economy | The roadside stand and its **request book** — written orders, generous deadlines, and some notes that are bait |
| NPC role | Co-conspirators: day job (front) + operation role (function) + recruitment beat (story) |
| Town agency | Player-placed public infrastructure in predefined sockets |
| Narrative | Recruitment beats, conspiracy trust, the Mill cellar |
| World | One connected exterior scene; interiors separate small spaces |

**The central fusion:** restoring the town and defending the operation are the *same action*. That is why the front-town fantasy holds, and it is the test every new system must pass.

---

## Part 1 — The player journey

### Act 0 — The tent prologue (0-40 min, 2-3 in-game days)

**Shipped.** This is the only part of the game that currently exists end to end.

| When | The player does | The hook doing the work |
|---|---|---|
| 0-3 min | Wakes in a tent with 3 Berry and a campfire pot. No instructions, no tutorial text | **Affordance curiosity.** You have the thing and the place to use it |
| 3-5 min | Starts a Berry Shine ferment — 3h game time, ~2.3 real minutes | **A timer is running.** You now have a reason to be somewhere later |
| 5-15 min | Explores while it brews. Berry bushes, stone piles, fallen logs, the road, the town, boarded windows, NPCs | **Spatial discovery**, plus the strongest single image in the game: a dark street of boarded windows. The promise rendered as level geometry — you can see what you'll become |
| 15-20 min | Dusk. Tormod at the Roadhouse back door. He tastes it and names a price. 2 jars × 15g | First money, and **the recruitment inversion** — he recruits *you*. You aren't asking permission; someone wants what you make |
| 20-40 min | Forage → ferment → sell, ×3. Gathering Stone and Wood between batches. Builds the homestead **shell** in 3 stages: Foundation (3 Stone) → Frame (3 Wood) → Walls (2 Wood + 3 Nails, the nails a gift from Tormod on first sale) | **A named price and a visible site.** Progress is spatial — the build sign physically changes — not a number in a bar |
| ~40 min | Shell complete. The prologue closes | **Capability jump** |

**Hard guardrail:** 20-40 real minutes. A playtester still in the tent on day 4 means the numbers are wrong — fix before proceeding.

**Why the tent works:** the player owns land from minute zero. The homestead is an upgrade to ground they already occupy, not a purchase of someone else's building. Smashing boards and clearing debris is cleaning up *their own camp*.

**Tormod is Act 0 only.** He is the tutorial buyer — a flat rate, no decisions, deliberately simple. He should leave at dawn and arrive at dusk, and once the stand opens he stops being the primary channel.

> **Known bug against this design:** `SellManager.cs:12` sets `tormodLeaveHour = -1` and spawns him once in `Start()`, never removing him. He is currently a permanent, always-open vending machine. This contradicts both the original spec and this document.

### Act 1 — The day game opens (~40 min - 2h)

**Not built.** This is the largest new design in this document.

The homestead shell gives the player an **address**, and an address is what a stand requires. You cannot run a business out of a tent in the woods.

**The roadside stand opens**, and with it **the request book**. This is the primary economy for the rest of the game. Full mechanics in Part 3.

> **The stand must be the first site upgrade, and it must be cheap and immediate** — buildable within minutes of the shell completing, not after another long material grind. This is not a tuning preference. Weakness ② is closed only if the new hook arrives *as* the old one runs out; putting a second grind between the shell and the stand re-opens the exact vacuum the stand exists to fill.

Why this matters more than the price: **it converts production from a routine into a plan.** Before the stand, brewing has no decision content — you make the only recipe you have, as much as you can. After it, brewing is a question: which recipe, how many, for whom, in what order, and what do I start tonight so it's ready Thursday.

**The morning ritual.** Walk out to the stand, read the book, decide what to brew. That is the loop's new heartbeat, and it is deliberately a physical object in the world rather than a menu.

Tormod pays you. The book tells you what to make. That information is the real unlock.

**Also in Act 1:** the Roadhouse becomes the effortless overflow channel · the first town building purchase · the Constable notices you for the first time.

### Act 2 — The town, the Constable, the Mill (~2-6h)

**Not built.**

- **Restoration compounds.** Each building buys recipes, people, and features. Buildings are the progression gate
- **Recruitment beats.** Berta catches you mid-something and covers for you *unprompted* — then you talk. The best beat in the design, because it demonstrates the thesis instead of stating it
- **The Constable becomes real** (see Part 3). Daylight questions, awkward requests, careless recruits
- **The town becomes armor.** Trust earned earlier starts paying out as protection
- **First lamppost.** The street changes because you changed it
- **The quality ladder** — berry → grain → aged → flavored
- **The Mill**, gated by Mrs. Holt, most expensive. Stage 1 completion reveals a cellar door, locked from the inside. Title card

**The playtest metric for the whole slice: do they ask what's in the cellar?**

### Hours 6-20 — In outline

Three engines, layered rather than competing:

1. **Restoration is the gate.** Each building unlocks recipes, people, features. The visible state of the town is the macro scoreboard
2. **The cellar is the pull.** What happened the year the law killed the shine town, and why the door is locked from the inside
3. **Empire and mastery are the texture.** Scale, quality ladder, efficiency, a business that hums

They converge on **ownership of the town** — the feeling that this place is yours because you made it.

**Moral choice enters here, not in the slice.** See Part 4.

---

## Part 2 — The hook inventory

### The six hooks

| # | Hook | Mechanism | Where it lives | Status |
|---|---|---|---|---|
| 1 | **Timer** | "Come back at X" | Ferments, request deadlines, dusk window | Shipped |
| 2 | **Ratchet** | "I'm 30g away" | Cash toward a named price, build stages | Shipped |
| 3 | **Discovery** | "There's more I haven't seen" | Recipes, map, fragments, NPCs | Partial |
| 4 | **Planning** | "What do I start tonight?" | **The request book** — read it each morning | Not built |
| 5 | **Transformation** | "I made this" | Restored buildings, lampposts, the street | Not built |
| 6 | **Question** | "I need to know" | The cellar | Not built |

> Hook 4 replaces what was previously **voluntary risk** (night runs). See Part 4.

### The cadence rule

**At any moment, the player should have 2-3 visible unlocks in reach — at different price points, in different categories.** One cheap and soon, one mid, one aspirational but named.

The eight unlock categories: **capacity · recipes · channels · access · people · automation · expression · knowledge.**

This rule is the mechanism that prevents hook vacuums. It is checkable, which is what makes it a design rule rather than a mood.

### Weaknesses — stated plainly

**① Everything past minute 40 is a hypothesis.** The game currently ends where Act 0 ends. Hooks 4, 5, and 6 — planning, transformation, and the question — have never been played by anyone. The build order is right; the honesty is the point.

**② The 40-75 minute vacuum — now answered, but unproven.** Previously this stretch ran on ratchet alone, the same hook Act 0 had just spent twenty minutes on, only with a bigger number. The stand is the fix: a genuinely new hook arrives exactly when the old one runs out. **This is the single most important thing to prototype next**, because the whole mid-game rests on it.

**③ The daily loop still has one verb.** Foraging feeds brewing; building spends brewing income. Everything routes through moonshine. The stand adds *decision* to that verb, which helps a great deal — but it does not add a second thing to do. See Part 4.

**④ The cellar fires once, at the end, and a free setup is going unused.** The grandfather's ruined recipe book — mostly destroyed, one legible page — is currently pure Act 0 flavor. The player carries a mysterious damaged object from minute zero. Wiring it to the cellar seeds hour six's payoff at minute three for almost nothing. **Cheapest high-value change in this document.**

---

## Part 3 — Reference

### The economy

| Channel | Price/unit | Volume | Constraint | Risk | When |
|---|---|---|---|---|---|
| **Tormod at the back door** | Low, flat | Whatever you have | None | None | Act 0 only |
| **Roadside stand — shelf** | Low | Whatever you stock | None | None | Act 1 onward, passive |
| **Roadside stand — requests** | High | Low — you make what was asked | **Knowing what to make** | Suspicious notes | Act 1 onward, **primary** |
| **The Roadhouse** | Medium | Capped daily | None | None | All game, the floor |
| **Town storefront** | Highest | Higher | Same, more of it | Same | Mid-game channel unlock |

**Tormod and the Roadhouse are the same man, but not the same channel.** In Act 0 you hand him jars at the back door at dusk and he names a price — a person doing you a favour. From Act 1 the Roadhouse becomes a standing account: a capped daily volume you can dump into any time, no conversation required. The person becomes an institution, which is itself a small piece of progression.

The Roadhouse cap exists so it never obsoletes the stand, and its zero-effort convenience exists so a tired player always has an out.

**Appointments survive, relocated.** Market days, festivals, a buyer visiting town — the "one more day" mechanic now brings demand *to* the player instead of sending them out to it. Nothing is ever permanently missable.

### The stand and the request book

**Designed 2026-07-25. The single most important system in the game.**

#### Placement

**Roadside, at the homestead**, on the camp clearing the player already owns. This works because the camp sits on the road between town and everywhere else — passing trade is the premise, and being *outside* town is exactly right for an illicit business.

**A town storefront is a mid-game channel unlock.** Better customers, higher prices, more requests, and a real reason to care about the street you've been restoring. The progression from roadside stall to shopfront narrates going legitimate.

#### Attendance — the stand does not need tending

The player is never summoned anywhere. This protects Act 0's proven hook: *start something, then go do something else*.

- **Shelf trade is passive.** Stock the stand, wander off, come back to coins. This is the income floor and needs no supervision
- **Requests arrive as written notes** in a book by the stand. Nobody arrives on a schedule; nothing expires while you're across the map
- **In-person trade is opportunistic.** If the player happens to be at the stand, someone may be browsing. Never a summons

**Why written orders beat customers arriving:** a queue of people means *reacting* one at a time. A book means seeing all demand at once and deciding what to brew against it — which is the actual hook. It also fits the fantasy harder: written orders, left quietly, no faces. Discretion is the theme, and the mechanic says so.

#### The book is a correspondence, not an inbox

Requests come in as notes. The player leaves goods. Next visit: payment **and a reply**.

> *"Better than the last batch."*
> *"My brother asked where I got it. I didn't say."*

Over twenty hours the book accumulates into a record of the player's relationship with the whole town — built from writing, not systems. The cheapest character in the game.

**Notes are signed.** Voice and relationship arrive through handwriting and phrasing, costing nothing in scheduling.

#### Who writes — and why the mix is the progress meter

| Stage | Who writes | What it means |
|---|---|---|
| Early | **Strangers.** Travelers, carters, passing trade. Anonymous scrawls | You are a stranger living in a tent outside town |
| Middle | Mixed, as restoration and trust grow | The town is starting to know you |
| Late | **Named residents.** Berta, Signe, eventually Mrs. Holt | The town writes to you by name |

The shift is never announced. The player simply notices one day that they know everyone in the book. **The composition of the request book is the game's progress meter** — and the storefront unlock accelerates the shift, so two progressions pull the same direction.

#### What a request specifies

**Mixed — an exact backbone with descriptive spikes.**

- **Most requests are exact:** product, quantity, date. Reliable, plannable, always something worth brewing
- **A minority are descriptive:** *"Something strong. It's for a wedding."* Maps to several valid answers, rewards knowing your own recipes

**Descriptive requests do a second job:** when someone asks for something you *can't make yet*, the book is pointing at your next unlock. A request for something aged, arriving before you have barrels, tells the player barrels exist. This does the work of the cadence rule using content that had to be written anyway.

#### Deadlines and misses

- Deadlines are **generous and stated up front**
- Missing one costs the **bonus, never the customer**. They return with a new want
- **Standing orders** provide a floor so there is always something worth brewing

A request the player can't fill for hours is not a flaw — it is a reason to come back tomorrow, a timer and a ratchet fused. It only sours if it expires punishingly, so it never does.

#### Tension: suspicious requests

Baseline tension is triage — limited ingredients and time mean you cannot fill everything. On top of that sits exactly **one** additional mechanic:

**Some notes are bait.** An odd quantity. A question about *where* you make it. Handwriting nobody recognises. Payment that is too generous. Filling one exposes you.

This matters structurally: it **returns danger to the stand without requiring the player to be present**, recovering the Constable's stage that the async design gave up. The threat is not a person leaning on your counter — it is a piece of paper you have to judge, in the morning, before you commit.

**It stays legible because the tells are in the writing.** A burned player can always point at the note and name what they missed. No hidden dice — just something they didn't read carefully enough.

**Deliberately excluded:** conflicting requests (two customers wanting the same scarce thing) and quality-reputation penalties. Both were considered and cut on 2026-07-25 to keep the morning ritual clean and let one mechanic carry the edge.

#### Capacity

The number of simultaneously active requests starts small and grows through upgrades — stand improvements, then the storefront. Board space is a **capacity unlock**, and more visible demand means deeper planning.

#### Open numbers

Deferred to implementation: request arrival rate · premium size over shelf price · exact active-request counts per upgrade tier · deadline lengths · the frequency and tell-density of suspicious notes.

### Buildings

Seven purchasable lots plus the homestead. Each has a front, an operation function, and a progression track.

| Building | Front | Operation function |
|---|---|---|
| **Homestead** | Your house | **The site.** Shell ends Act 0; then grows all game — the stand, second vat, storage, interior rooms |
| **Roadhouse** | Drinks and beds | The steady floor buyer |
| **Bakery** | Bread | Yeast (faster ferments) + the bread-cart cover |
| **General Store** | Dry goods | Ingredient discounts; Signe talks you up, buffing stand traffic |
| **Smithy & Cooperage** | Tools and barrels | Still upgrades, second vat, charred-oak barrels → aged recipes |
| **Apothecary** | Remedies | Botanicals → flavored recipes; buys openly as medicine base |
| **Boarding House** | Rooms, no questions | Houses recruits; rent income. **Operation role needs redesign** — it previously housed the courier |
| **The Old Mill** | Grinds the valley's grain | Bulk grain; **the cellar**. Endgame, Holt-gated |

**The Constable's office is never purchasable.** It sits on the street, light always on — the one window you didn't light. Deliberate exception to "every building aids progression"; it anchors the tension.

**The homestead is a site, not a purchase.** The shell (3 stages) closes Act 0 on schedule. Everything after — stand, vats, storage, rooms, eventually a cellar of your own — is ongoing. This is the permanent home for capacity unlocks and the reason the plot keeps mattering.

### NPCs

| NPC | Day job | Operation role | Recruitment beat |
|---|---|---|---|
| **Tormod** | Roadhouse keeper | Act 0 buyer | He recruits *you* — tastes the first batch, names a price |
| **Berta** | Baker | Bread-cart cover, yeast | Drowning in debt; catches you and covers unprompted — then you talk |
| **Signe** | Storekeeper | Supply + stand buff | The company store is bleeding her dry; joining is quiet revenge. The "world witnesses you" mirror |
| **Aksel** | Smith & cooper | Still upgrades, barrels | Recognizes the still's coppersmithing — he built its twin. First thread to the cellar |
| **Ingrid** | Apothecary | Recipes | Willing from the start; proposes the "medicinal" arrangement herself |
| **Elias** | Boarding house keeper | Houses recruits; repairs | Shelters people the law calls vagrants; you prove you're the same |
| **Mrs. Holt** | Owns the deeds | Gates the Mill | Contempt→respect. She knew the original operation and won't sell to a fool who'll repeat its ending |
| **Constable Aas** | The law | **The antagonist system** | Not recruitable in the slice |

**Binding thread:** the town was a moonshine town once. The law ended it, and the town began dying the same year. Every recruit knows a piece of that story.

### Danger — the Constable and the front

**This replaces the delivery-run system entirely.**

Danger is **social, daylight, and authored.** The Constable is polite, patient, and present in the cozy part of the game. Pressure arrives as discrete situations with visible choices:

- **A note in the request book is bait** — an odd quantity, a question about where you make it, handwriting nobody knows. This is the primary venue (see "Suspicious requests" above)
- A recruit is careless and you decide whether to cover for them
- He asks a neighbor about you — and what they say depends on what you've done for them
- He finds you at the stand while you're stocking it, and makes conversation

> **Still open (thread #3).** The stand going unattended removed the obvious staging ground — the Constable can no longer simply lean on your counter, because you are rarely behind it. Suspicious requests recover most of that, but what a *beat* actually is, how often one fires, and how the town's goodwill mechanically shields you are all undesigned. See Part 4.

**The town is your armor.** Trust and restoration are not only progression; they are protection. Neighbors who like you lie for you. This is the fusion: the loop that is already fun does double duty as the tension system.

**This is emphatically not the heat meter that was deleted in Phase D.** That was an invisible number accruing against the player passively, and it was right to kill it. This is discrete, authored, legible moments where the player makes a visible choice — which is what the "no hidden dice" guardrail actually asked for.

**Design requirements for any Constable beat:**

1. The player must be able to **name the choice they made** afterward
2. No accumulating invisible state
3. Failure costs **standing or opportunity**, never a locked-out path
4. It happens in daylight, in town, in the middle of the cozy part — that is the whole point

### Infrastructure

**Public sockets** (street): lampposts, plank sidewalks, benches, flower boxes, signs. Night light, stand-traffic buffs at beauty thresholds, NPC dialogue reactions. **Pure positives — beautification is never punished.**

Covert forest sockets (stash barrels, trail markers, shortcut planks) **are cut** — they existed to serve delivery runs.

Placement is into predefined sockets. No free placement system.

### Guardrails — non-negotiable

1. **Never punish the player for playing.** Day life carries no loss risk
2. **Legibility over drama.** No hidden dice. Every setback traces to a nameable choice
3. **Act 0 is a prologue, not an act.** 20-40 minutes, hard
4. **Appointments create "one more day," not FOMO.** Nothing is permanently missable
5. **Beautification is never punished**
6. **Restoration must always double as defense.** If a new system doesn't connect care for the town to safety of the operation, question whether it belongs

---

## Part 4 — Open problems

### Design threads — status board

Settled so far:

| Thread | Settled | Where |
|---|---|---|
| **The stand + request book** | 2026-07-25 | Part 3 — full design |
| **Delivery runs cut** | 2026-07-25 | Below |
| **Homestead shell vs. site** | 2026-07-25 | Parts 1 and 3 |
| **Danger relocates to the front** | 2026-07-25 | Part 3 — *partially* designed |

Still open, in dependency order. **Work them roughly in this sequence** — the ordering reflects real blocking, not preference:

| # | Thread | What's unresolved | Blocks |
|---|---|---|---|
| **1** | **The guardrail contradiction** ← *next* | Can daytime play cost the player anything? | #3 |
| 3 | **The Constable** | What a beat is, how often one fires, what a choice looks like, how the town's goodwill mechanically shields you. Venue exists (bait notes); system does not | #4 partly |
| 4 | **What is night for?** | Three candidates, none chosen. The blocking pillar question | #5 |
| 5 | **Side activities** | Filter written, candidates unscored, none chosen | — |
| 6 | **Homestead site upgrades** | Which upgrades, what order, what cost | #7 |
| 7 | **Unlock cadence** | The 2-3-visible rule has never been tested against real content | — |
| 8 | **Berta's recruitment beat** | Her trigger evaporated with the runs. Best beat in the design, currently orphaned | — |
| 9 | **Recipe book → cellar** | How the grandfather's book seeds the mystery. Small, self-contained, cheap | — |
| 10 | **The moral axis** | Post-slice. Three resolutions listed, none chosen | — |

### ⚠ The guardrail contradiction (thread #1)

**Unresolved, and it now blocks real work.**

`BuildPlan.md` rules say *"never punish daytime play."* Guardrail 1 in this document says *"Day life carries no loss risk."*

But the Constable system in Part 3 says failure costs *"standing or opportunity"* — and standing is a loss that happens in daylight. **This document currently contradicts itself.**

Suspicious requests sharpened the problem rather than easing it: filling a bait order has to cost *something*, or the one tension mechanic in the primary economy is empty theatre.

Two candidate resolutions, unchosen:

- **Narrow the guardrail** to "no loss of cash, goods, or progress during the day" — leaving social standing as fair game. More interesting, probably right
- **Constable can only cost upside not yet earned** — he takes away potential, never anything held. Safest, possibly toothless

Settle this before designing the Constable or scheduling any phase around him.

### `BuildPlan.md` reconciliation — the audit

Full line-by-line audit performed 2026-07-25. Twelve problems, priority order:

| # | Problem |
|---|---|
| 1 | **Header points at the superseded spec** — directs every future worker to the wrong document. Cheapest fix, highest priority |
| 2 | **Slice summary describes the old game** — "night = opt-in delivery runs" · "keep the woods dark" · "deep woods (routes, destinations)" · "stand (safe channel)" understates it · "two-layer infrastructure" is now one |
| 3 | **Phase 4 entirely dead** — routes, appointments, patrols, catching, bribes, load-outs, courier |
| 4 | **Phases 3 and 5 half dead** — near forest and public sockets survive; deep woods, corridors, destinations, stash barrels, trail markers, shortcut planks, lookout perch do not. Phase 3's darkness pass is *blocked* on thread #4 |
| 5 | **No phase exists for the stand or the Constable** — the primary economy and the entire tension system are unscheduled |
| 6 | **Broken metrics** — Phase 7 "first night run ~1 h" · Phase 9 "time to first night run (<75 min)" and "caught-players can name their mistake… patrol bug" · Phase 8 "night-run ambience layer". Two of Phase 9's six validation metrics measure a system that won't exist |
| 7 | **Orphaned completed work** — Phase D's bribe rework has nothing to catch you; guards were kept explicitly for "Phase 3 repurposes them onto routes," which no longer happens. *Silver lining: the finished Guard sprite becomes the Constable's* |
| 8 | **Berta's beat trigger gone** — "catches you, covers unprompted" was built on smuggling |
| 9 | **Aksel loses the handcart, Boarding House loses the courier** — both still listed in Phase 7 with dead functions |
| 10 | **Pre-existing bug, unrelated:** Phase D says reputation dies in Phase 5; Phase 6 says it dies there. Phase 6 is correct |
| 11 | **Phase 1 doesn't know about shell-vs-site** — still reads as a one-time build unlocking "proper still + vat" |
| 12 | **`tormodLeaveHour = -1` untracked** — shipped-but-wrong against old spec and this doc; no line item owns it |

Net: Phase 4 dies, Phases 3 and 5 halve, Phases 1/2/6/7/8/9 need edits, two new systems need phases that don't exist. **Resolve thread #1 first** — the guardrail wording determines what a Constable phase is allowed to contain.

### The runs decision (2026-07-25) — recorded reasoning

Delivery runs, routes, patrols, and load-outs are **cut**. Not deferred — cut.

**Why.** The guardrails and the runs were at war. "Never punish the player for playing" versus a system whose entire purpose is punishment. Each defanging step was individually correct — heat meter deleted, random spawns forbidden, patrols fixed and learnable, caught costs only carried cargo, bribe to keep even that, nothing permanently missable. Follow the chain to its end: **getting caught costs a fee you can afford.** The tension was theater, and it was the most expensive system in the plan.

**What it takes with it:**

| System | Fate |
|---|---|
| BuildPlan Phase 4 entire | Dead |
| Phase 3 deep woods — route corridors, logging camp, river dock, crossroads | Loses its justification. Near forest survives for foraging |
| Phase 5 covert sockets | Dead. Public sockets untouched |
| `Guard.cs` / `GuardManager` | **Orphaned.** Kept through Phase D specifically to be repurposed onto routes. Not deleted yet |
| Load-outs (satchel → handcart → courier) | Dead. Aksel loses the handcart payoff; still upgrades and barrels survive |
| Boarding House operation role | **Needs redesign** — it housed the courier |
| Appointments | **Survive, relocated** to the stand as demand events |
| Mill cellar, restoration, NPCs, infrastructure | Untouched |

**`BuildPlan.md` now contradicts this document in Phases 3, 4, and 5.** It needs its own revision pass. That is deliberately not done here.

### ★ The biggest open question: what is night for?

**Undecided. This deserves its own brainstorm.**

The core pillar is *day = the front, night = the operation.* Runs were the only night content in the design. Without them, night risks becoming a fast-forward button and sleep a skip key — which would make the day/night cycle, the lighting system, and a central pillar into decoration.

Candidates, none chosen:

- **Night is short and consequential.** Not an activity block but a scene: the day's fallout lands, someone knocks, the Constable was asking about you. Then you sleep
- **Night is quiet, cozy work.** Tending ferments, reading fragments, planning tomorrow's production against known requests, decorating. Night is when you plan; day is when you act
- **Night keeps covert activity** — things doable only unobserved. Danger is being *seen*, not being caught, and witnesses feed daylight social pressure

**Nothing else in this document should be built until this is answered**, because it determines whether the day/night axis survives at all.

### Side activities — the second verb

Weakness ③. The stand adds decision to brewing but not a second thing to do.

**The filter — a good Lamplight side activity:**

1. Uses verbs the player already has — walk, interact, carry, place. No new input systems
2. Feeds the main loop *indirectly*, so skipping is never strictly optimal
3. Runs on a **different clock than fermenting**, so it fills brew-waits rather than competing with them
4. Reinforces *"this is my town"* or *"I know these woods"*

**Unscored candidates:** fishing or trapping in the near forest · NPC favors and requests · decorating and furnishing interiors · mapping the woods as a collection · cooking and meals · hauling contracts for neighbors · the fragment hunt as an actual activity rather than a passive drop.

Score these against the filter before building any of them.

### The moral axis — deferred past the slice

Money versus helping people. Explicitly **not in the slice**.

**The tension to resolve first:** moral choice only has weight if choosing people *costs* something, but guardrail 1 forbids punishing the player. Candidate resolutions, unchosen:

- Costs cash only — the renewable resource. Never time, safety, or a closed path
- A currency conversion: money buys upgrades, generosity buys trust. Two builds, no loss — but arguably no moral weight either, just a second shop
- Genuinely closed doors. Maximum weight, directly bends guardrail 1

Design this when the town and its people actually exist to make choices about.

### Smaller open items

- **Boarding House** needs a new operation role
- **`Guard.cs` / `GuardManager`** are orphaned — decide whether the Constable reuses any of it before deleting
- **Deep woods** need a reason to exist, or the world shrinks to town + near forest
- **The grandfather's recipe book** should be wired to the cellar — cheapest high-value change available
- **`tormodLeaveHour = -1`** contradicts the design; Tormod should be dusk-only and Act 0-scoped

---

## Revision log

| Date | Change |
|---|---|
| 2026-07-22 | Front-town empire redesign approved (see superseded spec) |
| 2026-07-25 | Master doc created. Stand-with-requests becomes the primary economy · homestead split into shell + ongoing site · **delivery runs cut** · danger relocated to the front as a social system · "what is night for?" flagged as the blocking open question |
| 2026-07-25 | **Stand designed in full** (Part 3). Roadside at the homestead, town storefront as a mid-game channel unlock · stand needs no tending; requests arrive as written notes in a book · the book is a correspondence, with signed notes and replies · customer mix shifts strangers → named residents and *is* the progress meter · requests are exact with descriptive spikes that point at unlocks · suspicious notes are the one tension mechanic, recovering the Constable's staging ground · conflicting requests and quality-reputation penalties considered and cut |
