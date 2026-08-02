using NUnit.Framework;
using UnityEngine;
using Lamplight.TestSupport;

public class FootstepPlayerTests
{
    private GameObject playerGo;
    private FootstepPlayer footstepPlayer;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        playerGo = TestBootstrap.CreateGameObject("Player");
        playerGo.AddComponent<Rigidbody2D>();
        footstepPlayer = playerGo.AddComponent<FootstepPlayer>();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void GetClipsForSurface_Dirt_ReturnsDirtClips()
    {
        var dirtClips = new AudioClip[2];
        footstepPlayer.dirtClips = dirtClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Dirt);
        Assert.AreSame(dirtClips, result);
    }

    [Test]
    public void GetClipsForSurface_Sand_ReturnsSandClips()
    {
        var sandClips = new AudioClip[2];
        footstepPlayer.sandClips = sandClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Sand);
        Assert.AreSame(sandClips, result);
    }

    [Test]
    public void GetClipsForSurface_Stone_ReturnsStoneClips()
    {
        var stoneClips = new AudioClip[2];
        footstepPlayer.stoneClips = stoneClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Stone);
        Assert.AreSame(stoneClips, result);
    }

    [Test]
    public void GetClipsForSurface_Water_ReturnsWaterClips()
    {
        var waterClips = new AudioClip[2];
        footstepPlayer.waterClips = waterClips;
        var result = footstepPlayer.GetClipsForSurface(FootstepSurface.Water);
        Assert.AreSame(waterClips, result);
    }

    [Test]
    public void CurrentSurface_DefaultsToDirt()
    {
        Assert.AreEqual(FootstepSurface.Dirt, footstepPlayer.CurrentSurface);
    }
}
