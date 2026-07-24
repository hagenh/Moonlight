# Directional Sprite Animation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a reusable directional sprite animation system that swaps SpriteRenderer frames on a timer, with idle and walk clips in each cardinal direction, and wire it into the guard patrol system.

**Architecture:** Three new files — `DirectionalClip` (data struct for per-direction sprite arrays), `DirectionalAnimationSet` (named clip collection), `DirectionalSpriteAnimator` (MonoBehaviour that drives frame swapping). Guard.cs and GuardManager.cs get minor modifications to use the animator. ContentDb.cs gets a guard animation set.

**Tech Stack:** Unity 6, C#, existing FacingDirection enum and FacingMath, NUnit for EditMode tests

## Global Constraints

- No Unity Animator Controller or `.anim`/`.controller` assets — code-driven sprite swap only
- No ScriptableObjects — all data is code-constructed
- No frameworks — hand-rolled
- No `UnityEngine` types in Rules/ classes (not applicable here but noted)
- No comments in code unless explicitly requested
- Reuse existing `FacingDirection` enum and `FacingMath`
- UI is IMGUI — not relevant to this feature
- File placement follows AGENTS.md conventions

---

### Task 1: DirectionalClip data class

**Files:**
- Create: `Assets/Scripts/DirectionalClip.cs`
- Test: `Assets/Tests/EditMode/DirectionalClipTests.cs`

**Interfaces:**
- Consumes: `FacingDirection` enum from `Assets/Scripts/Player/Enums/FacingDirection.cs`
- Produces: `DirectionalClip` class with `down`, `up`, `left`, `right` (Sprite[]), `framesPerSecond` (float), `loop` (bool), and `GetSprites(FacingDirection)` method

- [ ] **Step 1: Write the failing test**

```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Run via Unity MCP `run_tests` with assembly `Lamplight.EditModeTests` and filter `DirectionalClipTests`, or via command line:
```
Unity.exe -runTests -testPlatform EditMode -testFilter DirectionalClipTests -projectPath .
```
Expected: FAIL — `DirectionalClip` does not exist

- [ ] **Step 3: Write minimal implementation**

```csharp
using UnityEngine;

public class DirectionalClip
{
    public Sprite[] down;
    public Sprite[] up;
    public Sprite[] left;
    public Sprite[] right;
    public float framesPerSecond = 8f;
    public bool loop = true;

    public Sprite[] GetSprites(FacingDirection facing)
    {
        return facing switch
        {
            FacingDirection.Down => down,
            FacingDirection.Up => up,
            FacingDirection.Left => left,
            FacingDirection.Right => right,
            _ => down
        };
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/DirectionalClip.cs Assets/Tests/EditMode/DirectionalClipTests.cs
git commit -m "Add DirectionalClip data class with per-direction sprite arrays"
```

---

### Task 2: DirectionalAnimationSet data class

**Files:**
- Create: `Assets/Scripts/DirectionalAnimationSet.cs`
- Test: `Assets/Tests/EditMode/DirectionalAnimationSetTests.cs`

**Interfaces:**
- Consumes: `DirectionalClip` from Task 1
- Produces: `DirectionalAnimationSet` class with `clips` (Dictionary\<string, DirectionalClip\>), `defaultClip` (string), `GetClip(string name)`, `AddClip(string name, DirectionalClip clip)`

- [ ] **Step 1: Write the failing test**

```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Run via Unity MCP or command line with filter `DirectionalAnimationSetTests`
Expected: FAIL — `DirectionalAnimationSet` does not exist

- [ ] **Step 3: Write minimal implementation**

```csharp
using System.Collections.Generic;

public class DirectionalAnimationSet
{
    public Dictionary<string, DirectionalClip> clips = new();
    public string defaultClip = "idle";

    public DirectionalClip GetClip(string name)
    {
        return clips.GetValueOrDefault(name);
    }

    public void AddClip(string name, DirectionalClip clip)
    {
        clips[name] = clip;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/DirectionalAnimationSet.cs Assets/Tests/EditMode/DirectionalAnimationSetTests.cs
git commit -m "Add DirectionalAnimationSet named clip collection"
```

---

### Task 3: DirectionalSpriteAnimator component

**Files:**
- Create: `Assets/Scripts/DirectionalSpriteAnimator.cs`
- Test: `Assets/Tests/EditMode/DirectionalSpriteAnimatorTests.cs`

**Interfaces:**
- Consumes: `DirectionalAnimationSet` from Task 2, `FacingDirection` enum, `FacingMath`
- Produces: `DirectionalSpriteAnimator : MonoBehaviour` with `Play(string clipName)`, `SetFacing(FacingDirection)`, `SetFacingFromVector(Vector2)`, `Stop()`, and `animationSet` field

- [ ] **Step 1: Write the failing tests**

```csharp
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
            down = new Sprite[] { _spriteA, _spriteB, _spriteC },
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
    public void Play_SameClip_DoesNotResetFrame()
    {
        _animator.Play("walk");
        _animator.Tick(0.26f);
        int frameBefore = _animator.CurrentFrame;
        _animator.Play("walk");

        Assert.AreEqual(frameBefore, _animator.CurrentFrame);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run with filter `DirectionalSpriteAnimatorTests`
Expected: FAIL — `DirectionalSpriteAnimator` does not exist

- [ ] **Step 3: Write minimal implementation**

```csharp
using UnityEngine;

public class DirectionalSpriteAnimator : MonoBehaviour
{
    public DirectionalAnimationSet animationSet;

    private SpriteRenderer _spriteRenderer;
    private string _currentClipName;
    private DirectionalClip _currentClip;
    private FacingDirection _facing = FacingDirection.Down;
    private int _currentFrame;
    private float _frameTimer;
    private bool _stopped;

    public int CurrentFrame => _currentFrame;

    public void Initialize()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (animationSet == null) return;

        string startClip = animationSet.defaultClip;
        _currentClipName = startClip;
        _currentClip = animationSet.GetClip(startClip);
        _currentFrame = 0;
        _frameTimer = 0f;
        _stopped = false;
        ApplyFrame();
    }

    public void Play(string clipName)
    {
        if (clipName == _currentClipName) return;

        var clip = animationSet.GetClip(clipName);
        if (clip == null) return;

        _currentClipName = clipName;
        _currentClip = clip;
        _currentFrame = 0;
        _frameTimer = 0f;
        _stopped = false;
        ApplyFrame();
    }

    public void SetFacing(FacingDirection facing)
    {
        if (_facing == facing) return;
        _facing = facing;
        ApplyFrame();
    }

    public void SetFacingFromVector(Vector2 movement)
    {
        if (movement.magnitude < 0.1f) return;
        var newFacing = FacingMath.FromVector(movement);
        SetFacing(newFacing);
    }

    public void Stop()
    {
        _stopped = true;
    }

    public void Tick(float dt)
    {
        if (_stopped || _currentClip == null) return;

        Sprite[] sprites = _currentClip.GetSprites(_facing);
        if (sprites == null || sprites.Length <= 1) return;

        _frameTimer += dt;
        float frameDuration = 1f / _currentClip.framesPerSecond;

        if (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            _currentFrame++;

            if (_currentFrame >= sprites.Length)
            {
                if (_currentClip.loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = sprites.Length - 1;
                    Play(animationSet.defaultClip);
                    return;
                }
            }

            ApplyFrame();
        }
    }

    private void ApplyFrame()
    {
        if (_currentClip == null) return;
        Sprite[] sprites = _currentClip.GetSprites(_facing);
        if (sprites == null || sprites.Length == 0) return;
        int frame = Mathf.Min(_currentFrame, sprites.Length - 1);
        _spriteRenderer.sprite = sprites[frame];
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/DirectionalSpriteAnimator.cs Assets/Tests/EditMode/DirectionalSpriteAnimatorTests.cs
git commit -m "Add DirectionalSpriteAnimator component with frame cycling and facing"
```

---

### Task 4: Guard integration

**Files:**
- Modify: `Assets/Scripts/Guard.cs` (add animator calls in UpdatePatrol)
- Modify: `Assets/Scripts/GuardManager.cs` (wire animator in SpawnGuard, remove static sprite)

**Interfaces:**
- Consumes: `DirectionalSpriteAnimator` from Task 3, `DirectionalAnimationSet` from Task 2, `FacingMath`
- Produces: Guard with animated sprite that switches between idle/walk based on patrol state

- [ ] **Step 1: Modify Guard.cs — add animator field and calls**

Add at the top of the class, after the existing fields:

```csharp
private DirectionalSpriteAnimator _animator;
```

In `Awake()`, after `CreateCone();`:

```csharp
_animator = GetComponent<DirectionalSpriteAnimator>();
```

In `UpdatePatrol(float dt)`, replace the method body. The new version calls the animator when entering walk or idle states:

```csharp
private void UpdatePatrol(float dt)
{
    if (waypoints == null || waypoints.Length == 0) return;

    if (_pausing)
    {
        _pauseTimer -= dt;
        if (_pauseTimer <= 0)
        {
            _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            _pausing = false;
        }
        return;
    }

    Transform target = waypoints[_currentWaypoint];
    if (target == null) return;

    Vector2 dir = (target.position - transform.position);
    float dist = dir.magnitude;

    if (dist < 0.1f)
    {
        _pausing = true;
        _pauseTimer = pauseAtWaypoint;
        if (_animator != null) _animator.Play("idle");
        return;
    }

    Vector2 move = dir.normalized * walkSpeed * dt;
    if (move.magnitude >= dist)
        transform.position = target.position;
    else
        transform.position += (Vector3)move;

    _baseFacing = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    if (_animator != null)
    {
        _animator.SetFacingFromVector(dir);
        _animator.Play("walk");
    }
}
```

In `ResetToStart()`, after `_lookingAway = false;`:

```csharp
if (_animator != null) _animator.Play("idle");
```

Add a `Tick` call in `Update()`, right after `float dt = Time.deltaTime;` and before the caught check:

```csharp
if (_animator != null) _animator.Tick(dt);
```

- [ ] **Step 2: Modify GuardManager.cs — wire animator in SpawnGuard**

Replace the `SpawnGuard` method:

```csharp
private void SpawnGuard(int routeIndex)
{
    var go = new GameObject($"Guard_{_activeGuards.Count}");
    var route = GetRoute(routeIndex);
    if (route != null && route.Length > 0 && route[0] != null)
        go.transform.position = route[0].position;
    else
        go.transform.position = Vector3.zero;

    var sr = go.AddComponent<SpriteRenderer>();
    sr.sortingOrder = 5;

    var animator = go.AddComponent<DirectionalSpriteAnimator>();
    animator.animationSet = ContentDb.Instance != null ? ContentDb.Instance.GuardAnimations : null;
    if (animator.animationSet != null) animator.Initialize();

    var col = go.AddComponent<BoxCollider2D>();
    col.isTrigger = true;
    col.size = new Vector2(0.8f, 1.2f);

    var guard = go.AddComponent<Guard>();
    if (route != null) guard.SetWaypoints(route);
    _activeGuards.Add(guard);
}
```

Remove the `_guardSprite` static field and the `GetGuardSprite()` method entirely.

- [ ] **Step 3: Verify no compile errors**

Run: check Unity console for errors, or via MCP `Unity_GetConsoleLogs`
Expected: No new errors

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Guard.cs Assets/Scripts/GuardManager.cs
git commit -m "Wire DirectionalSpriteAnimator into Guard patrol system"
```

---

### Task 5: ContentDb guard animation set

**Files:**
- Modify: `Assets/Scripts/ContentDb.cs` (add GuardAnimations field and placeholder creation)

**Interfaces:**
- Consumes: `DirectionalAnimationSet` from Task 2, `DirectionalClip` from Task 1
- Produces: `ContentDb.Instance.GuardAnimations` — a `DirectionalAnimationSet` with placeholder idle/walk clips

- [ ] **Step 1: Add GuardAnimations to ContentDb**

Add a new public field after the existing `Residents` dictionary:

```csharp
public DirectionalAnimationSet GuardAnimations;
```

Add a helper method to create placeholder sprites:

```csharp
private static Sprite MakePlaceholderSprite(Color color)
{
    var tex = new Texture2D(4, 4);
    var pixels = new Color32[16];
    for (int i = 0; i < 16; i++) pixels[i] = color;
    tex.SetPixels32(pixels);
    tex.Apply();
    return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
}
```

Add a method to build the guard animation set:

```csharp
private DirectionalAnimationSet BuildGuardAnimations()
{
    var set = new DirectionalAnimationSet();
    Color guardColor = new Color(0.3f, 0.4f, 0.6f);
    Color guardColorDark = new Color(0.25f, 0.35f, 0.55f);
    Color guardColorLight = new Color(0.35f, 0.45f, 0.65f);

    Sprite idleD = MakePlaceholderSprite(guardColor);
    Sprite idleU = MakePlaceholderSprite(guardColorDark);
    Sprite idleL = MakePlaceholderSprite(guardColorLight);
    Sprite idleR = MakePlaceholderSprite(guardColor);

    var idle = new DirectionalClip
    {
        down = new Sprite[] { idleD },
        up = new Sprite[] { idleU },
        left = new Sprite[] { idleL },
        right = new Sprite[] { idleR },
        framesPerSecond = 2f,
        loop = true
    };
    set.AddClip("idle", idle);

    Sprite walkD0 = MakePlaceholderSprite(guardColor);
    Sprite walkD1 = MakePlaceholderSprite(guardColorDark);
    Sprite walkD2 = MakePlaceholderSprite(guardColorLight);
    Sprite walkU0 = MakePlaceholderSprite(guardColorDark);
    Sprite walkU1 = MakePlaceholderSprite(guardColorLight);
    Sprite walkU2 = MakePlaceholderSprite(guardColor);
    Sprite walkL0 = MakePlaceholderSprite(guardColorLight);
    Sprite walkL1 = MakePlaceholderSprite(guardColor);
    Sprite walkL2 = MakePlaceholderSprite(guardColorDark);
    Sprite walkR0 = MakePlaceholderSprite(guardColor);
    Sprite walkR1 = MakePlaceholderSprite(guardColorLight);
    Sprite walkR2 = MakePlaceholderSprite(guardColorDark);

    var walk = new DirectionalClip
    {
        down = new Sprite[] { walkD0, walkD1, walkD2 },
        up = new Sprite[] { walkU0, walkU1, walkU2 },
        left = new Sprite[] { walkL0, walkL1, walkL2 },
        right = new Sprite[] { walkR0, walkR1, walkR2 },
        framesPerSecond = 8f,
        loop = true
    };
    set.AddClip("walk", walk);

    return set;
}
```

In `Awake()`, after the existing `RegisterResident(Berta);` line, add:

```csharp
GuardAnimations = BuildGuardAnimations();
```

- [ ] **Step 2: Verify no compile errors**

Run: check Unity console for errors
Expected: No new errors

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/ContentDb.cs
git commit -m "Add guard directional animation set to ContentDb with placeholders"
```

---

### Task 6: Guard-facing integration test

**Files:**
- Create: `Assets/Tests/EditMode/GuardAnimationTests.cs`

**Interfaces:**
- Consumes: `Guard` from Task 4, `DirectionalSpriteAnimator` from Task 3, `TestBootstrap`, `GameEventsReset`

- [ ] **Step 1: Write integration tests for guard animation**

```csharp
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
```

- [ ] **Step 2: Run test to verify it passes**

Run with filter `GuardAnimationTests`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/GuardAnimationTests.cs
git commit -m "Add guard animation integration tests"
```

---

## Self-Review

**1. Spec coverage:**
- DirectionalClip data struct → Task 1
- DirectionalAnimationSet collection → Task 2
- DirectionalSpriteAnimator component with Play/SetFacing/SetFacingFromVector/Stop/Tick → Task 3
- Guard.cs integration → Task 4
- GuardManager.cs spawn wiring → Task 4
- ContentDb guard animation set → Task 5
- Testing → Tasks 1, 2, 3, 6

**2. Placeholder scan:** No TBDs, TODOs, or vague steps. All code is complete.

**3. Type consistency:**
- `DirectionalClip.GetSprites(FacingDirection)` → returns `Sprite[]` — used consistently in Task 3
- `DirectionalAnimationSet.GetClip(string)` → returns `DirectionalClip` — used in Task 3
- `DirectionalAnimationSet.AddClip(string, DirectionalClip)` → used in Tasks 2, 5
- `DirectionalSpriteAnimator.animationSet` → `DirectionalAnimationSet` — used in Tasks 3, 4, 5
- `DirectionalSpriteAnimator.Initialize()` → used in Tasks 3, 4, 6
- `DirectionalSpriteAnimator.Play(string)` → used in Tasks 3, 4
- `DirectionalSpriteAnimator.SetFacing(FacingDirection)` → used in Task 3
- `DirectionalSpriteAnimator.SetFacingFromVector(Vector2)` → used in Tasks 3, 4
- `DirectionalSpriteAnimator.Tick(float)` → used in Tasks 3, 4
- `DirectionalSpriteAnimator.Stop()` → tested in Task 3
- `ContentDb.GuardAnimations` → `DirectionalAnimationSet` — used in Tasks 4, 6
