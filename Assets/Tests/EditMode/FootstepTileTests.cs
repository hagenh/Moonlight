using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FootstepTileTests
{
    [Test]
    public void Surface_DefaultsToDirt()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        Assert.AreEqual(FootstepSurface.Dirt, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToSand()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Sand;
        Assert.AreEqual(FootstepSurface.Sand, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToStone()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Stone;
        Assert.AreEqual(FootstepSurface.Stone, tile.Surface);
    }

    [Test]
    public void Surface_CanBeSetToWater()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        tile.Surface = FootstepSurface.Water;
        Assert.AreEqual(FootstepSurface.Water, tile.Surface);
    }

    [Test]
    public void GetTileData_SetsSprite()
    {
        var tile = ScriptableObject.CreateInstance<FootstepTile>();
        var tex = new Texture2D(1, 1);
        tile.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        var tileData = new TileData();
        tile.GetTileData(Vector3Int.zero, null, ref tileData);
        Assert.IsNotNull(tileData.sprite);
        Assert.AreEqual(tile.sprite, tileData.sprite);
    }

    [TearDown]
    public void TearDown()
    {
        var tiles = Resources.FindObjectsOfTypeAll<FootstepTile>();
        foreach (var t in tiles)
            Object.DestroyImmediate(t);
    }
}
