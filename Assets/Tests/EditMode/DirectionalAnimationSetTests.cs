using NUnit.Framework;
using UnityEngine;

public class DirectionalAnimationSetTests
{
    [Test]
    public void AddClip_ThenGetClip_ReturnsSameClip()
    {
        var set = new DirectionalAnimationSet();
        var clip = new DirectionalClip();
        clip.down = new Sprite[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f) };

        set.AddClip("walk", clip);
        var result = set.GetClip("walk");

        Assert.AreEqual(clip, result);
    }

    [Test]
    public void GetClip_UnknownName_ReturnsNull()
    {
        var set = new DirectionalAnimationSet();

        Assert.IsNull(set.GetClip("nonexistent"));
    }

    [Test]
    public void DefaultClip_IsIdle()
    {
        var set = new DirectionalAnimationSet();

        Assert.AreEqual("idle", set.defaultClip);
    }

    [Test]
    public void DefaultClip_CanBeOverridden()
    {
        var set = new DirectionalAnimationSet { defaultClip = "walk" };

        Assert.AreEqual("walk", set.defaultClip);
    }
}
