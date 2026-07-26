# LAMPLIGHT — Build Plan v2 (Front-Town Empire slice)

Design: Assets/Docs/GameDesign.md — the master design document. Read it before any phase.
Superseded: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md (kept as the dated record of what was approved on 2026-07-22; where the two disagree, GameDesign.md wins).
Stack: Unity 2D (URP), C#, Tilemap, Light2D. Evenings/weekends; phases are scoped, not dated.

Phase numbers are identifiers, not a sequence — Phase D came first and Phase 4 is retired. Build order is the order the sections appear below.

## Slice contents
- Fantasy: moonshiner rebuilds a dying town as the perfect front. **You light the town, and the town covers for you.** Day is when you act; night is when the day answers back.
- One connected exterior map: street (existing 60×20) → near forest (camp, foraging). **Deep woods are not scheduled** — the runs cut removed their only justification and nothing has replaced it (GameDesign.md Part 4, "Smaller open items"). Interiors: Roadhouse + homestead only; rest facade-only.
- Systems: movement/interaction · building states · staged construction · production (mash → ferment → bottle) · **roadside stand + request book (the primary economy)** · day-night + sleep-save · night beats · conspiracy trust · recruitment beats · public infrastructure · JSON save.
- 8 NPCs: Tormod, Berta, Signe, Aksel, Ingrid, Elias, Mrs. Holt, Constable Aas (antagonist, not recruitable).
- Cliffhanger: Mill cellar, locked from the inside. Metric: do they ask what's in the cellar?
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene framework, combat, minimap, co-op, free placement (sockets only), corrupt-deputy arc.
- CUT, not deferred (2026-07-25): delivery runs, routes, patrols, detection, load-outs, covert forest infrastructure, bait notes. See GameDesign.md Part 4, "The runs decision" and "The cozy decision".

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
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Tormod keeps dusk-to-dawn hours (18:00–06:00) via `SellerRules.IsPresent`; he is the Act 0 buyer, not a permanent shopfront.
- [x] Homestead **shell** build-from-scratch: 3 stages (Foundation 3 Stone → Frame 3 Wood → Walls 2 Wood + 3 Nails from Tormod) on the player's own camp clearing; player forages materials between ferment batches. **The shell closes Act 0 — it is not the finished homestead.** Everything after (stand, second vat, storage, interior rooms, eventually a cellar) is ongoing site growth, beginning with Phase S. See GameDesign.md Part 3, "The homestead is a site, not a purchase."
- [x] Tent persists after move: becomes first stash point.
- [x] Done: new player reaches the homestead shell in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.

## Phase 2 — Art: replace all placeholders
- [ ] FallenLog: pick or draw a log sprite from Grasslands tileset.
- [ ] BuildSign stages: signpost (Site), stone foundation outline (Foundation), wood frame (Frame), building exterior (Walls) — scale sprites to match 7×8 final building.
- [ ] Homestead facade: verify Town tileset sprites render correctly on build completion; remove Square overlay for good.
- [ ] Crate: replace `Texture2D.whiteTexture` with real crate sprite.
- [ ] Debris: replace `Texture2D.whiteTexture` with rubble sprite.
- [x] SellerInteractable (Tormod): NPC or stall sprite.
- [x] Constable Aas sprite (originally drawn as the Guard): directional idle/walk animations. The script is deleted; the art is kept and reused.
- [x] Player: directional idle/walk animations using DirectionalSpriteAnimator.
- [ ] Building facades (Bakery, General Store, Road House, Mill, Boarding House, Constable): each gets its own facade sprite instead of tinted Square overlay.
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
- [x] Most requests exact (product, quantity, date); a minority descriptive — *"something strong, it's for a wedding"* — mapping to several valid answers.
- [ ] Descriptive requests may ask for what the player cannot make yet, pointing at the next unlock (a request for something aged, before barrels exist).
- [x] **Requests never expire.** A note stays until filled or declined; new notes arrive only into free slots, so an ignored request costs the slot and nothing else. Declining is free.
- [ ] Payment *and a reply* on the next visit. Notes are signed; voice arrives through handwriting and phrasing.
- [ ] Customer mix shifts strangers → mixed → named residents. **This is the progress meter.** Never announced.
- [ ] Capacity: simultaneously active requests grow through stand upgrades, then the town storefront (mid-game channel unlock).
- [ ] Tormod retires as a channel once the stand opens. The capped Roadhouse account is **cut** — the shelf already is the zero-effort floor.
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
- [ ] Quest: "A Deal's a Deal" — sell 1 batch of Berry Shine to Tormod. Reward: +3 Nails from Tormod.
- [ ] Quest: "A Roof Over Your Head" — build the Homestead shell to Walls stage. Reward: none (tutorial quest, completion = progress).
- [ ] Per-NPC conspiracy trust gates function tiers AND dialogue (Signe t1 discounts, t2 sales buff).
- [ ] Recruitment beats on move-in coroutine tech: Tormod (Act 0), Berta, Signe, Aksel, Ingrid, Elias.
- [ ] **Berta's trigger needs a non-jeopardy replacement.** "Catches you, covers unprompted" was built on smuggling and there is nothing left to catch. Open — see GameDesign.md thread #8. Do not invent it here.
- [ ] Global reputation DIES here: remove rep meter/HUD/recipe gates; replace gates with trust/flags.
- [ ] 5 fragments = the old operation's story; sources: clearing debris, recruit gifts, milestones.
- [ ] Done: full Bakery arc — restore → Berta beat → bread-cart cover unlocked → her window lights.

## Phase N — Night beats and the Constable
Design source: GameDesign.md Part 4, threads #4 and #3. Depends on Phase 6 for narrative tech. **Mostly a writing job.**
- [ ] Beats wait at the homestead, where sleep already happens: someone sitting at your fire, a note weighted under a stone, a lamp lit in a window that was dark yesterday. **Unmissable by construction** — no telegraphing, no appointment, no scheduling system.
- [ ] A beat leaves the player knowing something or feeling something. **It never changes their inventory.**
- [ ] Beat content: a recruit with nowhere else to go · a fragment of the old operation's story · a thank-you for something done days ago and forgotten · the Constable, exactly once and memorably, simply standing in the road.
- [ ] Constable appearances: daylight, polite, patient, unhurried, always slightly too interested. He never takes cash, goods, progress, standing, or opportunity.
- [ ] Most nights are empty and the player simply goes to bed. That is correct, not a shortfall.
- [ ] **Open numbers, deferred to design:** beat frequency · how beats are authored and triggered (milestone, day count, or hand-placed) · whether empty nights get a small ambient reward · whether the 21:00 sleep floor (`Bed.cs:9`) should move earlier now that night has content worth encountering.
- [ ] Done: a tester, asked what they remember, describes a night beat unprompted.

## Phase 7 — Content build-out
- [ ] Buildings ×7 (front / function / track): Roadhouse (first buyer) · Bakery (yeast, bread-cart cover) · General Store (supply, sales buff) · Smithy & Cooperage (still upgrades, second vat, barrels) · Apothecary (botanicals, recipes) · Boarding House (recruits, rent — **operation role needs redesign**, it previously housed the courier) · Old Mill (bulk grain, cellar, endgame — Holt-gated).
- [ ] Constable's office: never purchasable. Light always on.
- [ ] Quality ladder: berry shine → corn/grain → aged (barrels) → flavored (botanicals).
- [ ] Mill stage 1 complete → cellar door → locked-from-inside line → title card.
- [ ] Numbers pass: homestead shell 20–40 min · first stand sale · Mill cliffhanger 4–6 h.
- [ ] Done: stranger plays start → cliffhanger, zero instructions.

## Phase 8 — Audio
- [ ] Audio ≈ 20 SFX + 2 loops. Priorities: deed stamp (THICK) · lamp-lighting sting (commission if anything) · a night-beat cue — the sound that says something is waiting at the homestead.

## Phase 9 — Playtest + tune
- [ ] 3 testers, recorded, you silent.
- [ ] Collect: time to homestead shell (20–40 min) · time to first stand sale · do they notice the request book's customer mix shifting? · stuck >60 s anywhere · unprompted reaction at first lamppost lighting · do they ask what's in the cellar?
- [ ] Cut pass: confused 2 of 3 → fix or cut; noticed by nobody → cut. No additions in final week.

## Rules
- Still minigame stays deferred — revisit only if a fun design emerges.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 2 (was Phase 7, moved up). Juice allowed early.
- Save versioning + tolerant deserializer from day one.
- Design guardrails (from GameDesign.md Part 3, non-negotiable): never punish the player for playing — no loss anywhere, at any hour · legibility over drama, no hidden dice · Act 0 is a prologue, 20–40 min · appointments create "one more day", never FOMO · beautification is never punished · restoration doubles as defense *in the fiction* · **cozy is the genre, not a fallback**.
- Every mid-build idea → the LATER note, unexamined. Review once, at playtest.
