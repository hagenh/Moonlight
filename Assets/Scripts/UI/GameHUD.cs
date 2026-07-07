using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private TMPro.TextMeshProUGUI cashText;
    [SerializeField] private TMPro.TextMeshProUGUI dayText;
    [SerializeField] private TMPro.TextMeshProUGUI toastText;
    [SerializeField] private TMPro.TextMeshProUGUI heatText;
    [SerializeField] private TMPro.TextMeshProUGUI repText;
    [SerializeField] private TMPro.TextMeshProUGUI clockText;
    [SerializeField] private TMPro.TextMeshProUGUI inventoryText;
    [SerializeField] private TMPro.TextMeshProUGUI cartStatusText;
    [SerializeField] private TMPro.TextMeshProUGUI hammerProgressText;
    [SerializeField] private float toastDuration = 2f;

    private float _toastTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (toastText != null) toastText.gameObject.SetActive(false);
        if (hammerProgressText != null) hammerProgressText.gameObject.SetActive(false);
        if (heatText != null) heatText.text = "Heat: 0";
        if (repText != null) repText.text = "Rep: 0";
        if (clockText != null) clockText.text = "06:00";
        if (dayText != null) dayText.text = "Day 1";
        if (inventoryText != null) inventoryText.text = "";
    }

    private void OnEnable()
    {
        GameEvents.ToastRequested += OnToastRequested;
        GameEvents.DayEnded += OnDayEnded;
        GameEvents.HourChanged += OnHourChanged;
        GameEvents.HeatChanged += OnHeatChanged;
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
        GameEvents.HeatChanged -= OnHeatChanged;
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
        UpdateCashDisplay();
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

    private void OnHeatChanged(int newHeat, int oldHeat)
    {
        if (heatText != null) heatText.text = $"Heat: {newHeat}";
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
        if (promptText == null) return;

        if (interactable is Building building)
        {
            bool atDoor = building.LastHitTrigger == building.DoorTrigger;

            if (atDoor && building.State != BuildingState.Restored)
            {
                promptText.gameObject.SetActive(false);
                return;
            }

            promptText.text = atDoor
                ? $"[E] Enter {building.buildingName}"
                : building.State switch
                {
                    BuildingState.Abandoned => $"[E] Buy {building.buildingName} ({building.purchaseCost}g)",
                    BuildingState.Purchased when !building.BoardsSmashed
                        => $"[E] Smash ({building.SmashHitsDone}/{building.smashHitsRequired})",
                    BuildingState.Purchased when building.BoardsSmashed
                        => "Clear the debris",
                    BuildingState.Cleared => building.RepairPointsDone >= building.totalRepairPoints
                        ? $"[E] {building.buildingName}"
                        : $"[Hold E] Repair ({building.RepairPointsDone}/{building.totalRepairPoints})",
                    BuildingState.Restored => building.UncollectedIncome > 0
                        ? $"[E] Collect {building.UncollectedIncome}g from {building.buildingName}"
                        : $"[E] {building.buildingName}",
                    _ => $"[E] {building.buildingName}"
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
                SellerType.RiskyBuyer => "[E] Shady Deal",
                _ => "[E] Interact"
            };
        }
        else if (interactable is Bed)
        {
            promptText.text = "[E] Sleep";
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
        else
        {
            promptText.text = "[E] Interact";
        }
    }

    private void UpdateCashDisplay()
    {
        if (GameManager.Instance == null) return;
        if (cashText != null) cashText.text = $"{GameManager.Instance.Cash}g";
    }

    private void UpdateClockDisplay()
    {
        if (TimeManager.Instance == null) return;
        if (clockText != null) clockText.text = $"{TimeManager.Instance.Hour:00}:{TimeManager.Instance.Minute:00}";
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
