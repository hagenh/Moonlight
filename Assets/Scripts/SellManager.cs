using UnityEngine;

public class SellManager : MonoBehaviour
{
    public static SellManager Instance { get; private set; }

    [SerializeField] private Transform cartPosition;
    [SerializeField] private int cartArriveHour = 10;
    [SerializeField] private int cartLeaveHour = 18;

    private SellerInteractable _cartInstance;

    private IRng _rng = UnityRng.Instance;

    public bool IsCartInTown => _cartInstance != null;

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
    }

    private void OnHourChanged(int hour, int day)
    {
        if (hour == cartArriveHour && EconomyRules.IsCartDay(day) && _cartInstance == null)
            SpawnCart();

        if (hour == cartLeaveHour && _cartInstance != null)
            RemoveCart();
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
