using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class HomesteadTests
{
    private Homestead _homestead;
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
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
    public void SetBuilt_SetsIsBuiltToTrue()
    {
        _homestead.SetBuilt();
        Assert.IsTrue(_homestead.IsBuilt);
    }

    [Test]
    public void SetBuilt_SwapsSpriteToBuiltSprite()
    {
        var builtTex = new Texture2D(16, 16);
        builtTex.SetPixel(0, 0, Color.red);
        builtTex.Apply();
        var builtSprite = Sprite.Create(builtTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);

        _homestead.SetBuiltSpriteForTest(builtSprite);
        _homestead.SetBuilt();

        Assert.AreEqual(builtSprite, _go.GetComponent<SpriteRenderer>().sprite);
    }

    [Test]
    public void InteractType_IsBuilding()
    {
        Assert.AreEqual(InteractType.Building, _homestead.InteractType);
    }

    [Test]
    public void Interact_WhenBuilt_DoesNotThrow()
    {
        _homestead.SetBuilt();
        Assert.DoesNotThrow(() => _homestead.Interact());
    }
}
