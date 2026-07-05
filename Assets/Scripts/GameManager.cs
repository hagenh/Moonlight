using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int startingCash = 500;
    public int Cash { get; private set; }
    public int Day { get; private set; } = 1;
    public int Heat { get; private set; }
    public int Reputation { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Cash = startingCash;
    }

    public bool TrySpend(int amount)
    {
        if (Cash < amount) return false;
        Cash -= amount;
        return true;
    }

    public void AddCash(int amount)
    {
        Cash += amount;
    }

    public void SetHeat(int value)
    {
        int old = Heat;
        Heat = Mathf.Max(0, value);
        if (Heat != old) GameEvents.OnHeatChanged(Heat, old);
    }

    public void SetReputation(int value)
    {
        int old = Reputation;
        Reputation = value;
        if (Reputation != old) GameEvents.OnRepChanged(Reputation, old);
    }

    public void AdvanceDay()
    {
        GameEvents.OnDayEnded(Day);
        Day++;
        GameEvents.OnToastRequested($"Day {Day} begins");
    }
}
