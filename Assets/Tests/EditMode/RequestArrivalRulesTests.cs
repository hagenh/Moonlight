using System.Collections.Generic;
using Lamplight.TestSupport.Fakes;
using NUnit.Framework;

public class RequestArrivalRulesTests
{
    private static readonly ItemDef Shine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);
    private static readonly ItemDef Sweet = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);

    private static List<RecipeData> TwoRecipes() => new()
    {
        new RecipeData("Berry Shine", 3, 2, Shine),
        new RecipeData("Sweet Batch", 6, 4, Sweet)
    };

    [Test]
    public void NotesPerNight_ThreeSlots_ReturnsTwo()
    {
        Assert.AreEqual(2, RequestArrivalRules.NotesPerNight(3));
    }

    [Test]
    public void NotesPerNight_FiveSlots_ReturnsThree()
    {
        Assert.AreEqual(3, RequestArrivalRules.NotesPerNight(5));
    }

    [Test]
    public void NotesPerNight_EightSlots_ReturnsThree()
    {
        Assert.AreEqual(3, RequestArrivalRules.NotesPerNight(8));
    }

    [Test]
    public void Generate_NoRecipes_ReturnsNull()
    {
        Assert.IsNull(RequestArrivalRules.Generate(new List<RecipeData>(), new SeededRng(1), "r1"));
    }

    [Test]
    public void Generate_NullRecipes_ReturnsNull()
    {
        Assert.IsNull(RequestArrivalRules.Generate(null, new SeededRng(1), "r1"));
    }

    [Test]
    public void Generate_UnitsAreAWholeNumberOfBatches()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");

            int outputCount = request.Accepts[0] == Shine ? 2 : 4;
            Assert.AreEqual(0, request.Units % outputCount,
                $"seed {seed}: {request.Units} units is not a whole number of {outputCount}-jar batches");

            int batches = request.Units / outputCount;
            Assert.GreaterOrEqual(batches, RequestArrivalRules.MinBatches);
            Assert.LessOrEqual(batches, RequestArrivalRules.MaxBatches);
        }
    }

    [Test]
    public void Generate_ExactRequest_AcceptsExactlyOneProduct()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");
            if (request.Kind != RequestKind.Exact) continue;

            Assert.AreEqual(1, request.Accepts.Count);
        }
    }

    [Test]
    public void Generate_DescriptiveRequest_AcceptsMoreThanOneProduct()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");
            if (request.Kind != RequestKind.Descriptive) continue;

            Assert.Greater(request.Accepts.Count, 1);
        }
    }

    [Test]
    public void Generate_SingleRecipe_NeverProducesADescriptiveRequest()
    {
        var one = new List<RecipeData> { new RecipeData("Berry Shine", 3, 2, Shine) };

        for (int seed = 0; seed < 40; seed++)
        {
            var request = RequestArrivalRules.Generate(one, new SeededRng(seed), $"r{seed}");

            Assert.AreEqual(RequestKind.Exact, request.Kind);
        }
    }

    [Test]
    public void Generate_UsesTheGivenId()
    {
        var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(1), "day3-note0");

        Assert.AreEqual("day3-note0", request.Id);
    }

    [Test]
    public void Generate_AlwaysCarriesSignatureAndText()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            var request = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(seed), $"r{seed}");

            Assert.IsNotEmpty(request.Signature);
            Assert.IsNotEmpty(request.Text);
        }
    }

    [Test]
    public void Generate_IsDeterministicForASeed()
    {
        var a = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(7), "r");
        var b = RequestArrivalRules.Generate(TwoRecipes(), new SeededRng(7), "r");

        Assert.AreEqual(a.Kind, b.Kind);
        Assert.AreEqual(a.Units, b.Units);
        Assert.AreEqual(a.Signature, b.Signature);
    }
}
