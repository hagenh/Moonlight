using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private TMPro.TextMeshProUGUI cashText;
    [SerializeField] private TMPro.TextMeshProUGUI dayText;
    [SerializeField] private TMPro.TextMeshProUGUI toastText;
    [SerializeField] private TMPro.TextMeshProUGUI repText;
    [SerializeField] private TMPro.TextMeshProUGUI clockText;
    [SerializeField] private TMPro.TextMeshProUGUI inventoryText;
    [SerializeField] private TMPro.TextMeshProUGUI cartStatusText;
    [SerializeField] private TMPro.TextMeshProUGUI hammerProgressText;
    [SerializeField] private float toastDuration = 2f;

    private float _toastTimer;
    private int _lastClockHour = -1;
    private int _lastClockMinute = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (toastText != null) toastText.gameObject.SetActive(false);
        if (hammerProgressText != null) hammerProgressText.gameObject.SetActive(false);
        if (repText != null) repText.text = "Rep: 0";
        if (clockText != null) clockText.text = "08:00";
        if (dayText != null) dayText.text = "Day 1";
        if (cashText != null && GameManager.Instance != null)
            cashText.text = $"{GameManager.Instance.Cash}g";
        if (inventoryText != null) inventoryText.text = "";
    }

    private void OnEnable()
    {
        GameEvents.ToastRequested += OnToastRequested;
        GameEvents.DayEnded += OnDayEnded;
        GameEvents.HourChanged += OnHourChanged;
        GameEvents.CashChanged += OnCashChanged;
        GameEvents.RepChanged += OnRepChanged;
        GameEvents.InventoryChanged += OnInventoryChanged;
        GameEvents.SellerArrived += OnSellerArrived;
        GameEvents.SellerLeft += OnSellerLeft;
        GameEvents.HammerStarted += OnHammerStarted;
        GameEvents.HammerProgress += OnHammerProgress;
        GameEvents.HammerEnded += OnHammerEnded;
    }

    private void OnDisable()
    {
        GameEvents.ToastRequested -= OnToastRequested;
        GameEvents.DayEnded -= OnDayEnded;
        GameEvents.HourChanged -= OnHourChanged;
        GameEvents.CashChanged -= OnCashChanged;
        GameEvents.RepChanged -= OnRepChanged;
        GameEvents.InventoryChanged -= OnInventoryChanged;
        GameEvents.SellerArrived -= OnSellerArrived;
        GameEvents.SellerLeft -= OnSellerLeft;
        GameEvents.HammerStarted -= OnHammerStarted;
        GameEvents.HammerProgress -= OnHammerProgress;
        GameEvents.HammerEnded -= OnHammerEnded;
    }

    private void Update()
    {
        UpdateInteractPrompt();
        UpdateClockDisplay();
        UpdateToast();
    }

    private void OnToastRequested(string message)
    {
        ShowToast(message);
    }

    private void OnDayEnded(int day)
    {
        if (dayText != null) dayText.text = $"Day {day + 1}";
    }

    private void OnHourChanged(int hour, int day)
    {
        if (dayText != null) dayText.text = $"Day {day}";
    }

    private void OnCashChanged(int newCash)
    {
        if (cashText != null) cashText.text = $"{newCash}g";
    }

    private void OnRepChanged(int newRep, int oldRep)
    {
        if (repText != null) repText.text = $"Rep: {newRep}";
    }

    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount)
    {
        UpdateInventoryDisplay();
    }

    private void OnSellerArrived(SellerType type)
    {
        UpdateCartStatus();
    }

    private void OnSellerLeft(SellerType type)
    {
        UpdateCartStatus();
    }

    private void OnHammerStarted(Building b)
    {
        if (hammerProgressText != null)
            hammerProgressText.gameObject.SetActive(true);
    }

    private void OnHammerProgress(Building b, float progress)
    {
        if (hammerProgressText != null)
            hammerProgressText.text = $"Hammering... {progress * 100:F0}%";
    }

    private void OnHammerEnded(Building b)
    {
        if (hammerProgressText != null)
            hammerProgressText.gameObject.SetActive(false);
    }

    private void UpdateCartStatus()
    {
        if (cartStatusText == null) return;
        if (SellManager.Instance != null && SellManager.Instance.IsCartInTown)
        {
            cartStatusText.gameObject.SetActive(true);
            cartStatusText.text = "Cart in town";
        }
        else
        {
            cartStatusText.gameObject.SetActive(false);
        }
    }

    private void UpdateInventoryDisplay()
    {
        if (inventoryText == null || InventoryManager.Instance == null) return;
        var sb = new System.Text.StringBuilder();
        foreach (var kvp in InventoryManager.Instance.AllItems)
            sb.AppendLine($"{kvp.Key.displayName}: {kvp.Value}");
        inventoryText.text = sb.ToString();
    }

    private void UpdateInteractPrompt()
    {
        var interactable = PlayerController.Instance != null ? PlayerController.Instance.CurrentInteractable : null;
        if (interactable == null)
        {
            if (promptText != null) promptText.gameObject.SetActive(false);
            return;
        }

        if (promptText != null) promptText.gameObject.SetActive(true);

        if (interactable is Building building)
        {
            bool atDoor = building.LastHitTrigger == building.DoorTrigger;

            if (atDoor && building.State != BuildingState.Restored)
            {
                promptText.gameObject.SetActive(false);
                return;
            }

            promptText.text = atDoor
                ? $"[E] Enter {building.BuildingName}"
                : building.State switch
                {
                    BuildingState.Abandoned => $"[E] Buy {building.BuildingName} ({building.PurchaseCost}g)",
                    BuildingState.Purchased when !building.BoardsSmashed
                        => $"[E] Smash ({building.SmashHitsDone}/{building.SmashHitsRequired})",
                    BuildingState.Purchased when building.BoardsSmashed
                        => "Clear the debris",
                    BuildingState.Cleared => building.RepairPointsDone >= building.TotalRepairPoints
                        ? $"[E] {building.BuildingName}"
                        : $"[Hold E] Repair ({building.RepairPointsDone}/{building.TotalRepairPoints})",
                    BuildingState.Restored => building.UncollectedIncome > 0
                        ? $"[E] Collect {building.UncollectedIncome}g from {building.BuildingName}"
                        : $"[E] {building.BuildingName}",
                    _ => $"[E] {building.BuildingName}"
                };
        }
        else if (interactable is Homestead homestead && !homestead.IsBuilt)
        {
            promptText.text = homestead.Stage switch
            {
                BuildStage.Site => $"[E] Homestead Site (need 3 Stone)",
                BuildStage.Foundation => $"[E] Build Frame (need 3 Wood)",
                BuildStage.Frame => $"[E] Build Walls (need 2 Wood, 3 Nails)",
                BuildStage.Walls => $"[E] Homestead",
                _ => $"[E] Homestead Site"
            };
        }
        else if (interactable is FermentVat vat)
        {
            promptText.text = vat.State switch
            {
                VatState.Empty => "[E] Start Batch",
                VatState.Fermenting => $"[E] Fermenting... {vat.CurrentBatch?.Progress * 100:F0}%",
                VatState.Ready => "[E] Collect Batch",
                _ => "[E] Vat"
            };
        }
        else if (interactable is SellerInteractable seller)
        {
            promptText.text = seller.sellerType switch
            {
                SellerType.Tormod => "[E] Sell to Tormod",
                SellerType.TravelingCart => "[E] Visit Cart",
                _ => "[E] Interact"
            };
        }
        else if (interactable is Bed)
        {
            promptText.text = "[E] Sleep";
        }
        else if (interactable is ExitDoor)
        {
            promptText.text = "[E] Exit";
        }
        else if (interactable is Crate)
        {
            promptText.text = PlayerController.Instance != null && PlayerController.Instance.IsCarryingAnything
                ? "Already carrying"
                : "[E] Pick up crate";
        }
        else if (interactable is DeliveryPoint dp)
        {
            promptText.text = dp.DeliveryType switch
            {
                DeliveryType.Cart => "[E] Deliver to Cart",
                DeliveryType.Tormod => "[E] Sell to Tormod",
                _ => "[E] Deliver"
            };
        }
        else if (interactable is Debris)
        {
            promptText.text = PlayerController.Instance != null && PlayerController.Instance.IsCarrying
                ? "Already carrying"
                : "[E] Pick up debris";
        }
        else if (interactable is DebrisPile)
        {
            promptText.text = PlayerController.Instance != null && PlayerController.Instance.IsCarrying
                ? "[E] Deposit debris"
                : "Debris pile";
        }
        else if (interactable is BerryBush bush)
        {
            promptText.text = bush.IsHarvested ? "Picked" : "[E] Forage";
        }
        else if (interactable is Resident resident)
        {
            promptText.text = resident.def != null
                ? $"[E] Talk to {resident.def.displayName}"
                : "[E] Talk";
        }
        else
        {
            promptText.text = "[E] Interact";
        }
    }

    private void UpdateClockDisplay()
    {
        if (TimeManager.Instance == null || clockText == null) return;
        int h = TimeManager.Instance.Hour;
        int m = TimeManager.Instance.Minute;
        if (h == _lastClockHour && m == _lastClockMinute) return;
        _lastClockHour = h;
        _lastClockMinute = m;
        clockText.text = $"{h:00}:{m:00}";
    }

    private void UpdateToast()
    {
        if (toastText == null || !toastText.gameObject.activeSelf) return;
        _toastTimer -= Time.deltaTime;
        if (_toastTimer <= 0f) toastText.gameObject.SetActive(false);
    }

    public void ShowToast(string message)
    {
        if (toastText == null) return;
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        _toastTimer = toastDuration;
    }
}
