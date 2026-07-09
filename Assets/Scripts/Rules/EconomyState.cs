using System;

public sealed class EconomyState
{
    public int Cash { get; private set; }
    public int Heat { get; private set; }
    public int Reputation { get; private set; }

    public EconomyState(int startingCash)
    {
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

    public int SetHeat(int value)
    {
        int old = Heat;
        Heat = Math.Max(0, value);
        return old;
    }

    public int AddHeat(int delta)
    {
        return SetHeat(Heat + delta);
    }

    public int SetReputation(int value)
    {
        int old = Reputation;
        Reputation = value;
        return old;
    }
}
