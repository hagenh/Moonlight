using Lamplight.TestSupport;
using NUnit.Framework;

public class RecipeBookEventTests
{
    [SetUp]
    public void SetUp() => GameEventsReset.ClearAll();

    [TearDown]
    public void TearDown() => GameEventsReset.ClearAll();

    [Test]
    public void OnRecipeBookRequested_NotifiesSubscribers()
    {
        int calls = 0;
        GameEvents.RecipeBookRequested += () => calls++;

        GameEvents.OnRecipeBookRequested();

        Assert.AreEqual(1, calls);
    }

    [Test]
    public void OnRecipeBookRequested_WithNoSubscribers_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameEvents.OnRecipeBookRequested());
    }

    [Test]
    public void ClearAll_RemovesRecipeBookSubscribers()
    {
        int calls = 0;
        GameEvents.RecipeBookRequested += () => calls++;

        GameEventsReset.ClearAll();
        GameEvents.OnRecipeBookRequested();

        Assert.AreEqual(0, calls);
    }
}
