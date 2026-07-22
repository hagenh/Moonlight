using NUnit.Framework;

public class EconomyStateTests
{
    private EconomyState _economy;

    [SetUp]
    public void SetUp()
    {
        _economy = new EconomyState(500);
    }

    [Test]
    public void TrySpend_InsufficientReturnsFalseAndKeepsCash()
    {
        bool result = _economy.TrySpend(501);
        Assert.IsFalse(result);
        Assert.AreEqual(500, _economy.Cash);
    }

    [Test]
    public void TrySpend_Exact_Succeeds()
    {
        bool result = _economy.TrySpend(500);
        Assert.IsTrue(result);
        Assert.AreEqual(0, _economy.Cash);
    }

    [Test]
    public void TrySpend_Over_Succeeds()
    {
        bool result = _economy.TrySpend(100);
        Assert.IsTrue(result);
        Assert.AreEqual(400, _economy.Cash);
    }

    [Test]
    public void AddCash_Stacks()
    {
        _economy.AddCash(50);
        _economy.AddCash(25);
        Assert.AreEqual(575, _economy.Cash);
    }

    [Test]
    public void SetReputation_ReturnsOld()
    {
        _economy.SetReputation(10);
        int old = _economy.SetReputation(20);
        Assert.AreEqual(10, old);
    }
}
