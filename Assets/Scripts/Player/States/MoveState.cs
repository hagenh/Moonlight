using Player.States;
using UnityEngine;

public class MoveState : PlayerState
{
    public MoveState(PlayerController controller) : base(controller) { }

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
        controller.RB.linearVelocity = controller.MoveInput.normalized * controller.WalkSpeed;
    }

    public override void OnMoveCanceled()
    {
        ChangeState(new IdleState(controller));
    }

    public override void OnInteractPerformed()
    {
        if (controller.CurrentInteractable == null) return;
        if (TryEnterHammerState()) return;
        ChangeState(new InteractState(controller));
    }

    public override void OnSprintPerformed()
    {
        ChangeState(new SprintState(controller));
    }
}
