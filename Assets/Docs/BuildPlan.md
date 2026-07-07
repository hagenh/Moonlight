# LAMPLIGHT — Slice Build Plan (Unity, lean)

Stack: Unity 2D (URP), C#, Tilemap, Light2D. Target: 10–12 weeks, evenings/weekends.

## Slice contents
- 1 street exterior (~60×20 tiles), 3 interiors: Roadhouse, Bakery, Mill. Other 3 buildings renovate facade-only (same verbs, no rooms).
- Systems: movement/interaction · building states · production (mash → ferment → still minigame) · selling ×3 channels · renovation verbs (smash/carry/hammer) · day-night + sleep-save · income tick · heat · reputation (3 dialogue tiers) · resident schedules (teleport/fade, no pathfinding) · fragments/journal · JSON save.
- 6 NPCs: Tormod, Berta, Elias, Aas, Signe, Mrs. Holt. ~20 lines each.
- 5 fragments, 1 move-in beat, 1 cliffhanger (Mill cellar) + title card.
- NOT in slice: pathfinding, weather, seasons, quest log, cutscene system, combat, minimap, settings beyond volume.

## Phase 0 — Skeleton (day 1)
- [x] URP 2D project, git.
- [ ] Defs as plain C# classes: `BuildingDef, ResidentDef, FragmentDef, RecipeDef`. A `ContentDb` singleton holds lists; loaded by a bootstrap scene. *(ItemDef + ContentDb done; BuildingDef, ResidentDef, FragmentDef deferred with their systems)*
- [ ] Runtime/save as plain classes: `GameState { day, cash, heat, rep, buildings{}, residents{}, batches[], inventory{}, flags }` + `SaveVersion`. *(deferred: GameManager holds state for now, separate when saving needs it)*
- [ ] Install Newtonsoft JSON package (`com.unity.nuget.newtonsoft-json`) — JsonUtility can't do dictionaries. Save to `Application.persistentDataPath`. *(deferred: too early)*
- [x] Static `GameEvents`: `BuildingStateChanged, FragmentFound, ResidentMovedIn, DayEnded, HeatChanged, RepChanged`. *(+ ToastRequested)*
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
- [ ] **Still minigame in its own scene** (iterate there, not in-world): *(DEFERRED)*
  - Vertical temp gauge, target band (~15%) drifts (sine + noise). Hold key = heat, release = cool, slight momentum. 25 s run. Score A = time-in-band.
  - Cut: scrolling strip heads/hearts/tails; hearts width scales with A. Press start/stop collection. Score B = hearts overlap, heavy penalty for heads.
  - Quality = ceil(ceiling × (0.35A + 0.65B)), 1–5★. Stars + sting + bottles to inventory.
  - Feel target: first try 2★, batch ten ~4★, 5★ rare. Tune band/drift until true. 3–4 sessions budgeted. **Go/no-go gate.**
- [x] Ferment: batches tick with game hours. Vat interactable: empty → choose recipe (consume ingredients) → fermenting → ready → minigame. Second vat purchasable mid-slice. *(continuous time via TimeManager.TotalGameMinutes; FermentBatch computes progress from start time; FermentManager checks in Update; no still minigame handoff yet)*
- [x] Selling: Tormod knocks (list price) · traveling cart 2 of 3 days (sells ingredients, buys bottles) · risky buyer event (2× price, +15 heat, 10% confiscation at heat > 50). *(SellManager schedules/spawns sellers as sprites at fixed street positions; SellUI with sell+buy tabs; SellerInteractable for world presence; ItemDef.basePrice + isBottle; cart status in HUD)*
- [x] HUD: cash, day, clock, heat, rep. *(all displays + inventory readout done)*
- [ ] Done: cart → mash → ferment → still → sell, no debug keys; 15 min of loop is near-fun.

## Phase 3 — Renovation + day cycle + save (wk 5–6)
- [x] Day: 1 real s ≈ 1.3 game min (~15 min days). Global Light2D color lerps noon→dusk→night. Window lights now pay off — screenshot the first lit window.
- [x] Sleep → tick, strict order: ferment → Elias repairs → income → heat decay → move-in checks → autosave. Comment the order; never reorder casually.
- [x] Verbs: smash (2–3 hits, drops carryable) · carry (overhead sprite, −20% speed, drop at pile) · hammer (hold-to-fill radial, consumes materials, 3–5 points/stage). *(Purchased state between Abandoned/Cleared; CarryState with -20% speed; HammerState with hold-to-fill progress; Debris+DebrisPile for carry loop; BuildingManager.TrySmashHit/TryHammerHit/CanHammer)*
- [ ] Fragment: one scripted debris per fragment building; smashing opens letter overlay + journal entry. Always mid-clearing, never as completion reward.
- [x] Facade-only buildings: pry boards (smash) + facade repair points (hammer). *(isFacadeOnly flag on Building skips carry step)*
- [ ] Berta: schedule = (hour, Transform marker); fade-teleport between. Dialogue: portrait + line from rep-tier pool (JSON). Move-in: hand-scripted 10 s (cart SFX, walk, one line, window lights). No cutscene framework.
- [ ] Done: full Bakery arc — buy → clear → fragment → repair → dusk swap → sleep → Berta arrives → income. **Gate: show a human, watch the two-windows moment.**

## Phase 4 — Content build-out (wk 7–8)
1. [ ] Boarding House + Elias (hire: pay/day, auto-advances one flagged stage). Rent income. Fragment 2.
2. [ ] Constable + heat teeth: restored → 3× heat decay, risky buyer blocked on Aas's shift; ruined → slow decay, frequent buyer. Fragment 3 in ruin either way.
3. [ ] General Store + Signe: discount flag + permanent buy/sell counter. Signe gets the biggest line count — she's the "world witnesses you" mirror (lines reference flags: owns bakery, heat > 40, ...).
4. [ ] Mrs. Holt: sells 4 deeds freely; Mill gated on rep threshold. Dialogue arc contempt → respect.
5. [ ] Mill: 3 stages, stage 1 reveals cellar door → locked-from-inside line → title card. Grain discount wired.
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
- Sprites: player (4-dir + carry, 16×16 minimum) · 6 facade bases + overlay kit · 6 NPC sprites · 6 portraits · props (still, 2 vats, cart, ~6 debris, scaffold, streetlamp, minimal furniture ×3 rooms) · items (bottle ×5 tints, grain, timber, nails) · UI (dialogue, journal, ledger, fragment paper, title card).
- Writing: ~120 dialogue lines + 5 fragments (~120 words each) + system strings ≈ 2,500 words. Write after systems exist so lines can reference real flags.
- Data: 6 BuildingDefs, 6 ResidentDefs, 5 FragmentDefs, 2 RecipeDefs, 6 dialogue JSONs.

## Rules
- Minigame is the product; it gets first claim on energy.
- No frameworks (dialogue/cutscene/quest). Hand-roll; extract patterns in game #2.
- No art before Phase 5. Juice (tweens, stings) allowed early.
- Save versioning + tolerant deserializer from day one.
- Every mid-build idea → the LATER note, unexamined. Review once, week 12.
