using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private TMPro.TextMeshProUGUI cashText;
    [SerializeField] private TMPro.TextMeshProUGUI dayText;
    [SerializeField] private TMPro.TextMeshProUGUI toastText;
    [SerializeField] private float toastDuration = 2f;

    private float _toastTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (toastText != null) toastText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.ToastRequested += OnToastRequested;
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.ToastRequested -= OnToastRequested;
        GameEvents.DayEnded -= OnDayEnded;
    }

    private void Update()
    {
        UpdateInteractPrompt();
        UpdateCashDisplay();
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
                    BuildingState.Cleared => $"[E] Repair {building.buildingName} ({building.repairCost}g)",
                    BuildingState.Restored => building.UncollectedIncome > 0
                        ? $"[E] Collect {building.UncollectedIncome}g from {building.buildingName}"
                        : $"[E] {building.buildingName}",
                    _ => $"[E] {building.buildingName}"
                };
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
