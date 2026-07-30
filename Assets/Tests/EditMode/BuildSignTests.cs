using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class BuildSignTests
{
    private InventoryManager _inventory;
    private BuildSign _sign;
    private GameObject _homesteadGo;
    private Homestead _homestead;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();

        var signGo = TestBootstrap.CreateGameObject("TestSign");
        _sign = signGo.AddComponent<BuildSign>();

        _homesteadGo = TestBootstrap.CreateGameObject("TestHomestead");
        _homestead = _homesteadGo.AddComponent<Homestead>();
        _homesteadGo.SetActive(false);
        _sign.homestead = _homestead;
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Interact_NoStone_StaysAtSite()
    {
        _sign.Interact();

        Assert.AreEqual(BuildStage.Site, _sign.Stage);
    }

    [Test]
    public void Interact_WithStone_AdvancesToFoundation()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Foundation, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_WithWood_AdvancesToFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_WithWoodAndNails_AdvancesToWalls()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Walls, _sign.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));
    }

    [Test]
    public void Interact_CompleteBuild_SetsHomesteadBuilt()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _sign.Interact();

        Assert.IsTrue(_homestead.IsBuilt);
        Assert.IsTrue(_homesteadGo.activeSelf);
        Assert.IsFalse(_sign.gameObject.activeSelf);
    }

    [Test]
    public void Interact_FrameWithoutNails_StaysAtFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _sign.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _sign.Interact();

        Assert.AreEqual(BuildStage.Frame, _sign.Stage);
    }

    [Test]
    public void HomesteadBuildStageChanged_FiresOnAdvance()
    {
        int firedStage = -1;
        GameEvents.HomesteadBuildStageChanged += s => firedStage = s;

        _inventory.TryAdd(ContentDb.Stone, 3);
        _sign.Interact();

        Assert.AreEqual(1, firedStage);
    }
}
