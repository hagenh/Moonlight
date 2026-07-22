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
    private DeliveryPoint _tormodDeliveryPoint;

    private IRng _rng = UnityRng.Instance;

    public bool IsCartInTown => _cartInstance != null;
    public bool IsTormodInTown => _tormodInstance != null;

    internal void SetRng(IRng rng) => _rng = rng;

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
        RemoveCart();
        RemoveTormod();
    }

    private void OnHourChanged(int hour, int day)
    {
        if (hour == cartArriveHour && EconomyRules.IsCartDay(day) && _cartInstance == null)
            SpawnCart();

        if (hour == cartLeaveHour && _cartInstance != null)
            RemoveCart();

        if (hour == tormodArriveHour && _tormodInstance == null)
            SpawnTormod();

        if (hour == tormodLeaveHour && _tormodInstance != null)
            RemoveTormod();
    }

    private void SpawnCart()
    {
        Vector3 pos = cartPosition != null ? cartPosition.position : Vector3.zero;
        _cartInstance = SellerInteractable.Create(SellerType.TravelingCart, pos);

        var dpGo = new GameObject("CartDeliveryPoint");
        dpGo.transform.position = pos;
        dpGo.layer = LayerMask.NameToLayer("Interactable");
        var col = dpGo.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1.5f);
        var dp = dpGo.AddComponent<DeliveryPoint>();
        dp.SetDeliveryType(DeliveryType.Cart);

        GameEvents.OnSellerArrived(SellerType.TravelingCart);
    }

    private void RemoveCart()
    {
        if (_cartInstance != null)
        {
            Destroy(_cartInstance.gameObject);
            _cartInstance = null;
            GameEvents.OnSellerLeft(SellerType.TravelingCart);
        }
    }

    private void SpawnTormod()
    {
        Vector3 pos = tormodPosition != null ? tormodPosition.position : Vector3.zero;
        _tormodInstance = SellerInteractable.Create(SellerType.Tormod, pos);

        var dpGo = new GameObject("TormodDeliveryPoint");
        dpGo.transform.position = pos;
        dpGo.layer = LayerMask.NameToLayer("Interactable");
        var col = dpGo.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1.5f);
        _tormodDeliveryPoint = dpGo.AddComponent<DeliveryPoint>();
        _tormodDeliveryPoint.SetDeliveryType(DeliveryType.Tormod);

        GameEvents.OnSellerArrived(SellerType.Tormod);
    }

    private void RemoveTormod()
    {
        if (_tormodInstance != null)
        {
            Destroy(_tormodInstance.gameObject);
            _tormodInstance = null;
        }
        if (_tormodDeliveryPoint != null)
        {
            Destroy(_tormodDeliveryPoint.gameObject);
            _tormodDeliveryPoint = null;
        }
        if (_tormodInstance == null && _tormodDeliveryPoint == null)
            GameEvents.OnSellerLeft(SellerType.Tormod);
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
