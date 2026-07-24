using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;

public class GuardAnimationTests
{
    private GameObject _guardGo;
    private Guard _guard;
    private DirectionalSpriteAnimator _animator;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        var dbGo = TestBootstrap.CreateSingleton<ContentDb>();

        _guardGo = TestBootstrap.CreateGameObject("Guard");
        var sr = _guardGo.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        _animator = _guardGo.AddComponent<DirectionalSpriteAnimator>();
        _animator.animationSet = dbGo.GetComponent<ContentDb>().GuardAnimations;
        _animator.Initialize();

        var col = _guardGo.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        _guard = _guardGo.AddComponent<Guard>();

        var wp1 = TestBootstrap.CreateGameObject("WP1");
        wp1.transform.position = new Vector3(2, 0, 0);
        var wp2 = TestBootstrap.CreateGameObject("WP2");
        wp2.transform.position = new Vector3(0, 2, 0);
        _guard.SetWaypoints(new Transform[] { wp1.transform, wp2.transform });
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [Test]
    public void Guard_HasAnimatorComponent()
    {
        Assert.IsNotNull(_guardGo.GetComponent<DirectionalSpriteAnimator>());
    }

    [Test]
    public void Guard_AnimationSetIsAssigned()
    {
        Assert.IsNotNull(_animator.animationSet);
    }
}
