using System.Collections;
using System.Linq;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine.TestTools;

public class RecipeDiscoveryTests
{
    private FermentManager _fermentManager;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        TestBootstrap.CreateSingleton<GameManager>();
        TestBootstrap.CreateSingleton<BuildingManager>();
        _fermentManager = TestBootstrap.CreateSingleton<FermentManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator BerryShine_IsDiscoveredFromStart()
    {
        var recipe = _fermentManager.Recipes.First(r => r.recipeName == "Berry Shine");
        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(recipe));
        yield return null;
    }

    [UnityTest]
    public IEnumerator OtherRecipes_NotDiscovered_UntilTriggered()
    {
        var highlandMash = _fermentManager.Recipes.First(r => r.recipeName == "Highland Mash");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(highlandMash));
        yield return null;
    }

    [UnityTest]
    public IEnumerator RecipeDiscoveredEvent_AddsToDiscoveredSet()
    {
        var highlandMash = _fermentManager.Recipes.First(r => r.recipeName == "Highland Mash");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(highlandMash));

        GameEvents.OnRecipeDiscovered("Highland Mash");

        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(highlandMash));
        yield return null;
    }

    [UnityTest]
    public IEnumerator BuildingRestored_AutoDiscoversRecipesGatedOnIt()
    {
        var sweetBatch = _fermentManager.Recipes.First(r => r.recipeName == "Sweet Batch");
        Assert.IsFalse(_fermentManager.IsRecipeDiscovered(sweetBatch));

        var buildingGo = TestBootstrap.CreateGameObject("TestBuilding");
        var building = buildingGo.AddComponent<Building>();

        GameEvents.OnBuildingStateChanged(building, BuildingState.Cleared, BuildingState.Restored);

        Assert.IsTrue(_fermentManager.IsRecipeDiscovered(sweetBatch));
        yield return null;
    }
}
