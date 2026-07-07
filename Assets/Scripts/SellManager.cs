using UnityEngine;

public class SellManager : MonoBehaviour
{
    public static SellManager Instance { get; private set; }

    [SerializeField] private Transform tormodPosition;
    [SerializeField] private Transform cartPosition;
    [SerializeField] private Transform riskyBuyerPosition;
    [SerializeField] private float riskyBuyerChance = 0.4f;
    [SerializeField] private int tormodArriveMin = 8;
    [SerializeField] private int tormodArriveMax = 14;
    [SerializeField] private int cartArriveHour = 10;
    [SerializeField] private int cartLeaveHour = 18;
    [SerializeField] private int riskyArriveMin = 12;
    [SerializeField] private int riskyArriveMax = 18;

    private SellerInteractable _tormodInstance;
    private SellerInteractable _cartInstance;
    private SellerInteractable _riskyInstance;

    private int _tormodArriveHour;
    private int _riskyArriveHour;
    private bool _riskyToday;

    public SellerType? ActiveSeller { get; private set; }
    public bool IsCartInTown => _cartInstance != null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
        GameEvents.HourChanged += OnHourChanged;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
        GameEvents.HourChanged -= OnHourChanged;
    }

    private void OnDayEnded(int day)
    {
        RemoveSeller(ref _tormodInstance, SellerType.Tormod);
        RemoveSeller(ref _cartInstance, SellerType.TravelingCart);
        RemoveSeller(ref _riskyInstance, SellerType.RiskyBuyer);

        PlanTomorrow(day + 1);
    }

    private void OnHourChanged(int hour, int day)
    {
        if (hour == _tormodArriveHour && _tormodInstance == null)
            SpawnTormod();

        if (hour == cartArriveHour && IsCartDay(day) && _cartInstance == null)
            SpawnCart();

        if (hour == cartLeaveHour && _cartInstance != null)
            RemoveSeller(ref _cartInstance, SellerType.TravelingCart);

        if (hour == _riskyArriveHour && _riskyToday && _riskyInstance == null)
            SpawnRiskyBuyer();
    }

    private void Start()
    {
        PlanTomorrow(TimeManager.Instance != null ? TimeManager.Instance.Day : 1);
    }

    private void PlanTomorrow(int day)
    {
        _tormodArriveHour = Random.Range(tormodArriveMin, tormodArriveMax + 1);
        _riskyToday = Random.value < riskyBuyerChance;
        _riskyArriveHour = Random.Range(riskyArriveMin, riskyArriveMax + 1);
    }

    private bool IsCartDay(int day) => day % 3 != 0;

    private Vector3 GetPosition(Transform marker) => marker != null ? marker.position : Vector3.zero;

    private void SpawnTormod()
    {
        _tormodInstance = SellerInteractable.Create(SellerType.Tormod, GetPosition(tormodPosition));
        GameEvents.OnSellerArrived(SellerType.Tormod);
    }

    private void SpawnCart()
    {
        _cartInstance = SellerInteractable.Create(SellerType.TravelingCart, GetPosition(cartPosition));
        GameEvents.OnSellerArrived(SellerType.TravelingCart);
    }

    private void SpawnRiskyBuyer()
    {
        _riskyInstance = SellerInteractable.Create(SellerType.RiskyBuyer, GetPosition(riskyBuyerPosition));
        GameEvents.OnSellerArrived(SellerType.RiskyBuyer);
    }

    private void RemoveSeller(ref SellerInteractable instance, SellerType type)
    {
        if (instance == null) return;
        Destroy(instance.gameObject);
        instance = null;
        GameEvents.OnSellerLeft(type);
    }

    public void OpenSellMenu(SellerType type)
    {
        ActiveSeller = type;
        GameEvents.OnSellMenuRequested(type);
    }

    public void CloseSellMenu()
    {
        ActiveSeller = null;
    }

    public int GetSellPrice(ItemDef item, SellerType seller)
    {
        float multiplier = seller switch
        {
            SellerType.RiskyBuyer => 2f,
            _ => 1f
        };
        return Mathf.RoundToInt(item.basePrice * multiplier);
    }

    public int GetBuyPrice(ItemDef item) => item.basePrice;

    public bool ExecuteSale(ItemDef item, int count, SellerType seller)
    {
        if (InventoryManager.Instance == null || GameManager.Instance == null) return false;
        if (!InventoryManager.Instance.Has(item, count)) return false;

        if (seller == SellerType.RiskyBuyer)
        {
            if (GameManager.Instance.Heat > 50 && Random.value < 0.1f)
            {
                InventoryManager.Instance.TryRemove(item, count);
                GameEvents.OnToastRequested("Confiscated! Items seized.");
                GameManager.Instance.AddHeat(15);
                return true;
            }
        }

        int price = GetSellPrice(item, seller) * count;
        InventoryManager.Instance.TryRemove(item, count);
        GameManager.Instance.AddCash(price);
        GameEvents.OnToastRequested($"+{price}g");

        if (seller == SellerType.RiskyBuyer)
            GameManager.Instance.AddHeat(15);

        return true;
    }

    public bool ExecutePurchase(ItemDef item, int count)
    {
        if (InventoryManager.Instance == null || GameManager.Instance == null) return false;

        int cost = GetBuyPrice(item) * count;
        if (!GameManager.Instance.TrySpend(cost)) return false;

        InventoryManager.Instance.TryAdd(item, count);
        return true;
    }
}
