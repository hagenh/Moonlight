# LAMPLIGHT — Slice Build Plan (Unity, lean)

Stack: Unity 2D (URP), C#, Tilemap, Light2D. Target: 10–12 weeks, evenings/weekends.

## Slice contents
- 1 street exterior (~60×20 tiles), 3 interiors: Roadhouse, Bakery, Mill. Other 3 buildings renovate facade-only (same verbs, no rooms). **NEEDS REWRITE — revisit building types and which get interiors.**
- Systems: movement/interaction · building states · production (mash → ferment → sell) · selling ×3 channels · renovation verbs (smash/carry/hammer) · day-night + sleep-save · income tick · heat · reputation (3 dialogue tiers) · resident schedules (teleport/fade, no pathfinding) · milestone/unlock narrative system TBD · JSON save.
- 6 NPCs: Tormod, Berta, Elias, Aas, Signe, Mrs. Holt. ~20 lines each.
- 1 move-in beat (Berta done), 1 cliffhanger (Mill cellar) + title card. Narrative from characters, not pickups.
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene system, combat, minimap, settings beyond volume.

## Phase 0 — Skeleton (day 1)
- [x] URP 2D project, git.
- [ ] Defs as plain C# classes: `BuildingDef, ResidentDef, RecipeDef`. A `ContentDb` singleton holds lists; loaded by a bootstrap scene. *(ItemDef + ContentDb done; BuildingDef, ResidentDef deferred with their systems. FragmentDef removed — replaced by milestone/unlock system TBD)*
- [ ] Runtime/save as plain classes: `GameState { day, cash, heat, rep, buildings{}, residents{}, batches[], inventory{}, flags }` + `SaveVersion`. *(deferred: GameManager holds state for now, separate when saving needs it)*
- [ ] Install Newtonsoft JSON package (`com.unity.nuget.newtonsoft-json`) — JsonUtility can't do dictionaries. Save to `Application.persistentDataPath`. *(deferred: too early)*
- [x] Static `GameEvents`: `BuildingStateChanged, ResidentMovedIn, DayEnded, HeatChanged, RepChanged`. *(+ ToastRequested. FragmentFound removed — replaced by milestone/unlock system TBD)*
- [x] Rules: defs immutable, managers talk only via events + GameState. *(managers use events; defs deferred)*
- [x] Done: boots, defs load, log prints 6 buildings.

## Phase 1 — Street + building states (wk 1–2)
- [x] Player: Rigidbody2D (kinematic) + box collider, 8-dir, ~90 px/s @ 16px tiles. Street walk end-to-end ≈ 12–15 s. *(walk 4f, sprint 7f, FSM with idle/move/sprint/interact)*
- [x] Tilemap: ground / collision / overhead (sorting layers). Placeholder tiles only. *(ground + collision painted, overhead empty)*
- [x] `Building.prefab` (×6, data-driven by BuildingDef ref): · door trigger collider · Light2D per window (on at Restored) · dev label (name/state/price). *(6 instances placed, serialized fields instead of BuildingDef — defs deferred)*
- [x] Interaction: one key, nearest trigger wins. Route by state: buy → clear (stub: instant) → advance stage. *(+ only interacts when target exists; E/Escape close menus; IsMenuOpen freezes player; punch-scale animates visual child not root)*
- [x] `BuildingManager` mutates runtime, fires `BuildingStateChanged`; prefab listens and swaps its own sprite/lights.
- [x] Add purchase juice now: sound + scale tween (cheap, teaches the pipeline). *(squash-and-stretch coroutine, no sound yet)*
- [x] Done: walk, buy all 6 with debug cash, states cycle grey→green, window appears. Play 10 min. *(+ debug menu with P key)*

## Phase 2 — Production + money (wk 3–4)
- [ ] **Still minigame** *(DEFERRED to Phase 4 or later — not fun during testing, no compelling design yet)*
- [x] Ferment: batches tick with game hours. Vat interactable: empty → choose recipe (consume ingredients) → fermenting → ready → minigame. Second vat purchasable mid-slice. *(continuous time via TimeManager.TotalGameMinutes; FermentBatch computes progress from start time; FermentManager checks in Update; no still minigame handoff yet)*
- [ ] **Selling rework — Stall model** *(replacing menu-based selling)*
  - Player has a stall/counter at a fixed street position. Carry bottles/crates to stall → interact to place. Product is visible on the stall.
  - Customers line up as fake AI (2–3 sprites, spawn at fixed point, shuffle forward one slot, despawn at stall, on a timer). They buy automatically — bottles disappear from stall, cash ticks. Player can watch or walk away and do other things.
  - Traveling cart still sells ingredients (buying stays as-is or goes spatial — grab from cart). Cart does NOT buy bottles anymore; the stall handles all selling.
  - Tormod removed as a seller — replaced by the stall customer line.
  - Risky buyer: comes to the player at night. After the stall closes, a figure walks up to you on the street — one-line dialogue: double price offer. Accept (cash ticks, +heat) or refuse. No menu, no hiding from the offer. Personal, not transactional.
  - Automation: hire an NPC to staff the stall (not Signe — she runs the General Store). Staffed stall = no player stocking needed, income ticks automatically. This is the "hire someone to do the task you're tired of" beat.
  - *(Old SellManager/SellUI/SellerInteractable/SellMenuRequested events will need rework)*
- [x] HUD: cash, day, clock, heat, rep. *(all displays + inventory readout done)*
- [ ] Done: cart → mash → ferment → still → sell, no debug keys; 15 min of loop is near-fun.

## Phase 3 — Renovation + day cycle + save (wk 5–6)
- [x] Day: 1 real s ≈ 1.3 game min (~15 min days). Global Light2D color lerps noon→dusk→night. Window lights now pay off — screenshot the first lit window.
- [x] Sleep → tick, strict order: ferment → Elias repairs → income → heat decay → move-in checks → autosave. Comment the order; never reorder casually.
- [ ] **Renovation rework — Staged construction** *(replacing flat smash→carry→hammer)*
  - New pipeline: Buy → Clear (smash boards, carry debris) → Stockpile (carry materials from inventory/crate to building site, on-site material pile visible) → Build (construction stages, each with different materials and visual state).
  - Construction stages (per building, early buildings may skip some):
    - **Foundation** — needs Stone (new item, bought from cart). Building shows cleared ground + foundation lines.
    - **Frame** — needs Timber. Building shows skeleton/frame.
    - **Finish** — needs Nails + building-specific materials (glass for windows, paint, etc.). Building shows complete exterior.
  - Each stage: player carries materials to site → material pile appears/shrinks → hammer the stage → building visually transforms. Materials consumed from the on-site pile, not abstract inventory.
  - More stages for later/expensive buildings. Mill (endgame) has the most stages and most exotic materials.
  - Facade-only buildings: simplified version (smash boards + facade repair), no full staging.
  - *(Old BuildingManager.TryHammerHit that checks inventory directly will need rework — materials must be on-site first. Building needs per-stage state tracking + on-site material pile. New item: Stone in ContentDb.)*
- [ ] Narrative delivery: NO fragment/letter pickup system. Story comes from characters — residents reveal more as rep/heat/milestones change; new dialogue lines or scenes unlock when buildings are completed, heat thresholds are reached, etc. NPCs react to what the player has done. Replace FragmentDef with a milestone/unlock system TBD.
- [x] Facade-only buildings: pry boards (smash) + facade repair points (hammer). *(isFacadeOnly flag on Building skips carry step)*
- [x] Berta: schedule = (hour, Transform marker); fade-teleport between. Dialogue: portrait + line from rep-tier pool (JSON). Move-in: hand-scripted 10 s (cart SFX, walk, one line, window lights). No cutscene framework.
- [ ] Done: full Bakery arc — buy → clear → repair → dusk swap → sleep → Berta arrives → income. **Gate: show a human, watch the two-windows moment.**

## Phase 4 — Content build-out (wk 7–8)
1. [ ] Boarding House + Elias (hire: pay/day, auto-advances one flagged stage). Rent income.
2. [ ] Constable + heat teeth: restored → 3× heat decay, risky buyer blocked on Aas's shift; ruined → slow decay, frequent buyer.
3. [ ] General Store + Signe: discount flag on ingredients. Signe gets the biggest line count — she's the "world witnesses you" mirror (lines reference flags: owns bakery, heat > 40, ...). Not the stall automation — that's a separate hire.
4. [ ] Mrs. Holt: sells 4 deeds freely; Mill gated on rep threshold. Dialogue arc contempt → respect.
5. [ ] Mill: full construction stages (all 3: foundation, frame, finish — most expensive building). Stage 1 complete reveals cellar door → locked-from-inside line → title card. Grain discount wired.
6. [ ] Streetlamps unlock at 4 buildings owned.
7. [ ] Numbers pass: full run 2–2.5 h, Bakery at ~45–55 min. Tune prices/income, not the minigame.
- [ ] Done: a stranger plays start → cliffhanger, zero instructions.

## Phase 5 — Art + audio (wk 9–10)
- [ ] One 16×16 (minimum) tileset pack family (itch/Asset Store). Mixing packs = amateur tell.
- [ ] Facades: 6 restored base sprites (16×16 minimum) + shared overlay kit (boards, tarps, scaffold, grime) composited per state — not 24 sprites.
- [ ] Characters: portraits > walk cycles. Spend there. 2 emotion variants for Berta/Holt/Aas.
- [ ] Lighting evening: night palette, warm window Light2D (slight flicker), streetlamps, 3 interiors.
- [ ] Audio ≈ 18 SFX + 2 music loops. Priorities: deed-purchase stamp (make it THICK) and the lamp-lighting sting (commission this one if anything).

## Phase 6 — Playtest + tune (wk 11–12)
- [ ] 3 testers, recorded, you silent. Score against the spec's minute table.
- [ ] Collect: time to first sale (<18 min) · time to Bakery (<55 min) · any stuck >60 s (= missing affordance) · minigame quality trend 2★→4★ over 8 batches · unprompted reaction at two windows · **do they ask what's in the cellar? (the metric)**
- [ ] Cut pass: confused 2 of 3 → fix or cut; noticed by nobody → cut. No additions in wk 12.

## Content inventory
- Scenes: street + 3 interiors.
- Sprites: player (4-dir + carry, 16×16 minimum) · 6 facade bases + overlay kit · 6 NPC sprites · 6 portraits · props (still, 2 vats, cart, ~6 debris, scaffold, streetlamp, minimal furniture ×3 rooms) · items (bottle ×5 tints, grain, timber, nails) · UI (dialogue, journal, ledger, title card).
- Writing: ~120 dialogue lines + system strings ≈ 2,500 words. Write after systems exist so lines can reference real flags.
- Data: 6 BuildingDefs, 6 ResidentDefs, 2 RecipeDefs, 6 dialogue JSONs.

## Rules
- Still minigame deferred — fermentation auto-completes for now. Revisit when a fun design emerges.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 5. Juice (tweens, stings) allowed early.
- Save versioning + tolerant deserializer from day one.
- Every mid-build idea → the LATER note, unexamined. Review once, week 12.
