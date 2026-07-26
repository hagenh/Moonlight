# LAMPLIGHT — Game Design

**Status:** Living document. Last major revision 2026-07-25.

**This document supersedes** `docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md`. That spec remains as the dated record of what was approved on 2026-07-22; where the two disagree, this document wins.

**What this document owns:** the player's experience — what they do, why they keep doing it, and where the design is weak.

**What it does not own:**

- `Assets/Docs/BuildPlan.md` — build order and phase status. Reconciled against this document on 2026-07-25; see Part 4 for the audit that drove it.
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
| Genre position | **Cozy by design.** No mechanical edge anywhere — not in the day, not at night. Settled 2026-07-25, see Part 4 |
| The day/night axis | *Day is when you act; night is when the day answers back.* Pacing and mood, not two modes of play. Settled 2026-07-25 (thread #4) |
| Where danger lives | **Nowhere mechanical.** The Constable is narrative pressure and atmosphere. He is never a loss system |
| What protects you | **The town.** Neighbors who like you lie for you. Restoration is literally defensive — as fiction and as story, not as a defence stat |
| Primary economy | The roadside stand and its **request book** — written orders with generous deadlines |
| NPC role | Co-conspirators: day job (front) + operation role (function) + recruitment beat (story) |
| Town agency | Player-placed public infrastructure in predefined sockets |
| Narrative | Recruitment beats, conspiracy trust, the Mill cellar |
| World | One connected exterior scene; interiors separate small spaces |

**The central fusion:** restoring the town and defending the operation are the *same action*. That is why the front-town fantasy holds, and it is the test every new system must pass.

> **Read this as fiction, not mechanics** (cozy decision, 2026-07-25). Nothing is attacking the operation, so nothing is mechanically defending it. The fusion survives as the reason the fantasy coheres and as the register every recruitment beat is written in — not as a safety stat the player is accumulating.

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

> **Fixed 2026-07-25 (uncommitted at time of writing).** This document previously recorded a known bug: `tormodLeaveHour = -1` with a one-shot spawn in `Start()`, making Tormod a permanent always-open vending machine. `SellManager` now keeps him to an 18:00-06:00 window via `SellerRules.IsPresent`, which handles the wrap past midnight, and settles presence once at startup so he isn't missing until the clock next ticks. Covered by `Assets/Tests/EditMode/SellerRulesTests.cs`.

### Act 1 — The day game opens (~40 min - 2h)

**Not built.** This is the largest new design in this document.

The homestead shell gives the player an **address**, and an address is what a stand requires. You cannot run a business out of a tent in the woods.

**The roadside stand opens**, and with it **the request book**. This is the primary economy for the rest of the game. Full mechanics in Part 3.

> **The stand must be the first site upgrade, and it must be cheap and immediate** — buildable within minutes of the shell completing, not after another long material grind. This is not a tuning preference. Weakness ② is closed only if the new hook arrives *as* the old one runs out; putting a second grind between the shell and the stand re-opens the exact vacuum the stand exists to fill.

Why this matters more than the price: **it converts production from a routine into a plan.** Before the stand, brewing has no decision content — you make the only recipe you have, as much as you can. After it, brewing is a question: which recipe, how many, for whom, in what order, and what do I start tonight so it's ready Thursday.

**The morning ritual.** Walk out to the stand, read the book, decide what to brew. That is the loop's new heartbeat, and it is deliberately a physical object in the world rather than a menu.

Tormod pays you. The book tells you what to make. That information is the real unlock.

**Also in Act 1:** the Roadhouse becomes the effortless overflow channel · the first town building purchase · the Constable notices you for the first time · **the first night beat** — the player comes home to find something waiting at the fire, and learns that nights are sometimes not empty.

### Act 2 — The town, the Constable, the Mill (~2-6h)

**Not built.**

- **Restoration compounds.** Each building buys recipes, people, and features. Buildings are the progression gate
- **Recruitment beats.** Berta catches you mid-something and covers for you *unprompted* — then you talk. The best beat in the design, because it demonstrates the thesis instead of stating it
- **The Constable starts appearing** (see Part 3). Daylight questions, awkward conversations, a man who is always slightly too interested — and who never actually takes anything from you
- **The town becomes armor** — in the fiction. Neighbours cover for you in dialogue and in story beats. Nothing is being subtracted, so nothing is being defended against
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

**There is no seventh hook, and there is deliberately no tension hook.** With the cozy decision of 2026-07-25, hooks 5 and 6 — transformation and the question — carry the load that danger would have carried in another game. That is a real bet: **the game's pull is the town changing and the cellar's answer, not jeopardy.** If the slice fails to hold players, these two are where to look first.

### The cadence rule

**At any moment, the player should have 2-3 visible unlocks in reach — at different price points, in different categories.** One cheap and soon, one mid, one aspirational but named.

The eight unlock categories: **capacity · recipes · channels · access · people · automation · expression · knowledge.**

This rule is the mechanism that prevents hook vacuums. It is checkable, which is what makes it a design rule rather than a mood.

### Weaknesses — stated plainly

**① Everything past minute 40 is a hypothesis.** The game currently ends where Act 0 ends. Hooks 4, 5, and 6 — planning, transformation, and the question — have never been played by anyone. The build order is right; the honesty is the point.

**② The 40-75 minute vacuum — now answered, but unproven.** Previously this stretch ran on ratchet alone, the same hook Act 0 had just spent twenty minutes on, only with a bigger number. The stand is the fix: a genuinely new hook arrives exactly when the old one runs out. **This is the single most important thing to prototype next**, because the whole mid-game rests on it.

**③ The daily loop still has one verb.** Foraging feeds brewing; building spends brewing income. Everything routes through moonshine. The stand adds *decision* to that verb, which helps a great deal — but it does not add a second thing to do. See Part 4.

**④ The cellar fires once, at the end, and a free setup is going unbuilt.** The grandfather's ruined recipe book — mostly destroyed, one legible page — would let the player carry a mysterious damaged object from minute zero, seeding hour six's payoff at minute three.

> **Correction, 2026-07-25:** this document previously described the book as existing ("currently pure Act 0 flavor"). **It does not exist** — not in code, not as an asset, not in the scene. It appears only in the superseded 2026-07-22 spec. Thread #9 is therefore *build it, then wire it*, not *wire it*. Still cheap, no longer free.

**⑤ ~~The primary economy has no edge.~~ Resolved 2026-07-25 — by decision, not by design.**

This was recorded as a weakness on the assumption that a twenty-hour game needs a mechanical edge somewhere, and that thread #4 would site it at night. **That assumption is now rejected.** Lamplight is cozy on purpose: there is no jeopardy system anywhere in the game, and the absence is a genre position rather than a gap. See "The cozy decision" in Part 4.

What remains in the economy is triage — limited ingredients and limited time against a book of requests — and triage is now understood as *sufficient*, because it is the only kind of pressure the game is trying to exert.

> **The honest counter-argument, kept on the record:** cozy games that work usually still have a pressure gradient — a season that ends, a stamina bar, a debt. Lamplight currently has none of these. **The thing to watch in playtest is whether days feel like they have a shape**, or whether the player drifts. If they drift, the fix is a *soft* clock (a reason to want the day to end well), not the reintroduction of loss.

---

## Part 3 — Reference

### The economy

| Channel | Price/unit | Volume | Constraint | Risk | When |
|---|---|---|---|---|---|
| **Tormod at the back door** | Low, flat | Whatever you have | None | None | Act 0 only |
| **Roadside stand — shelf** | Low | Whatever you stock | None | None | Act 1 onward, passive |
| **Roadside stand — requests** | High | Low — you make what was asked | **Knowing what to make** | None | Act 1 onward, **primary** |
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

#### Tension: triage, and only triage

The book's tension is **triage** — limited ingredients and limited time mean you cannot fill everything, so choosing what to brew has a cost in what you didn't. Nothing else.

**Deliberately excluded:** conflicting requests (two customers wanting the same scarce thing) and quality-reputation penalties, both cut 2026-07-25 to keep the morning ritual clean. **Bait notes are cut too** — see thread #1 in Part 4. They were the design's only source of daytime loss and they contradicted guardrail 1; the full design is parked in `LaterIdeas.md`.

> **Triage is the whole of it, and that is now the intended end state** — not a gap awaiting a fix. The cozy decision of 2026-07-25 means no edge is coming from anywhere, so the book is not under-built; it is finished. Bait notes stay parked permanently unless the genre position itself is revisited.

#### Capacity

The number of simultaneously active requests starts small and grows through upgrades — stand improvements, then the storefront. Board space is a **capacity unlock**, and more visible demand means deeper planning.

#### Open numbers

Deferred to implementation: request arrival rate · premium size over shelf price · exact active-request counts per upgrade tier · deadline lengths.

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

**The Constable's office is never purchasable.** It sits on the street, light always on — the one window you didn't light. Deliberate exception to "every building aids progression." It is the game's unease rendered as level geometry, and with the cozy decision it is now doing that job almost alone — which makes it *more* important, not less.

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

### The Constable — pressure without stakes

**Reframed 2026-07-25.** He replaced the delivery-run system; the cozy decision then removed the last reason for him to be a system at all.

**Constable Aas is a character, not a mechanic.** He is polite, patient, unhurried, and always slightly too interested. He appears in daylight, in the cozy part of the game, and he never takes anything from the player — not cash, not goods, not progress, not standing, not opportunity.

What he does instead is **make the fiction feel true**. The player is running an illegal business in a small town where one man has noticed. That produces unease without producing loss, and unease is exactly what a cozy game is allowed to have.

His appearances are authored, low-frequency, and **conversational**:

- He is on the street when you walk through town with a full satchel, and he says good morning
- He mentions, apropos of nothing, that he knew the family who used to own the Mill
- He asks a neighbour about you — and you hear about it afterward, from the neighbour, as a story
- He compliments the lamppost you put up

> **The chief risk, named:** a character with no mechanical teeth can read as toothless — the player learns after two hours that he never does anything, and he becomes scenery. **The mitigation is that he must never threaten anything the player can evaluate as a bluff.** He is not a warning of consequences that never arrive; he is a man who knows and has not decided what to do about it. Write him as unresolved, never as pending. This is a writing problem now, not a systems problem.

**The town is your armor — as fiction.** Neighbours who like you cover for you in dialogue, in recruitment beats, and in what they tell him. Nothing is being subtracted, so this is not a defence stat; it is the payoff of restoration told back to the player as story. The fusion in Part 0 survives, in a narrative register instead of a mechanical one.

**This is emphatically not the heat meter deleted in Phase D** — nor a quieter version of it. There is no accumulating state, hidden or visible, and nothing to accumulate toward.

**Design requirements for any Constable beat:**

1. It costs the player **nothing at all**. Not cash, goods, progress, standing, or opportunity
2. No accumulating state, hidden or visible
3. It happens in daylight, in town, in the middle of the cozy part — that is the whole point
4. It leaves the player *knowing something they did not know*, or *feeling seen*. That is the entire payload

> **Thread #3 is unblocked and radically smaller than it was.** It is no longer "design a tension system." It is "write a recurring character and decide how often he turns up." What remains open: appearance frequency, whether his lines react to restoration progress, and whether he ever appears at night (see thread #4's answer — probably yes, once).

### Infrastructure

**Public sockets** (street): lampposts, plank sidewalks, benches, flower boxes, signs. Night light, stand-traffic buffs at beauty thresholds, NPC dialogue reactions. **Pure positives — beautification is never punished.**

Covert forest sockets (stash barrels, trail markers, shortcut planks) **are cut** — they existed to serve delivery runs.

Placement is into predefined sockets. No free placement system.

### Guardrails — non-negotiable

1. **Never punish the player for playing.** **No loss anywhere, at any hour** — no cash, no goods, no built progress, no standing, no closed doors. Unconditional; the design contains no exception. Widened from "day life" to the whole game by the cozy decision, 2026-07-25
2. **Legibility over drama.** No hidden dice. Every setback traces to a nameable choice — and there are, by guardrail 1, almost no setbacks
3. **Act 0 is a prologue, not an act.** 20-40 minutes, hard
4. **Appointments create "one more day," not FOMO.** Nothing is permanently missable
5. **Beautification is never punished**
6. **Restoration must always double as defense — in the fiction.** If a new system doesn't connect care for the town to the safety of the operation *as story*, question whether it belongs. This is no longer a mechanical requirement, because there is nothing to defend against
7. **Cozy is the genre, not a fallback.** Any proposal that reintroduces jeopardy — loss, failure states, timers that punish, a resource that can hit zero badly — is rejected by default. Overturning this means overturning the genre position deliberately and in writing, not smuggling an exception through a single feature

---

## Part 4 — Open problems

### Design threads — status board

Settled so far:

| Thread | Settled | Where |
|---|---|---|
| **The stand + request book** | 2026-07-25 | Part 3 — full design |
| **Delivery runs cut** | 2026-07-25 | Below |
| **Homestead shell vs. site** | 2026-07-25 | Parts 1 and 3 |
| **Danger relocates to the front** | 2026-07-25 | Part 3 — superseded by the cozy decision below |
| **The guardrail contradiction** (#1) | 2026-07-25 | Below — **no.** Day life costs nothing; bait notes cut |
| **The cozy decision** (tone) | 2026-07-25 | Below — **no mechanical edge anywhere.** The parent decision that settled #4 |
| **What is night for?** (#4) | 2026-07-25 | Below — **night is a scene, not an activity block** |

Still open, in dependency order. **Work them roughly in this sequence** — the ordering reflects real blocking, not preference:

| # | Thread | What's unresolved | Blocks |
|---|---|---|---|
| **3** | **The Constable** ← *next* | **Radically reduced by the cozy decision** — no longer a tension system, now a recurring character. Open: how often he appears, whether his lines track restoration progress, his one night appearance. A writing job | — |
| 5 | **Side activities** | Filter written, candidates unscored, none chosen. **Now more load-bearing** — with no tension, variety is what keeps the middle hours alive | — |
| 6 | **Homestead site upgrades** | Which upgrades, what order, what cost | #7 |
| 7 | **Unlock cadence** | The 2-3-visible rule has never been tested against real content | — |
| 8 | **Berta's recruitment beat** | Her trigger evaporated with the runs, and the cozy decision rules out rebuilding it on danger. **She needs a non-jeopardy version of "catches you and covers unprompted"** — the beat is worth saving; only its trigger is broken | — |
| 9 | **Recipe book → cellar** | How the grandfather's book seeds the mystery — and the book itself, which **does not exist yet**. Small, self-contained, cheap. **Earliest thing the player would meet (minute 3), and now higher value** since hook 6 carries more of the game's pull | — |
| 10 | **The moral axis** | Post-slice. Down to two resolutions after the cozy decision eliminated "closed doors" — and may not be worth building at all | — |

### The guardrail contradiction (thread #1) — settled 2026-07-25

**Answer: no. Daytime play costs the player nothing.**

The contradiction was that guardrail 1 promised *"day life carries no loss risk"* while the Constable's beats were required to cost *"standing or opportunity"* — and standing is earned, and lost in daylight.

**Resolved by cutting bait notes**, which were the design's only source of daytime loss. The contradiction dissolves at its source rather than being negotiated around.

Three consequences, all folded into this document:

1. **Guardrail 1 stands, with explicit scope.** No cash, no goods, no built progress, no standing. Unconditional
2. **Constable beats cost opportunity only** — a door that never opens. "Standing" struck from the beat requirements
3. **Bait notes parked in `LaterIdeas.md`**, design intact, in case night turns out to need a daylight counterpart

**Why cut rather than narrow the guardrail.** Pinning the fiction decided it: the notes are written by *the law*, not by careless customers. Filling one means handing contraband to a guard who wanted exactly that. The honest consequences of that are evidence, confrontation, or surveillance — and each is either a heat meter in costume (deleted in Phase D, correctly) or a daylight loss of something held. None survive contact with guardrail 1.

**What it costs.** The request book keeps its planning hook but loses its edge — see weakness ⑤. And the Constable loses his last daytime venue, which makes thread #3 harder, not easier.

**That is the correct order of operations.** Bolting danger onto the day before answering the pillar question is what produced this contradiction in the first place.

> **Epilogue, same day.** Thread #4 was then taken up and answered by rejecting the question behind it: the game needs no edge at all. In hindsight this thread was the last of several attempts to place a tension system that the design had never actually decided it wanted. **The cut recorded here was correct, but for a larger reason than the one given at the time.**

Full record: `docs/superpowers/specs/2026-07-25-guardrail-contradiction-design.md`.

### `BuildPlan.md` reconciliation — the audit

Full line-by-line audit performed 2026-07-25. Twelve problems, priority order:

| # | Problem |
|---|---|
| 1 | **Header points at the superseded spec** — directs every future worker to the wrong document. Cheapest fix, highest priority |
| 2 | **Slice summary describes the old game** — "night = opt-in delivery runs" · "keep the woods dark" · "deep woods (routes, destinations)" · "stand (safe channel)" understates it · "two-layer infrastructure" is now one |
| 3 | **Phase 4 entirely dead** — routes, appointments, patrols, catching, bribes, load-outs, courier |
| 4 | **Phases 3 and 5 half dead** — near forest and public sockets survive; deep woods, corridors, destinations, stash barrels, trail markers, shortcut planks, lookout perch do not. Phase 3's darkness pass is **unblocked** by thread #4, but must be rescoped from "dark woods to sneak through" to "the homestead at night, and the lit town seen from the treeline" |
| 5 | **No phase exists for the stand, the Constable, or night beats** — the primary economy and all remaining night content are unscheduled |
| 6 | **Broken metrics** — Phase 7 "first night run ~1 h" · Phase 9 "time to first night run (<75 min)" and "caught-players can name their mistake… patrol bug" · Phase 8 "night-run ambience layer". Two of Phase 9's six validation metrics measure a system that won't exist |
| 7 | **Orphaned completed work** — Phase D's bribe rework has nothing to catch you; guards were kept explicitly for "Phase 3 repurposes them onto routes," which no longer happens. *Silver lining: the finished Guard sprite becomes the Constable's* |
| 8 | **Berta's beat trigger gone** — "catches you, covers unprompted" was built on smuggling |
| 9 | **Aksel loses the handcart, Boarding House loses the courier** — both still listed in Phase 7 with dead functions |
| 10 | **Pre-existing bug, unrelated:** Phase D says reputation dies in Phase 5; Phase 6 says it dies there. Phase 6 is correct |
| 11 | **Phase 1 doesn't know about shell-vs-site** — still reads as a one-time build unlocking "proper still + vat" |
| 12 | ~~**`tormodLeaveHour = -1` untracked**~~ — **fixed 2026-07-25.** Tormod now keeps an 18:00-06:00 window via `SellerRules.IsPresent`, tested. BuildPlan still needs a line item acknowledging it |

> **Resolved 2026-07-25.** All twelve items are addressed — `BuildPlan.md` has been reconciled and the guard system deleted. This table is kept as the record of what was wrong, not as a work queue.

Net: Phase 4 dies, Phases 3 and 5 halve, Phases 1/2/6/7/8/9 need edits, two new systems need phases that don't exist.

**All blockers on item 5 were discharged.** Threads #1 and #4 and the cozy decision between them settled what a Constable phase and a night phase may contain: authored, zero-cost, unmissable content and nothing else. Item 5 was then scheduled as **Phase S** (the stand and request book) and **Phase N** (night beats + Constable appearances), the latter mostly writing.

**Item 7 resolved as delete, and the deletion is done.** `Guard.cs` / `GuardManager` were kept through Phase D to be repurposed onto routes, then briefly to write and collect bait notes. Runs are cut, bait notes are out of genre, and the cozy decision guaranteed nothing would ever need patrol or detection code. **`Guard.cs`, `GuardManager`, `BribeUI`, the three bribe events, `Guard.prefab`, and all of their scene objects were deleted 2026-07-25.** The finished Guard *sprite* is still reused as Constable Aas; that is an art asset, not the script.

**Item 6 grew by one.** Phase 3's darkness pass was unblocked but its stated purpose ("night in the woods is genuinely dark") served the runs. It is now rescoped in `BuildPlan.md` to what night is actually for: the homestead at night, and the lit town seen from the treeline.

### The runs decision (2026-07-25) — recorded reasoning

Delivery runs, routes, patrols, and load-outs are **cut**. Not deferred — cut.

**Why.** The guardrails and the runs were at war. "Never punish the player for playing" versus a system whose entire purpose is punishment. Each defanging step was individually correct — heat meter deleted, random spawns forbidden, patrols fixed and learnable, caught costs only carried cargo, bribe to keep even that, nothing permanently missable. Follow the chain to its end: **getting caught costs a fee you can afford.** The tension was theater, and it was the most expensive system in the plan.

**What it takes with it:**

| System | Fate |
|---|---|
| BuildPlan Phase 4 entire | Dead |
| Phase 3 deep woods — route corridors, logging camp, river dock, crossroads | Loses its justification. Near forest survives for foraging |
| Phase 5 covert sockets | Dead. Public sockets untouched |
| `Guard.cs` / `GuardManager` | **Deleted 2026-07-25.** Orphaned by the runs cut, then finally by the cozy decision — nothing in the design will ever need patrol or detection code. `BribeUI` and the three bribe events went with them. The Guard *sprite* still becomes Constable Aas |
| Load-outs (satchel → handcart → courier) | Dead. Aksel loses the handcart payoff; still upgrades and barrels survive |
| Boarding House operation role | **Needs redesign** — it housed the courier |
| Appointments | **Survive, relocated** to the stand as demand events |
| Mill cellar, restoration, NPCs, infrastructure | Untouched |

> **`BuildPlan.md` was reconciled on 2026-07-25** and no longer contradicts this document. Phase 4 is a tombstone, Phases 3 and 5 are halved, and Phases S and N carry the stand and the night beats.

### The cozy decision (tone) — settled 2026-07-25

**Answer: Lamplight has no mechanical edge, anywhere, on purpose.**

The question put was whether the game needs jeopardy and, if so, where it lives. Both prior threads had assumed the answer was yes and argued only about location — thread #1 evicted it from the day, which left night holding it by default.

**Rejected.** Lamplight is a restoration game with a criminal skin. The criminality is *flavour, fantasy, and story*; it is not a risk system and never becomes one. Weakness ⑤ is closed by this, and guardrail 7 now protects it.

**What this buys.** Every remaining design question gets easier, because the hardest constraint in the document — build tension that never costs the player anything — was a contradiction the design kept paying interest on. Thread #3 shrinks from a system to a character. Thread #1's cut stops being a sacrifice. Bait notes stop being parked and become simply out of genre.

**What this costs, stated plainly.** The pull of the game now rests entirely on hooks 5 and 6 — *transformation* (the town visibly changing) and *the question* (the cellar). Neither has ever been played. If the slice does not hold testers, the cause is almost certainly here, and the fix is to strengthen those two, **not** to reintroduce danger.

Full record: `docs/superpowers/specs/2026-07-25-cozy-decision-and-night-design.md`.

### What is night for? (thread #4) — settled 2026-07-25

**Answer: night is a scene, not an activity block.**

Night is not a second work shift, a covert window, or a planning phase. It is the short warm tail of the day in which **the day's story occasionally lands**, followed by sleep. Most nights, nothing happens and the player simply goes to bed — and that is correct, not a shortfall.

#### Grounding — what night actually is in the build today

This was never written down and it constrains the answer, so it is recorded here:

| | |
|---|---|
| Real time per game hour | ~46 s (`TimeManager.realSecondsPerGameMinute = 0.77`) |
| Playable day | 08:00 → 24:00 = **~12.3 real minutes** |
| Dusk onward (19:00+) | **~3.8 real minutes** |
| Genuinely dark (21:00+, light intensity ≤ 0.5) | **~2.3 real minutes** |
| Midnight | `CurfewReached` forces sleep — `TimeManager.cs:58` → `SleepManager.cs:34` |
| Voluntary sleep | Permitted from 21:00 — `Bed.cs:9` |

**Night is already a two-to-four minute tail ending in a hard cut.** That rules out the covert-activity candidate on its own — there is not enough night to *do* anything in — and it means the scene answer is close to what is already shipped. **No clock retuning is required.**

#### How a beat reaches the player

**Beats wait at the homestead.** Every day ends by going home to sleep, so beats live where sleep already lives: someone sitting at your fire, a note weighted under a stone, a lamp lit in a window that was dark yesterday.

This makes them **unmissable by construction** — no telegraphing, no appointment to keep, no way to sleep through one — which satisfies guardrail 4 without adding a scheduling system. It also gives the walk home a reason to exist and reinforces the homestead-as-permanent-site principle from Part 3.

#### What a beat is made of

Cozy register, matching the tone decision. A beat is **warmth, story, or the cellar mystery** — never a threat and never a bill:

- A recruit is waiting at your fire because they had nowhere else to go
- A fragment of the old operation's story surfaces — left, found, or remembered
- Someone thanks you for something you did days ago and had forgotten
- The Constable, exactly once and memorably, is simply standing in the road

**A beat leaves the player knowing something, or feeling something. It never changes their inventory.**

#### The pillar, restated honestly

*"Day = the front, night = the operation"* **is retired.** It described the delivery-run game, which is cut, and no honest reading of the design supports it any more.

**Replaced by:** *day is when you act; night is when the day answers back.* The day/night cycle survives as pacing and mood rather than as two modes of play, and the lighting system survives because dusk over a lit street is one of the best images the game has — see Act 0, where a dark street of boarded windows is already doing real work.

#### Open — deferred to implementation

Beat frequency · how beats are authored and triggered (milestone, day count, or hand-placed) · whether empty nights get any small ambient reward · whether the 21:00 sleep floor should move earlier now that night has content worth encountering.

Full record: `docs/superpowers/specs/2026-07-25-cozy-decision-and-night-design.md`.

### Side activities — the second verb

Weakness ③. The stand adds decision to brewing but not a second thing to do.

> **Promoted by the cozy decision.** In a game with jeopardy, variety is a nice-to-have. In a game without it, **variety is the retention mechanism** — there is nothing else keeping the player in hours 3-10 but the pleasure of having things to do. Thread #5 is now second in the queue behind #3, and should be treated as core rather than garnish.

**The filter — a good Lamplight side activity:**

1. Uses verbs the player already has — walk, interact, carry, place. No new input systems
2. Feeds the main loop *indirectly*, so skipping is never strictly optimal
3. Runs on a **different clock than fermenting**, so it fills brew-waits rather than competing with them
4. Reinforces *"this is my town"* or *"I know these woods"*

**Unscored candidates:** fishing or trapping in the near forest · NPC favors and requests · decorating and furnishing interiors · mapping the woods as a collection · cooking and meals · hauling contracts for neighbors · the fragment hunt as an actual activity rather than a passive drop.

Score these against the filter before building any of them.

### The moral axis — deferred past the slice

Money versus helping people. Explicitly **not in the slice**.

**The tension to resolve first:** moral choice only has weight if choosing people *costs* something, but guardrail 1 forbids punishing the player. Candidate resolutions:

- Costs cash only — the renewable resource. Never time, safety, or a closed path. **Still live.** Spending money is not losing it, so guardrail 1 permits this
- A currency conversion: money buys upgrades, generosity buys trust. Two builds, no loss — but arguably no moral weight either, just a second shop. **Still live**
- ~~Genuinely closed doors. Maximum weight, directly bends guardrail 1~~ — **eliminated 2026-07-25.** Guardrail 7 forbids it outright, and the cozy decision means the door it was bending was load-bearing after all

**Consequence: the axis is now a choice between two low-stakes options, and may not be worth building at all.** Decide that honestly when the town and its people exist — a moral axis with nothing at stake is a menu, and the design should be willing to cut it rather than ship a hollow one.

### Smaller open items

- **Boarding House** needs a new operation role
- ~~**`Guard.cs` / `GuardManager`**~~ — **deleted 2026-07-25**, along with `BribeUI`, the three bribe events, `Guard.prefab`, and their scene objects. The sprite is kept for Constable Aas
- **Deep woods** need a reason to exist, or the world shrinks to town + near forest. **Now more pressing** — the cozy decision removed the last candidate reason (covert night activity), so either thread #5 gives the woods a use or they are cut
- **The grandfather's recipe book** does not exist yet and should be built, then wired to the cellar — see the correction under weakness ④. **Now higher value:** with hook 6 carrying more of the game's pull, seeding the cellar early matters more than it did
- ~~**`tormodLeaveHour = -1`**~~ — **fixed 2026-07-25.** Tormod is dusk-to-dawn via `SellerRules`. What remains open is the *design* half: he should also stop being the primary channel once the stand opens, which is Act 1 work
- **Bait notes** stay in `LaterIdeas.md` and are now **out of genre, not merely parked.** Reviving them means overturning guardrail 7 in writing
- **The 21:00 sleep floor** (`Bed.cs:9`) may want to move now that night has content worth encountering. Open number under thread #4

---

## Revision log

| Date | Change |
|---|---|
| 2026-07-22 | Front-town empire redesign approved (see superseded spec) |
| 2026-07-25 | Master doc created. Stand-with-requests becomes the primary economy · homestead split into shell + ongoing site · **delivery runs cut** · danger relocated to the front as a social system · "what is night for?" flagged as the blocking open question |
| 2026-07-25 | **Thread #1 settled — the guardrail contradiction.** Answer: daytime play costs the player nothing. **Bait notes cut** and parked in `LaterIdeas.md`; they were the design's only source of daylight loss · guardrail 1 stands with explicit scope (no cash, goods, progress, or standing) · Constable beats now cost **opportunity only** · new weakness ⑤, the primary economy has no edge · thread #3 reblocked from #1 onto #4, and **#4 "what is night for?" becomes the front of the queue and the design's only remaining home for tension** |
| 2026-07-25 | **Stand designed in full** (Part 3). Roadside at the homestead, town storefront as a mid-game channel unlock · stand needs no tending; requests arrive as written notes in a book · the book is a correspondence, with signed notes and replies · customer mix shifts strangers → named residents and *is* the progress meter · requests are exact with descriptive spikes that point at unlocks · suspicious notes are the one tension mechanic, recovering the Constable's staging ground · conflicting requests and quality-reputation penalties considered and cut |
| 2026-07-26 | **Audit closed out.** `BuildPlan.md` reconciled against this document — header repointed here, slice summary rewritten, Phase 4 tombstoned, Phases 3 and 5 halved, **Phase S** (stand + request book) and **Phase N** (night beats + Constable) added, four broken validation metrics replaced, Berta's trigger and the Boarding House role marked open, the Phase 5/6 reputation contradiction fixed, Phase 1 taught shell-vs-site, guardrails updated to seven · the guard system **deleted** in full: `Guard.cs`, `GuardManager`, `BribeUI`, `Guard.prefab`, the three bribe events, and 37 scene objects |
| 2026-07-25 | **The cozy decision — the largest revision since the runs cut.** The inherited assumption that the game needs a mechanical edge *somewhere* is **rejected**. Lamplight is a restoration game with a criminal skin; jeopardy is not relocated, it is removed · guardrail 1 widens from "day life" to the whole game · **new guardrail 7: cozy is the genre, not a fallback** · weakness ⑤ closed by decision · the Constable is reframed from a tension system to a recurring character who costs the player nothing (thread #3 unblocked and radically smaller) · `Guard.cs` / `GuardManager` resolve to **delete** · bait notes move from parked to out-of-genre · the moral axis loses its "closed doors" resolution and may not be worth building · **thread #5 (side activities) promoted to core**, since variety is now the only retention mechanism · the game's pull rests entirely on hooks 5 and 6, both unproven |
| 2026-07-25 | **Thread #4 settled — what night is for.** Answer: **night is a scene, not an activity block.** Most nights are empty and the player just sleeps · beats wait at the homestead, making them unmissable by construction without any scheduling system · a beat is warmth, story, or the cellar mystery, and never changes the player's inventory · **the pillar *"day = the front, night = the operation"* is retired** and replaced with *day is when you act; night is when the day answers back* · the day/night cycle survives as pacing and mood, and the lighting system survives on its own merits · **night's real duration recorded for the first time** — ~3.8 real minutes from dusk, ~2.3 genuinely dark, ending in a forced midnight sleep — which required no clock retuning and independently ruled out the covert-activity candidate |
