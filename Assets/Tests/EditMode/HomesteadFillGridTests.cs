using NUnit.Framework;
using Lamplight.TestSupport;
using UnityEngine;

public class HomesteadFillGridTests
{
    private HomesteadFillGrid _grid;

    [SetUp]
    public void SetUp()
    {
        var go = TestBootstrap.CreateGameObject("TestFillGrid");
        _grid = go.AddComponent<HomesteadFillGrid>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [Test]
    public void Awake_CreatesTwentyCells()
    {
        Assert.AreEqual(20, _grid.CellCount);
    }

    [Test]
    public void SetFilled_EnablesExactlyThatManyCells()
    {
        var tex = new Texture2D(4, 4);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

        _grid.SetFilled(5, sprite);

        int enabledCount = 0;
        foreach (var sr in _grid.GetComponentsInChildren<SpriteRenderer>())
            if (sr.enabled) enabledCount++;

        Assert.AreEqual(5, enabledCount);
    }

    [Test]
    public void SetFilled_ZeroFilled_AllCellsDisabled()
    {
        var tex = new Texture2D(4, 4);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

        _grid.SetFilled(0, sprite);

        foreach (var sr in _grid.GetComponentsInChildren<SpriteRenderer>())
            Assert.IsFalse(sr.enabled);
    }

    [Test]
    public void SetFilled_UsesGivenSprite()
    {
        var tex = new Texture2D(4, 4);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

        _grid.SetFilled(1, sprite);

        var firstEnabled = _grid.GetComponentsInChildren<SpriteRenderer>()[0];
        Assert.AreEqual(sprite, firstEnabled.sprite);
    }
}
