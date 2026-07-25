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

        _guardGo = TestBootstrap.CreateGameObject("Guard");
        var sr = _guardGo.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        _animator = _guardGo.AddComponent<DirectionalSpriteAnimator>();
        _animator.animationSet = BuildTestAnimationSet();
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

    private static DirectionalAnimationSet BuildTestAnimationSet()
    {
        var set = new DirectionalAnimationSet();
        var tex = new Texture2D(4, 4);
        var pixels = new Color32[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

        var idle = new DirectionalClip
        {
            down = new Sprite[] { s },
            up = new Sprite[] { s },
            left = new Sprite[] { s },
            right = new Sprite[] { s },
            framesPerSecond = 2f,
            loop = true
        };
        set.AddClip("idle", idle);

        var walk = new DirectionalClip
        {
            down = new Sprite[] { s, s, s },
            up = new Sprite[] { s, s, s },
            left = new Sprite[] { s, s, s },
            right = new Sprite[] { s, s, s },
            framesPerSecond = 8f,
            loop = true
        };
        set.AddClip("walk", walk);

        return set;
    }
}
