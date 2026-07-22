public sealed class EconomyState
{
    public int Cash { get; private set; }
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

    public int SetReputation(int value)
    {
        int old = Reputation;
        Reputation = value;
        return old;
    }
}
