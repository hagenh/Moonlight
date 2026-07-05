using UnityEngine;

public class SprintState : PlayerState
{
    public SprintState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.SetAnimatorTrigger(AnimatorParams.Sprint);
    }

    public override void Exit() { }

    public override void LogicUpdate()
    {
        UpdateFacingDirection(controller.MoveInput);
    }

    public override void PhysicsUpdate()
    {
        Vector2 velocity = controller.MoveInput.normalized * controller.SprintSpeed;
        controller.RB.linearVelocity = velocity;
        controller.SetAnimatorFloat(AnimatorParams.SpeedX, velocity.x);
        controller.SetAnimatorFloat(AnimatorParams.SpeedY, velocity.y);
    }

    public override void OnMoveCanceled()
    {
        ChangeState(new IdleState(controller));
    }

    public override void OnSprintCanceled()
    {
        ChangeState(new MoveState(controller));
    }
}
