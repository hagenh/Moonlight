# Pacing Fix: "Nothing to Do While Fermenting"

## Problem

Phase 1 (Act 0) has a dead-time gap: player forages 3 berries (30 seconds), starts a 6-hour ferment, then waits ~4.6 real minutes with nothing to do. The loop is: gather → wait → collect → sell. The wait is the dominant activity.

## Design Decision

**Approach A: Forage-First Loop with Starter Berries**

The player starts with 3 Berry in inventory on day 1, allowing them to start fermenting immediately. While the batch brews (reduced to 3h), they explore the map, discover berry bushes, visit town, and meet NPCs. This inverts the dead time into the core gameplay loop.

## Revised Act 0 Daily Loop

### Day 1 (tutorial day)

1. Player wakes in tent at 8:00 with 3 Berry in inventory
2. Walks to vat → starts Berry Shine immediately (3h ferment, ready at ~11:00)
3. While fermenting: explores camp area, discovers town, meets NPCs
4. ~11:00: Berry Shine ready — collect it
5. Explores more, finds berry bushes scattered around the map for tomorrow
6. 18:00: Tormod arrives at Roadhouse back door → sell Berry Shine for 15g each
7. Day ends

### Day 2+

1. Forage scattered berry bushes (exploration-driven gathering)
2. Start ferment when you have enough ingredients
3. Explore / town / NPCs while it brews
4. Collect, sell to Tormod (dusk) or Cart (if it's a Cart day)
5. Gradually discover new recipes via `RecipeDiscovered` events

## Changes

### 1. Starting Inventory

- `GameManager` or `InventoryManager` gives the player 3 Berry on day 1 start
- Implementation: check if Day == 1 and Berry Shine not yet discovered → add berries, fire discovery event
- No new objects or systems needed

### 2. Berry Bush Distribution

- Remove the 3 clustered bushes from camp area
- Place 8-10 berry bushes scattered across the map:
  - A few near camp
  - Some along the road to town
  - Some in the town outskirts
  - A few hidden in corners
- Bushes respawn daily (existing behavior)
- Player must explore to find them — no bushes right next to the vat

### 3. Ferment Time Reduction

- Berry Shine: 6h → 3h
- Other recipes unchanged (Basic Mash 4h, Sweet Batch 6h, Highland Mash 8h, Aged Reserve 12h)

### 4. Recipe Discovery System

- Add `RecipeDiscovered` event to `GameEvents` with `string recipeId` parameter
- Recipes start **hidden** (not shown in ferment vat UI) until discovered
- **Berry Shine is always discovered** — it never goes through the discovery gate and is visible in the vat UI from game start (matches the Act 0 design's "no building gate, no min reputation — always unlocked" rule). No `RecipeDiscovered("berry_shine")` event is fired or needed.
- Other recipes discovered by: building restoration, NPC dialogue, world items — all fire `RecipeDiscovered` with different recipe IDs
- `FermentManager` tracks which recipes have been discovered; only shows discovered + unlocked recipes in the vat UI
- Starting berries give the player something to ferment immediately; the discovery system's tutorial role shifts to introducing later recipes (Basic Mash, etc.) rather than Berry Shine itself
- The event-based approach allows adding new discovery triggers later without changing the core system

## Alternatives Considered

- **Minute Tasks:** Keep 6h ferment but add chores (chop wood, fetch water). Rejected — feels like busywork, not gameplay.
- **Time Compression:** Reduce real-seconds-per-game-minute so waits are shorter. Rejected — compresses the entire game, hard to tune.
- **No starter berries:** Player must forage before first ferment. Rejected — first 5 minutes are still dead time.

## Status

Design approved, not yet implemented.
