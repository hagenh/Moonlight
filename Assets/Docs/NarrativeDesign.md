# LAMPLIGHT — Narrative System Design

## Philosophy
Story comes from people, found items, milestones, and actions — not just from smashing debris. A blended system where NPCs, world events, and collectibles all feed into a shared journal and drive dialogue progression.

**Rule: No frameworks.** Hand-roll everything. Extract patterns in game #2.

## Three Pillars

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
- Each milestone sets a narrative flag that can gate dialogue, fragments, or world state
- Logged in journal with day stamp

### 3. Collectibles (Fragments)
- Story pieces discovered from multiple sources, not just debris
- Trigger types: `SmashDebris` (from renovation), `Milestone` (from flag), `NPCGift` (from dialogue)
- Each fragment: title + ~120 word body
- Displayed as letter overlay when found, stored in journal
- 5 fragments for the slice, each revealing a piece of the village's story

---

## Architecture

### NarrativeFlags — Global boolean state
```
Static class, HashSet<string>
IsSet(flag) / Set(flag) / Clear(flag) / AllFlags
Set() fires GameEvents.OnFlagSet(flag)
Flags: bakery_restored, first_sale, berta_moved_in, bakery_fragment_found, etc.
```

### MilestoneDetector — Event → Flag bridge
```
MonoBehaviour, subscribes to GameEvents
BuildingStateChanged → {building}_restored, four_buildings_restored
InventoryChanged (bottle) → first_batch_produced
ResidentMovedIn → {resident}_moved_in
FlagSet → check for milestone-triggered fragments
```

### FragmentDef — Story fragment data
```
Plain C# class (follows ItemDef pattern)
id, title, body, triggerType (SmashDebris/Milestone/NPCGift), sourceId
5 definitions registered in ContentDb
```

### FragmentUI — Letter overlay
```
OnGUI overlay: parchment background, title, body text
Subscribes to FragmentFound event
On close: adds to journal, sets narrative flag
```

### JournalState — Collection tracker
```
List<FragmentDef> collected, List<string> milestone log
AddFragment(), AddMilestone()
Subscribes to FragmentFound and FlagSet
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
Trust increments on dialogue interaction (+1), quest completion (+5), building restored (+10)
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
    │                              ├─→ JournalState
    │                              └─→ Building/NPC gates
    │
    ├─→ Resident.Interact() ─→ DialogueResolver ─→ DialogueUI
    │                                              └─→ On close: trust++, set flags
    │
    ├─→ BuildingManager.SmashHit ─→ Fragment check ─→ FragmentUI
    │
    └─→ SleepManager pipeline ─→ Narrative checkpoint
                                       ├─→ Day-gated events
                                       └─→ Flag-gated events
```

---

## Fragment Content (Slice — 5 fragments)

| # | Title | Trigger | Source |
|---|---|---|---|
| 1 | The Baker's Last Loaf | SmashDebris | Bakery (mid-clearing) |
| 2 | A Carpenter's Ledger | SmashDebris | Boarding House (mid-clearing) |
| 3 | The Constable's Report | Milestone | `constable_restored` flag |
| 4 | A Merchant's Confession | NPCGift | Signe dialogue (trust ≥ 10) |
| 5 | The Mill Cellar | Milestone | `mill_stage1_complete` flag |

---

## NPC Dialogue Tiers (expanded from rep-only to rep + trust + flags)

### Berta (Bakery)
- **One-shot:** First meeting greeting, move-in thanks
- **Conditional:** "The oven's warm" (bakery_restored), "Heard someone's been dealing shady..." (heat > 40)
- **Pool:** 3 tiers by trust (same as current rep tiers)

### Signe (General Store)
- **Flag-referencing:** "Owns the bakery now, do you?" (bakery_restored), "Word is you're running hot..." (heat > 40), "Four buildings lit up!" (four_buildings_restored)
- **Biggest line count** — she's the "world witnesses you" mirror

### Mrs. Holt
- **Arc:** Contempt → respect (trust-gated, not just rep)
- **Low trust:** Dismissive, refuses Mill deed
- **High trust:** Opens up, grants Mill access

### Elias (Boarding House)
- **Functional:** Hire dialogue, repair reports
- **Story:** Fragment about the old village

### Aas (Constable)
- **Functional:** Heat management dialogue, shift schedules
- **Story:** The constable's report (fragment)

---

## What NOT to Build

- No quest log UI (BuildPlan explicitly excludes)
- No dialogue node editor or visual tool
- No branching dialogue with player choices
- No journal viewer UI (data layer only for now)
- No cutscene framework (keep using coroutines)
- No save/load integration yet (flags are in-memory)
- No per-NPC save format beyond simple dictionaries

---

## Implementation Order

1. `NarrativeFlags` + `GameEvents` additions (foundation)
2. `FragmentDef` + `ContentDb` registration
3. `MilestoneDetector`
4. Fragment discovery in `BuildingManager` smash pipeline
5. `FragmentUI` + `JournalState`
6. `DialogueLine` struct + `DialogueResolver`
7. `ResidentDef` upgrade + `Resident` trust
8. Berta conditional lines
