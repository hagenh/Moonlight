using Lamplight.TestSupport;
using NUnit.Framework;

public class GameEventsTests
{
    [TearDown]
    public void TearDown() => GameEventsReset.ClearAll();

    [Test]
    public void Subscribe_Fires()
    {
        int fired = 0;
        GameEvents.CashChanged += (cash) => fired++;

        GameEvents.OnCashChanged(100);

        Assert.AreEqual(1, fired);
    }

    [Test]
    public void Unsubscribe_StopsFiring()
    {
        int fired = 0;
        System.Action<int> handler = (cash) => fired++;

        GameEvents.CashChanged += handler;
        GameEvents.OnCashChanged(100);
        Assert.AreEqual(1, fired);

        GameEvents.CashChanged -= handler;
        GameEvents.OnCashChanged(200);
        Assert.AreEqual(1, fired);
    }

    [Test]
    public void ClearAll_RemovesAllSubscriptions()
    {
        int fired = 0;
        GameEvents.CashChanged += (cash) => fired++;
        GameEvents.DayEnded += (day) => fired++;

        GameEventsReset.ClearAll();

        GameEvents.OnCashChanged(100);
        GameEvents.OnDayEnded(5);

        Assert.AreEqual(0, fired);
    }

    [Test]
    public void MultipleSubscribers_AllFire()
    {
        int a = 0, b = 0;
        GameEvents.HeatChanged += (newHeat, oldHeat) => a++;
        GameEvents.HeatChanged += (newHeat, oldHeat) => b++;

        GameEvents.OnHeatChanged(50, 30);

        Assert.AreEqual(1, a);
        Assert.AreEqual(1, b);
    }
}
