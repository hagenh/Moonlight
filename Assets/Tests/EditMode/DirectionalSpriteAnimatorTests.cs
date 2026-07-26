using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;

public class DirectionalSpriteAnimatorTests
{
    private GameObject _go;
    private SpriteRenderer _sr;
    private DirectionalSpriteAnimator _animator;
    private DirectionalAnimationSet _set;
    private Sprite _spriteA;
    private Sprite _spriteB;
    private Sprite _spriteC;

    [SetUp]
    public void SetUp()
    {
        _go = TestBootstrap.CreateGameObject("AnimTest");
        _sr = _go.AddComponent<SpriteRenderer>();
        _animator = _go.AddComponent<DirectionalSpriteAnimator>();

        _spriteA = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        _spriteB = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        _spriteC = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

        _set = new DirectionalAnimationSet();

        var idle = new DirectionalClip
        {
            down = new Sprite[] { _spriteA },
            up = new Sprite[] { _spriteB },
            left = new Sprite[] { _spriteC },
            right = new Sprite[] { _spriteA },
            framesPerSecond = 2f,
            loop = true
        };
        _set.AddClip("idle", idle);

        var walk = new DirectionalClip
        {
            down = new Sprite[] { _spriteB, _spriteC, _spriteA },
            up = new Sprite[] { _spriteB, _spriteC, _spriteA },
            left = new Sprite[] { _spriteC, _spriteA, _spriteB },
            right = new Sprite[] { _spriteA, _spriteC, _spriteB },
            framesPerSecond = 4f,
            loop = true
        };
        _set.AddClip("walk", walk);

        _animator.animationSet = _set;
        _animator.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
    }

    [Test]
    public void Initialize_PlaysDefaultClip_FirstFrame()
    {
        Assert.AreEqual(_spriteA, _sr.sprite);
    }

    [Test]
    public void Play_SetsFirstFrameOfClip()
    {
        _animator.Play("walk");

        Assert.AreEqual(_spriteB, _sr.sprite);
    }

    [Test]
    public void SetFacing_SwapsToCorrectDirection()
    {
        _animator.SetFacing(FacingDirection.Up);

        Assert.AreEqual(_spriteB, _sr.sprite);
    }

    [Test]
    public void SetFacing_Left_SwapsCorrectly()
    {
        _animator.SetFacing(FacingDirection.Left);

        Assert.AreEqual(_spriteC, _sr.sprite);
    }

    [Test]
    public void SetFacingFromVector_Up_ReturnsUpFacing()
    {
        _animator.SetFacingFromVector(new Vector2(0, 1));

        Assert.AreEqual(_spriteB, _sr.sprite);
    }

    [Test]
    public void SetFacingFromVector_SmallMagnitude_DoesNotChangeFacing()
    {
        _animator.SetFacing(FacingDirection.Down);
        _animator.SetFacingFromVector(new Vector2(0.01f, 0.01f));

        Assert.AreEqual(_spriteA, _sr.sprite);
    }

    [Test]
    public void Tick_AdvancesFrame()
    {
        _animator.Play("walk");
        _animator.Tick(0.26f);

        Assert.AreEqual(_spriteC, _sr.sprite);
    }

    [Test]
    public void Tick_LoopsBackToFirstFrame()
    {
        _animator.Play("walk");
        _animator.Tick(0.26f);
        _animator.Tick(0.26f);
        _animator.Tick(0.26f);

        Assert.AreEqual(_spriteB, _sr.sprite);
    }

    [Test]
    public void Tick_IdleSingleFrame_StaysOnSameSprite()
    {
        _animator.Tick(1f);
        _animator.Tick(1f);

        Assert.AreEqual(_spriteA, _sr.sprite);
    }

    [Test]
    public void Stop_FreezesOnCurrentFrame()
    {
        _animator.Play("walk");
        _animator.Tick(0.26f);
        _animator.Stop();
        _animator.Tick(1f);

        Assert.AreEqual(_spriteC, _sr.sprite);
    }

    [Test]
    public void NonLoopingClip_ReturnsToDefaultWhenFinished()
    {
        var nod = new DirectionalClip
        {
            down = new Sprite[] { _spriteB, _spriteC },
            up = new Sprite[] { _spriteB },
            left = new Sprite[] { _spriteB },
            right = new Sprite[] { _spriteB },
            framesPerSecond = 4f,
            loop = false
        };
        _set.AddClip("nod", nod);

        _animator.Play("nod");
        _animator.Tick(0.26f);
        _animator.Tick(0.26f);

        Assert.AreEqual(_spriteA, _sr.sprite);
    }

    [Test]
    public void Play_NoAnimationSet_DoesNotThrow()
    {
        var go = TestBootstrap.CreateGameObject("BareAnim");
        go.AddComponent<SpriteRenderer>();
        var animator = go.AddComponent<DirectionalSpriteAnimator>();
        animator.Initialize();

        Assert.DoesNotThrow(() => animator.Play("walk"));
    }

    [Test]
    public void Play_SameClip_DoesNotResetFrame()
    {
        _animator.Play("walk");
        _animator.Tick(0.26f);
        int frameBefore = _animator.CurrentFrame;
        _animator.Play("walk");

        Assert.AreEqual(frameBefore, _animator.CurrentFrame);
    }
}
