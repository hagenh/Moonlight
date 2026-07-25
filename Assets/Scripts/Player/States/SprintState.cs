using UnityEngine;

public class SprintState : PlayerState
{
    public SprintState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.PlayAnimation("walk");
    }

    public override void Exit() { }

    public override void LogicUpdate()
    {
        UpdateFacingDirection(controller.MoveInput);
    }

    public override void PhysicsUpdate()
    {
        controller.RB.linearVelocity = controller.MoveInput.normalized * controller.SprintSpeed;
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
