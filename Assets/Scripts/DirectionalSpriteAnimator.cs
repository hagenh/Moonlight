using UnityEngine;

public class DirectionalSpriteAnimator : MonoBehaviour
{
    public DirectionalAnimationSet animationSet;

    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer Renderer => _spriteRenderer ??= GetComponent<SpriteRenderer>();
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
        if (animationSet == null) return;

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
        Renderer.sprite = sprites[frame];
    }
}
