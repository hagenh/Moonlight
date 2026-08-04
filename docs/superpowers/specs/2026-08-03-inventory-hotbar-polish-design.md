# Inventory Hotbar — Polish Pass Design

**Follows:** `docs/superpowers/specs/2026-08-03-inventory-hotbar-design.md` (implemented on branch `feature/inventory-hotbar`, not yet merged).

## Goal

Address feedback from a visual review of the just-built hotbar before merging:

1. The selected-slot highlight (`UI_SlotsA_01`, light/gold) blends into the green grass behind the HUD — needs a background-independent treatment.
2. The 9 slot views are instantiated at runtime from a hidden template — the user wants them authored directly in the scene, visible in Edit Mode.
3. New feature: a label above the hotbar shows the newly-active item's name (and count) for 3 seconds when the active slot changes.

## 1. Selected-slot outline

**Problem:** `InventorySlotView.Render` swaps the slot's own background `Image` between `normalSprite` and `selectedSprite`. That works for the full inventory screen because it sits on an opaque dark panel. The hotbar sits directly over gameplay (grass, dirt, etc.), so a light sprite swap isn't reliably visible.

**Fix:** Don't swap the hotbar's background at all — set both `normalSprite` and `selectedSprite` to `UI_SlotsA_04` on every hotbar slot, so `InventorySlotView.Render(slot, false)` (always called with `false` from the hotbar) never changes the background. Add a second, additive visual: a sibling `Outline` `Image` per slot using `UI_SlotsA_02` (`Assets/Sprite/Post-Baroque UI Kit/Slots/Slots A/UI_SlotsA_02.png`, 32×32, a ring shape with a transparent center — same format/family as the other already-used slot sprites), 9-sliced, sized to overlay the whole 60×60 slot, tinted a bright gold/amber (`color: {r: 1, g: 0.75, b: 0, a: 1}` — the sprite's own native tone is a pale gold that has the same low-contrast problem as `UI_SlotsA_01`; the tint makes it a saturated, clearly-visible highlight regardless of background), inactive by default. `HotbarUI` enables exactly one slot's `Outline` GameObject — whichever matches `ActiveSlotIndex` — and disables the rest.

(Earlier draft of this spec proposed `UI_Frames_05` for the outline; inspecting its `.meta` showed it's actually two decorative 64×25 horizontal strips, not a single enclosed frame, so it doesn't fit a square icon slot. `UI_SlotsA_02`'s ring shape is the corrected choice — transparent center means it layers over the existing background/icon/count without obscuring them.)

**No changes to `InventorySlotView.cs`** — this is purely a scene-wiring choice (both sprite fields pointing at the same asset) plus a new sibling GameObject `HotbarUI` toggles directly.

`UI_SlotsA_02.png.meta` needs the same 9-slice treatment already applied to the other kit sprites in this feature (it currently sits at the same unconfigured defaults `UI_SlotsA_01`/`UI_SlotsA_04` had before that treatment): `spriteMode: 1` (Single), `spriteBorder` — start at `{x: 10, y: 10, z: 10, w: 10}` (matching the identical treatment already given to the same-sized `UI_SlotsA_01`/`UI_SlotsA_04`), tune by screenshot.

## 2. Edit-time-authored slots

**Problem:** `HotbarUI.BuildSlots()` instantiates 9 `InventorySlotView` copies from a hidden template at runtime (`Awake`). Nothing is visible in the scene until Play Mode.

**Fix:** Remove `slotTemplate`, `slotContainer`, `BuildSlots()`, and the `Instantiate` call entirely. `HotbarUI` instead holds a fixed-size array of slot references, each pairing the slot's `InventorySlotView` with its new `Outline` child:

```csharp
[System.Serializable]
private struct HotbarSlotRefs
{
    public InventorySlotView view;
    public GameObject outline;
}

[SerializeField] private HotbarSlotRefs[] slots;
```

All 9 entries are wired in the Inspector (via Unity MCP) to 9 real, always-present GameObjects built directly under the existing `SlotContainer` (`HorizontalLayoutGroup`) — no hidden/disabled template. `HorizontalLayoutGroup` lays out active children in both Edit Mode and Play Mode identically, so this alone satisfies "visible in Edit Mode."

`Refresh()` becomes:

```csharp
private void Refresh()
{
    if (InventoryManager.Instance == null) return;

    for (int i = 0; i < slots.Length; i++)
    {
        var slot = InventoryManager.Instance.Slots[i];
        slots[i].view.Render(slot, false);
        if (slots[i].outline != null)
            slots[i].outline.SetActive(i == InventoryManager.Instance.ActiveSlotIndex);
    }
}
```

`slots.Length` is expected to equal `InventoryManager.HotbarSlotCount` (9) — authored to match, not enforced in code (no validation added for a fixed, hand-wired array; a mismatch would only happen from a scene-editing mistake, not a runtime condition).

## 3. Item-announcement label

**New component:** `Assets/Scripts/UI/HotbarAnnouncementUI.cs` — kept separate from `HotbarUI` (which already owns input + slot rendering) rather than growing it further.

```csharp
using System.Collections;
using TMPro;
using UnityEngine;

public class HotbarAnnouncementUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text label;

    private const float DisplayDuration = 3f;
    private Coroutine _hideRoutine;

    private void OnEnable()
    {
        GameEvents.ActiveSlotChanged += OnActiveSlotChanged;
        if (root != null) root.SetActive(false);
    }

    private void OnDisable()
    {
        GameEvents.ActiveSlotChanged -= OnActiveSlotChanged;
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
    }

    private void OnActiveSlotChanged(int index)
    {
        if (InventoryManager.Instance == null) return;
        var slot = InventoryManager.Instance.Slots[index];

        if (slot.IsEmpty)
        {
            if (root != null) root.SetActive(false);
            return;
        }

        if (label != null)
            label.text = slot.Count > 1 ? $"{slot.Item.displayName} x{slot.Count}" : slot.Item.displayName;
        if (root != null) root.SetActive(true);

        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(DisplayDuration);
        if (root != null) root.SetActive(false);
        _hideRoutine = null;
    }
}
```

Behavior, matching the answered clarifying questions:
- Triggers only on `GameEvents.ActiveSlotChanged` (i.e., only when the active slot actually changes — pressing the already-active slot's key is already a no-op in `InventoryManager.SetActiveSlot` and fires no event, so it correctly does not re-trigger or reset the timer).
- Selecting an empty slot shows nothing (hides any currently-shown label immediately, per the "show nothing" answer).
- Text format is `"{displayName} x{count}"`, count omitted when `count <= 1` — this mirrors `InventorySlotView`'s own existing count-display rule (`slot.Count > 1 ? slot.Count.ToString() : ""`) rather than the full inventory screen's `"Stack: N/30"` wording, per the answered question.
- Re-selecting a different non-empty slot while the label is already showing restarts the 3-second timer against the new item (stops the old coroutine, starts a new one) rather than stacking timers.

**Scene wiring:** a new `ItemAnnouncement` GameObject as a sibling of `Hotbar` under `HUDCanvas`, anchored bottom-center like the hotbar but offset above it (e.g. `anchorMin/Max (0.5, 0)`, `pivot (0.5, 0)`, `anchoredPosition (0, hotbar height + hotbar's own y-offset + margin)`), containing a `TMP_Text` sized to comfortably fit an item name + count, inactive by default. `HotbarAnnouncementUI.root` wires to this GameObject (or a child panel if a background is added — a plain text label is sufficient, no panel sprite requested).

## Testing

- No new automated tests planned for the announcement timing itself (a 3-second real-time coroutine is impractical to unit test meaningfully in this codebase's existing test style, which has no precedent for `WaitForSeconds`-based PlayMode timing tests). Verified manually instead, consistent with how the rest of this feature's visual behavior was verified (Play Mode + screenshots via Unity MCP, `SetActiveSlot`/`TryAdd` as a proxy for real key presses).
- Existing `InventoryManager`/`GameEvents` tests are unaffected — no changes to `InventoryManager.cs` or `GameEvents.cs` in this polish pass; `ActiveSlotChanged` already exists and is already covered.
- Full EditMode + PlayMode suite must stay green (same pre-existing, unrelated baseline as before: `FootstepTileTests` ×5, `HomesteadTests.SetBuilt_SwapsSpriteToBuiltSprite`).

## Scope boundary

No changes to `InventoryManager.cs`, `GameEvents.cs`, `InventorySlotView.cs`, or the `Hotbar` input action — this pass only touches `HotbarUI.cs` (rewritten slot-reference mechanism), adds `HotbarAnnouncementUI.cs`, and updates `SampleScene.unity` + `UI_Frames_05.png.meta`.
