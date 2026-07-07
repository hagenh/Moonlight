using Player.States;
using UnityEngine;

public class MoveState : PlayerState
{
    public MoveState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.SetAnimatorTrigger(AnimatorParams.Move);
    }

    public override void Exit() { }

    public override void LogicUpdate()
    {
        UpdateFacingDirection(controller.MoveInput);
    }

    public override void PhysicsUpdate()
    {
        Vector2 velocity = controller.MoveInput.normalized * controller.WalkSpeed;
        controller.RB.linearVelocity = velocity;
        controller.SetAnimatorFloat(AnimatorParams.SpeedX, velocity.x);
        controller.SetAnimatorFloat(AnimatorParams.SpeedY, velocity.y);
    }

    public override void OnMoveCanceled()
    {
        ChangeState(new IdleState(controller));
    }

    public override void OnInteractPerformed()
    {
        if (controller.CurrentInteractable == null) return;

        if (controller.CurrentInteractable is Building b
            && b.State == BuildingState.Cleared
            && (b.LastHitTrigger == b.BoardTrigger || b.BoardTrigger == null))
        {
            if (BuildingManager.Instance != null
                && BuildingManager.Instance.CanHammer(b))
            {
                ChangeState(new HammerState(controller, b));
                return;
            }
            else
            {
                GameEvents.OnToastRequested(
                    $"Need {b.timberPerRepair} Timber & {b.nailsPerRepair} Nails");
                return;
            }
        }

        ChangeState(new InteractState(controller));
    }

    public override void OnSprintPerformed()
    {
        ChangeState(new SprintState(controller));
    }
}
