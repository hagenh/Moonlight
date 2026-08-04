# LAMPLIGHT — Narrative System Design

## Philosophy
Story comes from people and from the one found object — not from a general collectible system. A blended system where NPCs and world events feed into dialogue progression and the grandfather's recipe book.

**Rule: No frameworks.** Hand-roll everything. Extract patterns in game #2.

## Two Pillars

### 1. NPC Relationships
- Per-NPC trust value (integer, keyed by resident ID)
- Trust grows through: dialogue (+1), quests (+5), building their home restored (+10)
- Conditional dialogue: lines that only appear when flags are set or trust thresholds met
- One-shot lines: first meeting, quest offers, story beats — shown once, never repeated
- Dialogue arcs: Holt's contempt→respect, Signe's world-witnessing commentary

### 2. Milestones
- First-time events detected from existing game systems
- Examples: first sale, first batch produced, first building restored, Berta moved in, 4 buildings restored
- Detected by `MilestoneDetector` subscribing to existing `GameEvents`
- Each milestone sets a narrative flag that can gate dialogue, a recipe book page, or world state
- Logged in journal with day stamp

---

### Architecture

### NarrativeFlags — Global boolean state
```
Static class, HashSet<string>
IsSet(flag) / Set(flag) / Clear(flag) / AllFlags
Set() fires GameEvents.OnFlagSet(flag)
Flags: bakery_restored, first_sale, berta_moved_in, smithy_restored, etc.
```

### MilestoneDetector — Event → Flag bridge
```
MonoBehaviour, subscribes to GameEvents
BuildingStateChanged → {building}_restored, four_buildings_restored
InventoryChanged (bottle) → first_batch_produced
ResidentMovedIn → {resident}_moved_in
FlagSet → check for milestone-triggered recipe book pages
```

### RecipeBookPageDef — Story page data
```
Plain C# class (follows ItemDef pattern)
id, title, body (~120 words, the grandfather's voice), triggerFlag, page order
5 definitions registered in ContentDb — one owned from Act 0 minute 3, no trigger;
the rest gated behind triggerFlag (see "Recipe Book Content" below)
```

### RecipeBookUI — Page-reveal overlay
```
OnGUI overlay: "a page is legible now", title, body text
Subscribes to RecipeBookState's reveal event
No flip-through viewer yet — data layer only
```

### RecipeBookState — Legible-page tracker
```
List<RecipeBookPageDef> legible pages, List<string> milestone log
RevealPage(), AddMilestone()
Subscribes to FlagSet (checks each undiscovered page's triggerFlag) and to trust-threshold
crossings for the one trust-gated page
Data layer only — no viewer UI yet
```

### DialogueLine — Conditional dialogue entry
```
Serializable struct: text, requiredFlag, setsFlag, minTrust, oneShot, portraitEmotion
Priority-sorted: conditional lines checked first, rep-tier pool as fallback
```

### DialogueResolver — Smart dialogue selection
```
Static resolver:
1. Check conditional lines in priority order
2. Skip if requiredFlag not set, or minTrust not met, or oneShot already seen
3. On match: if oneShot add to SeenLines, if setsFlag set it
4. Fallback: rep-tier pool (existing behavior)
```

### Per-NPC Trust
```
Dictionary<string, int> on GameManager or ResidentManager
Trust increments on dialogue interaction (+1), quest completion (+5), building's home restored (+10)
Feeds into DialogueResolver alongside global reputation
```

---

## Data Flow

```
GameEvents (existing)
    │
    ├─→ MilestoneDetector ─→ NarrativeFlags
    │                              │
    │                              ├─→ DialogueResolver
    │                              ├─→ RecipeBookState ─→ RecipeBookUI
    │                              └─→ Building/NPC gates
    │
    ├─→ Resident.Interact() ─→ DialogueResolver ─→ DialogueUI
    │                                              └─→ On close: trust++, set flags
    │
    └─→ SleepManager pipeline ─→ Narrative checkpoint
                                       ├─→ Day-gated events
                                       └─→ Flag-gated events
```

---

## Recipe Book Content (Slice — 5 pages)

**Replaces the fragment collectible system (cut 2026-08-04).** One object, owned from Act 0 minute 3, mostly ruined — legible pages accumulate over the game instead of new objects being found. Full design: `docs/superpowers/specs/2026-08-04-recipe-book-narrative-redesign-design.md`.

| # | Trigger | Why |
|---|---|---|
| 1 | Owned from Act 0, minute 3 — no trigger | The one legible page from the start |
| 2 | `bakery_restored` | Reuses an existing flag already used elsewhere in this doc |
| 3 | `smithy_restored` | Aksel — "he built the still's twin," the existing first-cellar-thread |
| 4 | Mrs. Holt trust threshold | She "knew the original operation" (`GameDesign.md` Part 3, NPCs) |
| 5 | `mill_stage1_complete` | The cellar door beat |

Titles and bodies are not written yet — placeholder, matching the miller's vetoable-name treatment elsewhere in the design. A sixth page (e.g. `boarding_house_restored`) is a straightforward future addition, not a cap.

---

## NPC Dialogue Tiers (expanded from rep-only to rep + trust + flags)

### Berta (Bakery)
- **One-shot:** First meeting greeting, move-in thanks
- **Conditional:** "The oven's warm" (bakery_restored)
- **Pool:** 3 tiers by trust (same as current rep tiers)

### Signe (General Store)
- **Flag-referencing:** "Owns the bakery now, do you?" (bakery_restored), "Four buildings lit up!" (four_buildings_restored)
- **Biggest line count** — she's the "world witnesses you" mirror

### Mrs. Holt
- **Arc:** Contempt → respect (trust-gated, not just rep)
- **Low trust:** Dismissive, refuses Mill deed
- **High trust:** Opens up, grants Mill access; a recipe book page becomes legible (see above)

### Elias (Boarding House)
- **Functional:** Hire dialogue, repair reports
- **Story:** A dialogue line about the old village

---

## What NOT to Build

- No quest log UI (BuildPlan explicitly excludes)
- No dialogue node editor or visual tool
- No branching dialogue with player choices
- No recipe book viewer UI (data layer only for now)
- No cutscene framework (keep using coroutines)
- No save/load integration yet (flags are in-memory)
- No per-NPC save format beyond simple dictionaries

---

## Implementation Order

1. `NarrativeFlags` + `GameEvents` additions (foundation)
2. `RecipeBookPageDef` + `ContentDb` registration
3. `MilestoneDetector`
4. `RecipeBookState` + `RecipeBookUI`
5. `DialogueLine` struct + `DialogueResolver`
6. `ResidentDef` upgrade + `Resident` trust
7. Berta conditional lines
