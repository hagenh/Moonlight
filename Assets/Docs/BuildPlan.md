# LAMPLIGHT — Build Plan v2 (Front-Town Empire slice)

Spec: docs/superpowers/specs/2026-07-22-lamplight-redesign-design.md — read it before any phase.
Stack: Unity 2D (URP), C#, Tilemap, Light2D. Evenings/weekends; phases are scoped, not dated.

## Slice contents
- Fantasy: moonshiner rebuilds a dying town as the perfect front. Day = cozy front life; night = opt-in delivery runs. You light the town and keep the woods dark.
- One connected exterior map, three depths: street (existing 60×20) → near forest (camp, foraging) → deep woods (routes, destinations). Interiors: Roadhouse + homestead only; rest facade-only.
- Systems: movement/interaction · building states · staged construction · production (mash → ferment → bottle) · stand (safe channel) · delivery runs (routes, patrols, load-outs) · day-night + sleep-save · conspiracy trust · recruitment beats · two-layer infrastructure · JSON save.
- 8 NPCs: Tormod, Berta, Signe, Aksel, Ingrid, Elias, Mrs. Holt, Constable Aas (antagonist, not recruitable).
- Cliffhanger: Mill cellar, locked from the inside. Metric: do they ask what's in the cellar?
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene framework, combat, minimap, co-op, free placement (sockets only), corrupt-deputy arc.

## Phase D — Demolition (done)
- [x] Delete heat/suspicion: meter, tiers, guard-count scaling, sleep raids, heat decay, suspicion pricing, risky buyer.
- [x] Bribe rework: caught while carrying → pay to keep cargo, refuse to lose it. No heat aftermath.
- [x] Guards: single fixed patrol until Phase 3 repurposes them onto routes.
- [x] Keep: reputation (dies in Phase 5 with conspiracy trust), stand plan, staged construction, sleep pipeline (minus punishment).
- [x] Done: compiles, all tests green, no reference to Heat anywhere in Assets/Scripts.
- Playtest fixes: instant guard detection (no gradual ramp) · CarryState freezes on menu open instead of dropping crate · Building interact passes through without dropping crate · moveInput zeroed on menu open to prevent ghost movement

## Phase 1 — Act 0: the tent prologue (in progress)
- [x] Camp clearing in near forest: tent, campfire pot (1-slot wild ferment, slow, yields 2 jars).
- [x] Foraging: berry bushes (respawn daily), 8–10 scattered across camp, the road to town, town outskirts, and hidden corners — forage verb = existing interact.
- [x] Berry shine recipe (wild yeast — no yeast ingredient, 3h ferment, always discovered).
- [x] Day 1 starting inventory: 3 Berry so the player can start fermenting immediately instead of waiting idle.
- [x] Recipe discovery scaffolding: `RecipeDiscovered` event on GameEvents, hidden/discovered recipe tracking in FermentManager; Berry Shine is exempt and always visible.
- [x] Roadhouse back door: dusk-only delivery point, Tormod buys, names price (his recruitment beat = tutorial).
- [x] Homestead build/restore: on the player's own camp clearing (not a separate town-edge lot) — smash/clear/repair doubles as cleaning up the player's own camp; price reachable in ~3 sales; unlocks proper still + vat + game proper.
- [x] Tent persists after move: becomes first stash point.
- [ ] Done: new player reaches homestead in 20–40 min without instructions. HARD gate: still in tent on day 4 = numbers wrong, fix before proceeding.

## Phase 2 — World: one map, three depths
- [ ] Extend tilemap: near forest (camp + foraging) and deep woods with 3 route corridors and 3 destination sites (logging camp, river dock, crossroads).
- [ ] No exterior scene loads; interiors stay separate (existing InteriorManager).
- [ ] Walk timings: town end-to-end 12–15 s (existing), camp ~20 s from town, destinations 60–90 s.
- [ ] Darkness pass: night in the woods is genuinely dark (Light2D); the lit town visible from the treeline — screenshot this.
- [ ] Done: walk street → camp → each destination and back, day and night.

## Phase 3 — Delivery runs
- [ ] Destinations buy at run prices: stand (safe, low) < Roadhouse (safe, capped/day, medium) < runs (high).
- [ ] Appointments: logging camp payday Fridays (2× demand) · river dock barge nights · crossroads wagon on set nights/hours. All recur; nothing permanently missable.
- [ ] Routes: main road (fast, night checkpoints) · forest trail (slow, dark, sparse) · creek path (locked until shortcut plank).
- [ ] Patrols: existing Guard vision cones on fixed waypoint schedules per route/hour. NO random spawns, ever. Detection only while carrying cargo. Patrols only on routes at night — town and near forest never patrolled.
- [ ] Caught: cargo confiscated; bribe keeps it (cost scales with load). Nothing else. Ever.
- [ ] Load-outs: satchel 2 jars (off-path capable) → handcart 8 jars (path-bound, wider profile, built by Aksel in Phase 6) → courier automation (5 clean runs on a route + Boarding House recruit → auto-resolve for a cut).
- [ ] Done: full loop — brew by day, run by night, near-miss stories happen unscripted.

## Phase 4 — Two-layer infrastructure
- [ ] Public sockets (street): lamppost, plank sidewalk, bench, flower box, sign. Effects: night light, small stand buff at beauty thresholds, dialogue reactions. Never any downside.
- [ ] Covert sockets (forest): stash barrel (ditch/retrieve cargo mid-run) · trail marker (faint night glint) · shortcut plank (unlocks creek path). Lookout perch = stretch, cut first.
- [ ] Done: a player who beautifies the street AND builds the smuggler's toolkit feels both are "mine."

## Phase 5 — Narrative: conspiracy trust + recruitment
- [ ] NarrativeFlags + MilestoneDetector + conditional DialogueResolver per Assets/Docs/NarrativeDesign.md architecture (still valid — reskin meanings only).
- [ ] Per-NPC conspiracy trust gates function tiers AND dialogue (Signe t1 discounts, t2 sales buff).
- [ ] Recruitment beats on move-in coroutine tech: Tormod (Act 0), Berta (catches you, covers unprompted), Signe, Aksel, Ingrid, Elias.
- [ ] Global reputation DIES here: remove rep meter/HUD/recipe gates; replace gates with trust/flags.
- [ ] 5 fragments = the old operation's story; sources: clearing debris, recruit gifts, milestones.
- [ ] Done: full Bakery arc — restore → Berta beat → bread-cart cover unlocked → her window lights.

## Phase 6 — Content build-out
- [ ] Buildings ×7 (front / function / track): Roadhouse (first buyer) · Bakery (yeast, bread-cart cover) · General Store (supply, sales buff) · Smithy & Cooperage (still upgrades, handcart, barrels) · Apothecary (botanicals, recipes) · Boarding House (recruits, rent) · Old Mill (bulk grain, cellar, endgame — Holt-gated).
- [ ] Constable's office: never purchasable. Light always on.
- [ ] Quality ladder: berry shine → corn/grain → aged (barrels) → flavored (botanicals).
- [ ] Mill stage 1 complete → cellar door → locked-from-inside line → title card.
- [ ] Numbers pass: homestead 20–40 min · first night run ~1 h · Mill cliffhanger 4–6 h.
- [ ] Done: stranger plays start → cliffhanger, zero instructions.

## Phase 7 — Art + audio
- [ ] One 16×16 tileset family incl. forest. Facades: 7 restored bases + shared overlay kit.
- [ ] Portraits > walk cycles; 2 emotion variants for Holt/Aas/Berta.
- [ ] Light pass: warm windows (flicker), player-placed lampposts, dark woods, lantern cone on runs.
- [ ] Audio ≈ 20 SFX + 2 loops. Priorities: deed stamp (THICK) · lamp-lighting sting (commission if anything) · night-run ambience layer.

## Phase 8 — Playtest + tune
- [ ] 3 testers, recorded, you silent.
- [ ] Collect: time to homestead (20–40 min) · time to first night run (<75 min) · caught-players can name their mistake (legibility check — if not, patrol/telegraphing bug) · stuck >60 s anywhere · unprompted reaction at first lamppost lighting · do they ask what's in the cellar?
- [ ] Cut pass: confused 2 of 3 → fix or cut; noticed by nobody → cut. No additions in final week.

## Rules
- Still minigame stays deferred — revisit only if a fun design emerges.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 7. Juice allowed early.
- Save versioning + tolerant deserializer from day one.
- Design guardrails (from spec, non-negotiable): never punish daytime play · no hidden dice against the player · Act 0 is a prologue · appointments recur · beautification is never punished.
- Every mid-build idea → the LATER note, unexamined. Review once, at playtest.
