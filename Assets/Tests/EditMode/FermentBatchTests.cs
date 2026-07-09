using NUnit.Framework;

public class FermentBatchTests
{
    private RecipeData _recipe;
    private float _currentTime;

    [SetUp]
    public void SetUp()
    {
        _recipe = new RecipeData("Test Mash", 4, 3, new ItemDef("out", "Output", false, 25, true))
            .AddIngredient(new ItemDef("grain", "Grain", true, 5), 2);
        _currentTime = 100f;
    }

    private FermentBatch CreateBatch()
    {
        return new FermentBatch(_recipe, () => _currentTime);
    }

    [Test]
    public void Progress_ZeroAtStart()
    {
        var batch = CreateBatch();
        Assert.AreEqual(0f, batch.Progress, 0.001f);
    }

    [Test]
    public void Progress_HalfwayAtHalfDuration()
    {
        var batch = CreateBatch();
        _currentTime = batch.StartGameMinutes + _recipe.fermentationHours * 60f * 0.5f;
        Assert.AreEqual(0.5f, batch.Progress, 0.001f);
    }

    [Test]
    public void Progress_ClampsToOneWhenOvertime()
    {
        var batch = CreateBatch();
        _currentTime = batch.StartGameMinutes + _recipe.fermentationHours * 60f * 2f;
        Assert.AreEqual(1f, batch.Progress, 0.001f);
    }

    [Test]
    public void IsComplete_TrueAtFullProgress()
    {
        var batch = CreateBatch();
        _currentTime = batch.StartGameMinutes + _recipe.fermentationHours * 60f;
        Assert.IsTrue(batch.IsComplete);
    }

    [Test]
    public void IsComplete_FalseBeforeCompletion()
    {
        var batch = CreateBatch();
        _currentTime = batch.StartGameMinutes + 10f;
        Assert.IsFalse(batch.IsComplete);
    }

    [Test]
    public void Progress_TotalFermentMinutesZero_ReturnsOne()
    {
        var zeroHourRecipe = new RecipeData("Instant", 0, 1, new ItemDef("out", "Out", false, 1));
        var batch = new FermentBatch(zeroHourRecipe, () => _currentTime);
        Assert.AreEqual(1f, batch.Progress);
        Assert.IsTrue(batch.IsComplete);
    }
}
