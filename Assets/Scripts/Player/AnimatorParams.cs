using UnityEngine;

public static class AnimatorParams
{
    public static readonly int FacingDirection = Animator.StringToHash("FacingDirection");
    public static readonly int SpeedX = Animator.StringToHash("SpeedX");
    public static readonly int SpeedY = Animator.StringToHash("SpeedY");
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Move = Animator.StringToHash("Move");
    public static readonly int Sprint = Animator.StringToHash("Sprint");
    public static readonly int Interact = Animator.StringToHash("Interact");
}
