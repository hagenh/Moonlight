using UnityEngine;

public class SellManager : MonoBehaviour
{
    public static SellManager Instance { get; private set; }

    [SerializeField] private Transform cartPosition;
    [SerializeField] private int cartArriveHour = 10;
    [SerializeField] private int cartLeaveHour = 18;
    [SerializeField] private Transform tormodPosition;
    [SerializeField] private int tormodArriveHour = 18;
    [SerializeField] private int tormodLeaveHour = 6;

    private SellerInteractable _cartInstance;
    private SellerInteractable _tormodInstance;

    private IRng _rng = UnityRng.Instance;
    private bool _tormodNailsGranted;

    public bool IsCartInTown => _cartInstance != null;
    public bool IsTormodInTown => _tormodInstance != null;

    internal void SetRng(IRng rng) => _rng = rng;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // OnHourChanged only fires on a change, so presence has to be settled once
        // at startup or Tormod would be missing until the clock next ticks over.
        if (TimeManager.Instance != null)
            UpdateTormodPresence(TimeManager.Instance.Hour);
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
        GameEvents.HourChanged += OnHourChanged;
        GameEvents.DeliveryMade += OnDeliveryMade;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
        GameEvents.HourChanged -= OnHourChanged;
        GameEvents.DeliveryMade -= OnDeliveryMade;
    }

    private void OnDayEnded(int day)
    {
        RemoveCart();
    }

    private void OnHourChanged(int hour, int day)
    {
        UpdateTormodPresence(hour);
    }

    /// <summary>
    /// Tormod keeps hours: dusk to dawn at the Roadhouse back door. He is the
    /// Act 0 buyer, not a permanent shopfront — the gap between a finished
    /// ferment and his arrival is the prologue's exploration window.
    /// </summary>
    private void UpdateTormodPresence(int hour)
    {
        bool shouldBePresent = SellerRules.IsPresent(hour, tormodArriveHour, tormodLeaveHour);

        if (shouldBePresent && _tormodInstance == null)
            SpawnTormod();
        else if (!shouldBePresent && _tormodInstance != null)
            RemoveTormod();
    }

    private void SpawnCart()
    {
        Vector3 pos = cartPosition != null ? cartPosition.position : Vector3.zero;
        _cartInstance = SellerInteractable.Create(SellerType.TravelingCart, pos);
        GameEvents.OnSellerArrived(SellerType.TravelingCart);
    }

    private void RemoveCart()
    {
        if (_cartInstance == null) return;

        Destroy(_cartInstance.gameObject);
        _cartInstance = null;
        GameEvents.OnSellerLeft(SellerType.TravelingCart);
    }

    private void SpawnTormod()
    {
        Vector3 pos = tormodPosition != null ? tormodPosition.position : Vector3.zero;

        if (ContentDb.Instance != null && ContentDb.Instance.TormodPrefab != null)
        {
            var go = Instantiate(ContentDb.Instance.TormodPrefab, pos, Quaternion.identity);
            go.name = "Tormod";
            go.layer = LayerMask.NameToLayer("Interactable");
            _tormodInstance = go.GetComponent<SellerInteractable>();

            var animator = go.GetComponent<DirectionalSpriteAnimator>();
            if (animator != null)
            {
                animator.Initialize();
                animator.Play("idle");
            }
        }
        else
        {
            _tormodInstance = SellerInteractable.Create(SellerType.Tormod, pos);
        }

        GameEvents.OnSellerArrived(SellerType.Tormod);
    }

    private void RemoveTormod()
    {
        if (_tormodInstance == null) return;

        Destroy(_tormodInstance.gameObject);
        _tormodInstance = null;
        GameEvents.OnSellerLeft(SellerType.Tormod);
    }

    private void OnDeliveryMade(DeliveryType type, ItemDef item, int count, int price)
    {
        if (type != DeliveryType.Tormod || _tormodNailsGranted) return;
        _tormodNailsGranted = true;
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.TryAdd(ContentDb.Nails, 3);
            GameEvents.OnToastRequested("+3 Nails from Tormod");
        }
    }

    public void OpenSellMenu(SellerType type)
    {
        GameEvents.OnSellMenuRequested(type);
    }

    public void CloseSellMenu() { }

    public int GetBuyPrice(ItemDef item) => EconomyRules.GetBuyPrice(item);

    public bool ExecutePurchase(ItemDef item, int count)
    {
        if (InventoryManager.Instance == null || GameManager.Instance == null) return false;

        int cost = GetBuyPrice(item) * count;
        if (!GameManager.Instance.TrySpend(cost)) return false;

        InventoryManager.Instance.TryAdd(item, count);
        return true;
    }
}
