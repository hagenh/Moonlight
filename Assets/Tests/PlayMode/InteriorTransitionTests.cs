using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class InteriorTransitionTests
{
    private InteriorManager _interiorManager;
    private PlayerController _player;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameEventsReset.ClearAll();

        InteriorManager.SkipDefaultBuild = true;
        _interiorManager = TestBootstrap.CreateSingleton<InteriorManager>();

        _player = TestBootstrap.CreateSingleton<PlayerController>();
        _player.RB.gravityScale = 0;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
        InteriorManager.SkipDefaultBuild = false;

        yield return null;
    }

    [UnityTest]
    public IEnumerator EnterInterior_TeleportsPlayerToSpawn_AndSetsIsInside()
    {
        Vector2 originalPos = _player.RB.position;
        Vector2 spawn = new Vector2(200f, -2f);

        _interiorManager.EnterInterior(spawn);

        yield return new WaitForSeconds(1f);

        Assert.IsTrue(_interiorManager.IsInside, "IsInside should be true after entering");
        Assert.AreEqual(spawn.x, _player.RB.position.x, 0.01f, "Player X should match spawn");
        Assert.AreEqual(spawn.y, _player.RB.position.y, 0.01f, "Player Y should match spawn");
    }

    [UnityTest]
    public IEnumerator ExitInterior_RestoresPriorPosition()
    {
        Vector2 originalPos = new Vector2(-5f, 3f);
        _player.RB.position = originalPos;

        _interiorManager.EnterInterior(new Vector2(200f, -2f));
        yield return new WaitForSeconds(1f);

        Assert.IsTrue(_interiorManager.IsInside);

        _interiorManager.ExitInterior();
        yield return new WaitForSeconds(1f);

        Assert.IsFalse(_interiorManager.IsInside, "IsInside should be false after exiting");
        Assert.AreEqual(originalPos.x, _player.RB.position.x, 0.01f, "Player X should be restored");
        Assert.AreEqual(originalPos.y, _player.RB.position.y, 0.01f, "Player Y should be restored");
    }
}
