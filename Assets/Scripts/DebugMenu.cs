#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private int cashGrant = 100;

    private bool _visible;
    private bool _positioned;
    private Rect _windowRect = new Rect(10, 0, 220, 700);
    private Vector2 _scrollPos;

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame) _visible = !_visible;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        if (!_positioned)
        {
            _windowRect.y = Screen.height - _windowRect.height - 10;
            _positioned = true;
        }
        _windowRect = GUI.Window(0, _windowRect, DrawWindow, "Debug");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, 220, 20));

        if (GameManager.Instance == null) return;

        GUILayout.Label($"Cash: {GameManager.Instance.Cash}g");
        GUILayout.Label($"Day: {(TimeManager.Instance != null ? TimeManager.Instance.Day : 0)}");
        GUILayout.Label($"Time: {(TimeManager.Instance != null ? $"{TimeManager.Instance.Hour:00}:{TimeManager.Instance.Minute:00}" : "--:--")}");
        GUILayout.Label($"Heat: {GameManager.Instance.Heat}");
        GUILayout.Label($"Rep: {GameManager.Instance.Reputation}");

        GUILayout.Space(8);

        if (GUILayout.Button($"+{cashGrant}g"))
            GameManager.Instance.AddCash(cashGrant);

        if (GUILayout.Button("Advance Day"))
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.AdvanceToDayEnd();
        }

        if (GUILayout.Button("Heat +10"))
            GameManager.Instance.SetHeat(GameManager.Instance.Heat + 10);

        if (GUILayout.Button("Rep +10"))
            GameManager.Instance.SetReputation(GameManager.Instance.Reputation + 10);

        GUILayout.Space(8);

        if (GUILayout.Button("Reset Heat"))
            GameManager.Instance.SetHeat(0);

        if (GUILayout.Button("Reset Rep"))
            GameManager.Instance.SetReputation(0);

        GUILayout.Space(8);
        GUILayout.Label("--- Inventory ---");

        if (InventoryManager.Instance != null)
        {
            if (GUILayout.Button("+5 Grain"))
                InventoryManager.Instance.TryAdd(ContentDb.Grain, 5);
            if (GUILayout.Button("+5 Sugar"))
                InventoryManager.Instance.TryAdd(ContentDb.Sugar, 5);
            if (GUILayout.Button("+5 Yeast"))
                InventoryManager.Instance.TryAdd(ContentDb.Yeast, 5);
            if (GUILayout.Button("+5 Water"))
                InventoryManager.Instance.TryAdd(ContentDb.Water, 5);
            if (GUILayout.Button("+5 Timber"))
                InventoryManager.Instance.TryAdd(ContentDb.Timber, 5);
            if (GUILayout.Button("+5 Nails"))
                InventoryManager.Instance.TryAdd(ContentDb.Nails, 5);
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Time ---");

        if (TimeManager.Instance != null)
        {
            if (GUILayout.Button("+1 Hour"))
                TimeManager.Instance.AdvanceHour();
            if (GUILayout.Button("Skip to Dawn"))
                TimeManager.Instance.AdvanceToDayEnd();
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Sleep ---");

        if (SleepManager.Instance != null)
        {
            if (GUILayout.Button("Sleep Now"))
                SleepManager.Instance.BeginSleep();
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Fermentation ---");

        if (FermentManager.Instance != null)
        {
            if (GUILayout.Button("Skip Fermentation"))
            {
                foreach (var vat in FermentManager.Instance.Vats)
                {
                    if (vat.State == VatState.Fermenting && vat.CurrentBatch != null)
                    {
                        var old = vat.State;
                        vat.MarkReady();
                        GameEvents.OnVatStateChanged(vat, old, vat.State);
                    }
                }
            }
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Renovation ---");

        if (BuildingManager.Instance != null)
        {
            foreach (var b in BuildingManager.Instance.Buildings)
            {
                GUILayout.Label($"  {b.BuildingName}: {b.State}");
                if (b.State == BuildingState.Abandoned && GUILayout.Button($"  Buy {b.BuildingName}"))
                {
                    GameManager.Instance.AddCash(b.PurchaseCost);
                    BuildingManager.Instance.TryPurchase(b);
                }
                if (b.State == BuildingState.Purchased && GUILayout.Button("  Complete Smash"))
                    BuildingManager.Instance.ForceCompleteSmash(b);
                if (b.State == BuildingState.Cleared && GUILayout.Button("  Complete Repair"))
                    BuildingManager.Instance.ForceCompleteRepair(b);
                if (b.State != BuildingState.Abandoned && GUILayout.Button("  Reset"))
                    BuildingManager.Instance.ResetBuilding(b);
            }
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Residents ---");

        if (ResidentManager.Instance != null)
        {
            bool bertaMovedIn = ResidentManager.Instance.IsResidentMovedIn("berta");
            GUILayout.Label($"  Berta: {(bertaMovedIn ? "Moved in" : "Not yet")}");

            if (!bertaMovedIn)
            {
                if (GUILayout.Button("  Force Berta Move-In (instant)"))
                    ResidentManager.Instance.ForceBertaMoveIn();

                if (GUILayout.Button("  Force Berta Move-In (sequence)"))
                    ResidentManager.Instance.ForceBertaMoveInSequence();
            }
            else
            {
                if (GUILayout.Button("  Reset Berta"))
                    ResidentManager.Instance.ResetResident("berta");
            }
        }

        GUILayout.Space(8);
        GUILayout.Label("--- Stock ---");

        if (InventoryManager.Instance != null)
        {
            foreach (var kvp in InventoryManager.Instance.AllItems)
                GUILayout.Label($"  {kvp.Key.displayName}: {kvp.Value}");
        }
    }
}
#endif
