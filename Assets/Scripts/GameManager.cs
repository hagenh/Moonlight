using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int startingCash = 500;
    public int Cash => Economy.Cash;
    public int Heat => Economy.Heat;
    public int Reputation => Economy.Reputation;

    private EconomyState _economy;

    private EconomyState Economy
    {
        get
        {
            if (_economy == null)
                _economy = new EconomyState(startingCash);
            return _economy;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _economy = new EconomyState(startingCash);
    }

    public bool TrySpend(int amount)
    {
        if (!Economy.TrySpend(amount)) return false;
        GameEvents.OnCashChanged(Cash);
        return true;
    }

    public void AddCash(int amount)
    {
        Economy.AddCash(amount);
        GameEvents.OnCashChanged(Cash);
    }

    public void AddHeat(int delta) => SetHeat(Heat + delta);

    public void SetHeat(int value)
    {
        int old = Economy.SetHeat(value);
        if (Heat != old) GameEvents.OnHeatChanged(Heat, old);
    }

    public void SetReputation(int value)
    {
        int old = Economy.SetReputation(value);
        if (Reputation != old) GameEvents.OnRepChanged(Reputation, old);
    }
}
