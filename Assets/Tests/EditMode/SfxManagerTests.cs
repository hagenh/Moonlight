using NUnit.Framework;
using Lamplight.TestSupport;
using Lamplight.TestSupport.Fakes;
using UnityEngine;

public class SfxManagerTests
{
    private SfxManager _sfx;

    [SetUp]
    public void SetUp()
    {
        var go = TestBootstrap.CreateGameObject("TestSfx");
        _sfx = go.AddComponent<SfxManager>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [Test]
    public void PickClip_NullArray_ReturnsNull()
    {
        Assert.IsNull(_sfx.PickClip(null));
    }

    [Test]
    public void PickClip_EmptyArray_ReturnsNull()
    {
        Assert.IsNull(_sfx.PickClip(new AudioClip[0]));
    }

    [Test]
    public void PickClip_UsesRngToSelectElement()
    {
        var clips = new[]
        {
            AudioClip.Create("a", 1, 1, 44100, false),
            AudioClip.Create("b", 1, 1, 44100, false),
            AudioClip.Create("c", 1, 1, 44100, false)
        };
        _sfx.SetRng(new SeededRng(1));
        int expectedIndex = new SeededRng(1).Range(0, clips.Length);

        var picked = _sfx.PickClip(clips);

        Assert.AreSame(clips[expectedIndex], picked);
    }
}
