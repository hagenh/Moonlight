using NUnit.Framework;
using Lamplight.TestSupport;
using Lamplight.TestSupport.Fakes;

public class SmokeTest
{
    [TearDown]
    public void TearDown() => GameEventsReset.ClearAll();

    [Test]
    public void FrameworkWorks()
    {
        Assert.Pass();
    }

    [Test]
    public void GameEventsClearAll_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameEventsReset.ClearAll());
    }

    [Test]
    public void StubRng_ReturnsQueuedValues()
    {
        var rng = new StubRng(0.5f, 0.9f);
        Assert.AreEqual(0.5f, rng.Value01());
        Assert.AreEqual(0.9f, rng.Value01());
    }

    [Test]
    public void SeededRng_IsDeterministic()
    {
        var a = new SeededRng(42);
        var b = new SeededRng(42);
        Assert.AreEqual(a.Value01(), b.Value01());
        Assert.AreEqual(a.Value01(), b.Value01());
    }
}
