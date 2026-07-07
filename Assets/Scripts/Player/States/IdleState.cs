using Player.States;
using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.SetAnimatorTrigger(AnimatorParams.Idle);
        controller.RB.linearVelocity = Vector2.zero;
        controller.SetAnimatorFloat(AnimatorParams.SpeedX, 0f);
        controller.SetAnimatorFloat(AnimatorParams.SpeedY, 0f);
    }

    public override void Exit() { }

    public override void LogicUpdate() { }

    public override void PhysicsUpdate()
    {
        controller.RB.linearVelocity = Vector2.zero;
    }

    public override void OnMovePerformed(Vector2 input)
    {
        UpdateFacingDirection(input);
        if (controller.IsSprintHeld)
            ChangeState(new SprintState(controller));
        else
            ChangeState(new MoveState(controller));
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
}
