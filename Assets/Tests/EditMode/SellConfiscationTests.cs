using Lamplight.TestSupport.Fakes;
using NUnit.Framework;

public class SellConfiscationTests
{
    [Test]
    public void HeatAtOrBelow50_NeverConfiscates_RegardlessOfRng()
    {
        var rng = new StubRng(0.01f, 0.0f, 0.001f);
        Assert.IsFalse(EconomyRules.ShouldConfiscate(0, rng));
        Assert.IsFalse(EconomyRules.ShouldConfiscate(25, rng));
        Assert.IsFalse(EconomyRules.ShouldConfiscate(50, rng));
    }

    [Test]
    public void HeatAbove50_RngBelowThreshold_Confiscates()
    {
        var rng = new StubRng(0.05f);
        Assert.IsTrue(EconomyRules.ShouldConfiscate(51, rng));
    }

    [Test]
    public void HeatAbove50_RngAtOrAboveThreshold_DoesNotConfiscate()
    {
        var rng = new StubRng(0.1f);
        Assert.IsFalse(EconomyRules.ShouldConfiscate(80, rng));

        var rng2 = new StubRng(0.99f);
        Assert.IsFalse(EconomyRules.ShouldConfiscate(100, rng2));
    }

    [Test]
    public void ShouldConfiscate_Boundary_HeatExactly50_False()
    {
        var rng = new StubRng(0.0f);
        Assert.IsFalse(EconomyRules.ShouldConfiscate(50, rng));
    }

    [Test]
    public void ShouldConfiscate_Boundary_HeatExactly51_CanConfiscate()
    {
        var rng = new StubRng(0.05f);
        Assert.IsTrue(EconomyRules.ShouldConfiscate(51, rng));
    }
}
