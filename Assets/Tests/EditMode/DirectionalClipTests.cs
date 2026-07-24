using NUnit.Framework;
using UnityEngine;

public class DirectionalClipTests
{
    [Test]
    public void GetSprites_Down_ReturnsDownArray()
    {
        var clip = new DirectionalClip();
        var sprites = new Sprite[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f) };
        clip.down = sprites;

        Assert.AreEqual(sprites, clip.GetSprites(FacingDirection.Down));
    }

    [Test]
    public void GetSprites_Up_ReturnsUpArray()
    {
        var clip = new DirectionalClip();
        var sprites = new Sprite[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f) };
        clip.up = sprites;

        Assert.AreEqual(sprites, clip.GetSprites(FacingDirection.Up));
    }

    [Test]
    public void GetSprites_Left_ReturnsLeftArray()
    {
        var clip = new DirectionalClip();
        var sprites = new Sprite[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f) };
        clip.left = sprites;

        Assert.AreEqual(sprites, clip.GetSprites(FacingDirection.Left));
    }

    [Test]
    public void GetSprites_Right_ReturnsRightArray()
    {
        var clip = new DirectionalClip();
        var sprites = new Sprite[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f) };
        clip.right = sprites;

        Assert.AreEqual(sprites, clip.GetSprites(FacingDirection.Right));
    }

    [Test]
    public void Defaults_FpsIsEightAndLoopIsTrue()
    {
        var clip = new DirectionalClip();

        Assert.AreEqual(8f, clip.framesPerSecond);
        Assert.IsTrue(clip.loop);
    }
}
