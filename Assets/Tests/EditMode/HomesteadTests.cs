using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class HomesteadTests
{
    private Homestead _homestead;
    private GameObject _go;
    private InventoryManager _inventory;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _inventory = TestBootstrap.CreateSingleton<InventoryManager>();
        _go = TestBootstrap.CreateGameObject("TestHomestead");
        var sr = _go.AddComponent<SpriteRenderer>();
        _homestead = _go.AddComponent<Homestead>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void IsBuilt_DefaultsToFalse()
    {
        Assert.IsFalse(_homestead.IsBuilt);
    }

    [Test]
    public void SetBuilt_SwapsSpriteToBuiltSprite()
    {
        var builtTex = new Texture2D(16, 16);
        builtTex.SetPixel(0, 0, Color.red);
        builtTex.Apply();
        var builtSprite = Sprite.Create(builtTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);

        _homestead.SetBuiltSpriteForTest(builtSprite);
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _homestead.Interact();

        Assert.AreEqual(builtSprite, _go.GetComponent<SpriteRenderer>().sprite);
    }

    [Test]
    public void InteractType_IsBuilding()
    {
        Assert.AreEqual(InteractType.Building, _homestead.InteractType);
    }

    [Test]
    public void Interact_NoStone_StaysAtSite()
    {
        _homestead.Interact();
        Assert.AreEqual(BuildStage.Site, _homestead.Stage);
    }

    [Test]
    public void Interact_WithStone_AdvancesToFoundation()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        Assert.AreEqual(BuildStage.Foundation, _homestead.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Stone));
    }

    [Test]
    public void Interact_WithWood_AdvancesToFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _homestead.Interact();
        Assert.AreEqual(BuildStage.Frame, _homestead.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
    }

    [Test]
    public void Interact_WithWoodAndNails_AdvancesToWalls()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _homestead.Interact();
        Assert.AreEqual(BuildStage.Walls, _homestead.Stage);
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Wood));
        Assert.AreEqual(0, _inventory.GetCount(ContentDb.Nails));
    }

    [Test]
    public void Interact_FrameWithoutNails_StaysAtFrame()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _homestead.Interact();
        Assert.AreEqual(BuildStage.Frame, _homestead.Stage);
    }

    [Test]
    public void Interact_CompleteBuild_SetsIsBuilt()
    {
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 3);
        _homestead.Interact();
        _inventory.TryAdd(ContentDb.Wood, 2);
        _inventory.TryAdd(ContentDb.Nails, 3);
        _homestead.Interact();
        Assert.IsTrue(_homestead.IsBuilt);
    }

    [Test]
    public void HomesteadBuildStageChanged_FiresOnAdvance()
    {
        int firedStage = -1;
        GameEvents.HomesteadBuildStageChanged += s => firedStage = s;
        _inventory.TryAdd(ContentDb.Stone, 3);
        _homestead.Interact();
        Assert.AreEqual(1, firedStage);
    }

    [Test]
    public void CanInteract_IsAlwaysTrue()
    {
        Assert.IsTrue(_homestead.CanInteract);
    }
}
