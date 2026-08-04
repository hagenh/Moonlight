# LAMPLIGHT — Build Plan v2 (Front-Town Empire slice)

Design: Assets/Docs/GameDesign.md — the master design document. Read it before any phase.
Superseded: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md (kept as the dated record of what was approved on 2026-07-22; where the two disagree, GameDesign.md wins).
Stack: Unity 2D (URP), C#, Tilemap, Light2D. Evenings/weekends; phases are scoped, not dated.

Phase numbers are identifiers, not a sequence — Phase D came first and Phase 4 is retired. Build order is the order the sections appear below.

## Slice contents
- Fantasy: moonshiner rebuilds a dying town as the perfect front. **You light the town, and the town covers for you.** Day is when you act; night is when the day answers back.
- One connected exterior map: street (existing 60×20) → near forest (camp, foraging). **Deep woods are not scheduled** — the runs cut removed their only justification and nothing has replaced it (GameDesign.md Part 4, "Smaller open items"). Interiors: Roadhouse + homestead only; rest facade-only.
- Systems: movement/interaction · building states · staged construction · production (mash → ferment → bottle) · **roadside stand + request book (the primary economy)** · day-night + sleep-save · night beats · conspiracy trust · recruitment beats · public infrastructure · JSON save (built in Phase 9 — see Rules).
- 8 NPCs: Tormod, Berta, Signe, Aksel, Ingrid, Elias, Mrs. Holt, the miller (placeholder name Runa, vetoable). **Constable Aas cut 2026-08-04** — see LaterIdeas.md for the after-demo revisit item.
- Cliffhanger: Mill cellar, locked from the inside. Metric: do they ask what's in the cellar?
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene framework, combat, minimap, co-op, free placement (sockets only), corrupt-deputy arc.
- CUT, not deferred (2026-07-25): delivery runs, routes, patrols, detection, load-outs, covert forest infrastructure, bait notes. See GameDesign.md Part 4, "The runs decision" and "The cozy decision". **Also cut (2026-08-04):** the police — Constable Aas and all police content. See GameDesign.md, "The Constable."

## Phase D — Demolition (done)
- [x] Delete heat/suspicion: meter, tiers, guard-count scaling, sleep raids, heat decay, suspicion pricing, risky buyer.
- [x] ~~Bribe rework: caught while carrying → pay to keep cargo, refuse to lose it.~~ **Superseded 2026-07-25** — nothing catches the player any more. `BribeUI` and the three bribe events are deleted.
- [x] ~~Guards: single fixed patrol until Phase 3 repurposes them onto routes.~~ **Superseded 2026-07-25** — Phase 3 never repurposes them. `Guard.cs`, `GuardManager`, `BribeUI` and their scene objects are **deleted**. The Guard *sprite* survives as Constable Aas.
- [x] Keep: reputation (dies in Phase 6 with conspiracy trust), stand plan, staged construction, sleep pipeline (minus punishment).
- [x] Done: compiles, all tests green, no reference to Heat anywhere in Assets/Scripts.
- Playtest fixes (historical — the guard items no longer apply): instant guard detection (no gradual ramp) · CarryState freezes on menu open instead of dropping crate · Building interact passes through without dropping crate · moveInput zeroed on menu open to prevent ghost movement

## Phase 1 — Act 0: the tent prologue (in progress)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), 8–10 scattered across camp, the road to town, town outskirts, and hidden corners — forage verb = existing interact.
- [x] Foraging: stone piles and fallen logs (respawn daily) yield Stone and Wood for Homestead construction.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, 3h ferment, always discovered).
- [x] Day 1 starting inventory: 3 Berry so the player can start fermenting immediately instead of waiting idle.
- [x] Recipe discovery scaffolding: `RecipeDiscovered` event on GameEvents, hidden/discovered recipe tracking in FermentManager; Berry Shine is exempt and always visible.
- [x] ~~Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).~~ **Superseded 2026-08-03** — Tormod is never a buyer. The back-door delivery point is deleted in Phase S; the stand sells, and the nails move to his first conversation.
- [x] ~~Tormod keeps dusk-to-dawn hours (18:00–06:00) via `SellerRules.IsPresent`; he is the Act 0 buyer, not a permanent shopfront.~~ **Superseded 2026-08-03** — deleted with the channel.
- [x] Homestead **shell** build-from-scratch: 3 stages (Foundation 3 Stone → Frame 3 Wood → Walls 2 Wood + 3 Nails from Tormod) on the player's own camp clearing; player forages materials between ferment batches. **The shell closes Act 0 — it is not the finished homestead.** Everything after (stand, second vat, storage, interior rooms, eventually a cellar) is ongoing site growth, beginning with Phase S. See GameDesign.md Part 3, "The homestead is a site, not a purchase."
- [x] Tent persists after move: becomes first stash point.
- [x] Done: new player reaches the homestead shell in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.

## Phase 2 — Art: replace all placeholders
- [ ] FallenLog: pick or draw a log sprite from Grasslands tileset.
- [ ] BuildSign stages: **Site done 2026-07-28** — `BuildingPreBuildingStage.prefab`, a staked-out fence-outline construction site, replaces the tan placeholder for `BuildStage.Site`. Foundation, Frame, and Walls still use the tint-and-scale placeholder.
- [x] Town building Abandoned state (all 6, via the shared `Building.prefab`): same `BuildingPreBuildingStage.prefab` fence outline now hides the real facade until purchased — done 2026-07-28. Purchased/Cleared/Restored unchanged.
- [ ] Homestead facade: verify Town tileset sprites render correctly on build completion; remove Square overlay for good.
- [ ] Crate: replace `Texture2D.whiteTexture` with real crate sprite.
- [ ] Debris: replace `Texture2D.whiteTexture` with rubble sprite.
- [x] SellerInteractable (Tormod): NPC or stall sprite.
- [x] Constable Aas sprite (originally drawn as the Guard): directional idle/walk animations. The script is deleted; the art is kept and reused.
- [x] Player: directional idle/walk animations using DirectionalSpriteAnimator.
- [ ] Building facades (Bakery, General Store, Road House, Mill, Boarding House): each gets its own facade sprite instead of tinted Square overlay.
- [ ] Bed, DebrisPile, DeliveryPoint, ExitDoor: verify real sprites assigned in scene.
- [ ] Fix `Texture2D.whiteTexture` bugs in Crate.Create() and Debris.Create() — use `new Texture2D(16,16)`.
- [ ] Done: no placeholder colored squares or 4×4 white textures remain. All sprites are real tileset art.

## Phase 3 — World: one map, two depths
- [ ] Extend tilemap: near forest (camp + foraging). Route corridors and the three destination sites (logging camp, river dock, crossroads) are **cut with the runs**.
- [ ] No exterior scene loads; interiors stay separate (existing InteriorManager).
- [ ] Walk timings: town end-to-end 12–15 s (existing), camp ~20 s from town.
- [ ] Darkness pass, **rescoped**: night is the homestead at night, and the lit town seen from the treeline — not dark woods to sneak through. Serves the night-beat design, not the runs. Screenshot this.
- [ ] Done: walk street → camp and back, day and night.

## Phase 4 — Delivery runs (CUT 2026-07-25)
- Cut entirely. See GameDesign.md Part 4, "The runs decision". Phase number retired, not reused.

## Phase S — The roadside stand and the request book
Design source: GameDesign.md Part 3, "The stand and the request book". **The primary economy and the single most important system in the slice.**
- [x] Stand built on the homestead site, roadside — ongoing site growth, not a one-time purchase.
- [x] Shelf trade is passive: stock it, wander off, come back to coins. The income floor; never needs tending.
- [x] Request book by the stand: orders arrive as written notes. No customer queue, no summons, nothing expires while the player is across the map.
- [ ] **Playtest question, not settled work (2026-08-03): the stand's shape beyond the book.** The book is primary and wins over customers in any form — that part is decided. To test, not to design: book from day 1 (current build) vs. unlocked at the shell · whether the passive shelf earns its place or book-only is cleaner. In-person browsing is cut by default. Do not build gating or cut the shelf ahead of playtest.
- [x] Most requests exact (product, quantity, date); a minority descriptive — *"something strong, it's for a wedding"* — mapping to several valid answers.
- [ ] Descriptive requests may ask for what the player cannot make yet, pointing at the next unlock (a request for something aged, before barrels exist).
- [x] **Requests never expire.** A note stays until filled or declined; new notes arrive only into free slots, so an ignored request costs the slot and nothing else. Declining is free.
- [ ] Payment *and a reply* on the next visit. Notes are signed; voice arrives through handwriting and phrasing.
- [ ] Customer mix shifts strangers → mixed → named residents. **This is the progress meter.** Never announced.
- [ ] Capacity: simultaneously active requests grow through stand upgrades, then the town storefront (mid-game channel unlock).
- [ ] **Tormod is never a channel (settled 2026-08-03; supersedes "retires once the stand opens").** The player never sells to him — the stand is the selling channel from the start. Work: delete the Roadhouse back-door delivery point and the `SellManager`/`SellerRules` Tormod flow (and `SellerRulesTests`); grant the 3 Nails on the player's **first conversation** with him instead of first delivery. He stays in the game as the first person the player talks to — a character and a tutorial pointer, never a price. The capped Roadhouse account stays cut.
- [ ] Appointments relocate here as demand events (market days, festivals, a buyer visiting town). All recur; nothing permanently missable.
- [ ] Tension is **triage only** — limited ingredients and time. No conflicting requests, no quality-reputation penalties, no bait notes.
- [x] **Numbers settled 2026-07-26** (GameDesign.md Part 3, "The numbers"): 2 notes per night rising to 3 · requests sized 1-3 batches · 3 active slots → 5 → 8 · shelf 1.0×, exact request 1.8×, descriptive 2.2× · no expiry.
- [ ] Done: the player's first question each morning is *what does the book want today?* — brewing is chosen against demand rather than repeated.

**Phase S is partly done, not done.** Built 2026-07-26: the request domain (`Rules/StandRequest`, `RequestBook`, `RequestBookRules`, `RequestArrivalRules`), `StandManager` bridging clock and inventory, the `Stand` interactable and the IMGUI `RequestBookUI`. All three are placed in SampleScene — `StandManager` at root, `RequestBookUI` on `HUDCanvas` beside the other panels, `Stand` on the camp clearing at `(-17.25, 8)`. Still open, each its own later plan: replies and the correspondence voice (notes carry placeholder signatures only), the strangers → residents customer mix, descriptive requests pointing past what the player can brew, slot upgrades beyond the starting 3 (`RequestBook.SetSlotCount` exists but nothing grants it), the town storefront, retiring Tormod as a channel, and appointments as demand events. `RequestArrivalRules.DescriptiveInN = 4` is an **invented** tuning number — GameDesign.md says only "a minority are descriptive" and does not fix the fraction.

## Phase 5 — Public infrastructure
- [ ] Public sockets (street): lamppost, plank sidewalk, bench, flower box, sign. Effects: night light, small stand buff at beauty thresholds, dialogue reactions. Never any downside.
- [ ] Covert forest sockets (stash barrel, trail marker, shortcut plank, lookout perch) are **cut** — they existed to serve delivery runs.
- [ ] Done: a player who beautifies the street feels it is "mine", and sees the stand busier for it.

## Phase 6 — Narrative: conspiracy trust + recruitment
- [ ] NarrativeFlags + MilestoneDetector + conditional DialogueResolver per Assets/Docs/NarrativeDesign.md architecture (still valid — reskin meanings only).
- [ ] Quest system: QuestDef (id, description, trigger event, condition, reward) + QuestTracker that listens to GameEvents, checks conditions, grants rewards. No quest log UI yet — just toast on completion.
- [ ] Quest: "First Batch" — ferment 1 batch of Berry Shine. Reward: none (tutorial quest, completion = progress).
- [ ] Quest: "A Deal's a Deal" — talk to Tormod for the first time. Reward: +3 Nails, his gift — the walls need them. *(Rewritten 2026-08-03: selling to Tormod is cut.)*
- [ ] Quest: "A Roof Over Your Head" — build the Homestead shell to Walls stage. Reward: none (tutorial quest, completion = progress).
- [ ] Per-NPC conspiracy trust gates function tiers AND dialogue (Signe t1 discounts, t2 sales buff).
- [ ] Recruitment beats on move-in coroutine tech: Tormod (Act 0), Berta, Signe, Aksel, Ingrid, Elias. **Berta, Aksel, and Ingrid's move-in destinations retarget from their operated building to their player-built house (2026-08-04 — tech survives, destination changes; see Phase H).** The miller (placeholder Runa) is a new recruitment beat, arriving via the Boarding House.
- [ ] **Berta's trigger needs a non-jeopardy replacement.** "Catches you, covers unprompted" was built on smuggling and there is nothing left to catch. Open — see GameDesign.md thread #8. Do not invent it here.
- [ ] Global reputation DIES here: remove rep meter/HUD/recipe gates; replace gates with trust/flags.
- [ ] 5 recipe book pages = the old operation's story; sources: milestones (building restorations, the Mill cellar), Mrs. Holt's trust threshold. Replaces the cut fragment system — see `docs/superpowers/specs/2026-08-04-recipe-book-narrative-redesign-design.md`.
- [ ] Done: full Bakery arc — restore → hand-produce yeast → Berta beat → build her house → she moves in → hire her → automated yeast + her house window lights.

## Phase N — Night beats
Design source: GameDesign.md Part 4, thread #4. Depends on Phase 6 for narrative tech. **Mostly a writing job.** Thread #3 (the Constable) is cut, 2026-08-04 — no Constable content in this phase; see LaterIdeas.md for the after-demo revisit item.
- [ ] Beats wait at the homestead, where sleep already happens: someone sitting at your fire, a note weighted under a stone, a lamp lit in a window that was dark yesterday. **Unmissable by construction** — no telegraphing, no appointment, no scheduling system.
- [ ] A beat leaves the player knowing something or feeling something. **It never changes their inventory.**
- [ ] Beat content: a recruit with nowhere else to go · another page of the grandfather's recipe book becomes legible · a thank-you for something done days ago and forgotten.
- [ ] Most nights are empty and the player simply goes to bed. That is correct, not a shortfall.
- [ ] **Open numbers, deferred to design:** beat frequency · how beats are authored and triggered (milestone, day count, or hand-placed) · whether empty nights get a small ambient reward · whether the 21:00 sleep floor (`Bed.cs:9`) should move earlier now that night has content worth encountering.
- [ ] Done: a tester, asked what they remember, describes a night beat unprompted.

## Phase 7 — Content build-out
- [ ] Buildings ×7 (front / function / track): Roadhouse (first buyer) · Bakery (yeast, bread-cart cover) · General Store (supply, sales buff) · Smithy & Cooperage (still upgrades, second vat, barrels) · Apothecary (botanicals, recipes) · Boarding House (Elias's recruitment housing — **operation role settled 2026-08-04**, see Phase H) · Old Mill (bulk grain, cellar, endgame — Holt-gated).
- [ ] ~~Constable's office: never purchasable. Light always on.~~ **Cut 2026-08-04** — see GameDesign.md, "The Constable."
- [ ] Quality ladder: berry shine → corn/grain → aged (barrels) → flavored (botanicals).
- [ ] Mill stage 1 complete → cellar door → locked-from-inside line → title card.
- [ ] Numbers pass: homestead shell 20–40 min · first stand sale · Mill cliffhanger 4–6 h.
- [ ] Done: stranger plays start → cliffhanger, zero instructions.

## Phase F — Factories
Design source: GameDesign.md Part 3, Buildings — "the factory model" (`docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md`). Depends on Phase 7 (the four factory buildings restored).
- [ ] Restoring a factory (Bakery, Smithy & Cooperage, Apothecary, Old Mill) immediately unlocks hand-production of its goods — no resource ever gated behind a person.
- [ ] Bakery hand-production: yeast (faster ferments).
- [ ] Smithy & Cooperage hand-production: nails, barrels, still parts (answers the parked Nail Economy item, LaterIdeas.md).
- [ ] Apothecary hand-production: botanical extracts → flavored recipes.
- [ ] Old Mill hand-production: bulk grain.
- [ ] **Open playtest question, deliberately unsettled (2026-08-04):** is hand-production a menu, or a small physical activity at the factory? Do not build gating either way ahead of playtest.
- [ ] Done: all four factories produce their goods by hand once restored, with no operator hired.

## Phase H — Houses, hiring, and automation
Design source: GameDesign.md Part 3, Buildings — "the people ladder" (`docs/superpowers/specs/2026-08-04-npc-roster-and-factory-model-design.md`). Depends on Phase F (factories producing) and Phase 6 (conspiracy trust, for hiring trust thresholds).
- [ ] Four new player-built house lots: Berta's, Aksel's, Ingrid's, the miller's — built on the homestead build-from-scratch tech (staged builds from gathered materials), same as the homestead shell.
- [ ] The house is the hiring gate: no house, no hire.
- [ ] Hiring is named-NPCs-only (2026-08-03) — reaffirmed, no random-hire path.
- [ ] Wages: hiring an operator switches their factory to automated production, minus a kept share of output (reduced gain) — the player never loses anything held. Share is a playtest number.
- [ ] The miller (placeholder name Runa, vetoable): arrives via the Boarding House, builds a house, works the player's Mill. The endgame hire.
- [ ] **Open playtest questions, deliberately unsettled (2026-08-04):** operator wage share %, production rates per factory, house build costs/stages, hiring trust thresholds.
- [ ] Done: hiring an operator (after their house is built) automates their factory — it produces without the player present — and the player's held goods never decrease as a result.

## Phase 8 — Audio
- [ ] Audio ≈ 20 SFX + 2 loops. Priorities: deed stamp (THICK) · lamp-lighting sting (commission if anything) · a night-beat cue — the sound that says something is waiting at the homestead.

## Phase 9 — Playtest + tune
- [ ] Build the save system before the first external tester: versioned JSON save/load to `Application.persistentDataPath`, tolerant deserializer, autosave on sleep. (Deferred here by decision 2026-08-03 — see Rules.)
- [ ] 3 testers, recorded, you silent.
- [ ] Collect: time to homestead shell (20–40 min) · time to first stand sale · do they notice the request book's customer mix shifting? · stuck >60 s anywhere · unprompted reaction at first lamppost lighting · do they ask what's in the cellar?
- [ ] Stand-shape questions (deliberately unsettled 2026-08-03, decided here): does the passive shelf earn its place next to the book, or is book-only cleaner? · does the book landing at day 1 leave Act 1 without a new hook (weakness ②)? · does the stand ever feel dead without people at it?
- [ ] Cut pass: confused 2 of 3 → fix or cut; noticed by nobody → cut. No additions in final week.

## Rules
- Still minigame stays deferred — revisit only if a fun design emerges.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 2 (was Phase 7, moved up). Juice allowed early.
- Save system deferred by decision (2026-08-03): none exists and none is needed until strangers play the build. It lands in Phase 9, before the first external tester. When built: versioned JSON + tolerant deserializer + autosave on sleep. Until then, every system should keep its state in plain serializable fields so the eventual save is an extraction job, not a rewrite.
- Design guardrails (from GameDesign.md Part 3, non-negotiable): never punish the player for playing — no loss anywhere, at any hour · legibility over drama, no hidden dice · Act 0 is a prologue, 20–40 min · appointments create "one more day", never FOMO · beautification is never punished · restoration doubles as defense *in the fiction* · **cozy is the genre, not a fallback**.
- Every mid-build idea → the LATER note, unexamined. Review once, at playtest.
