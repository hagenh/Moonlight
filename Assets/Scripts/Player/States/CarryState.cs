using UnityEngine;

namespace Player.States
{
    public class CarryState : PlayerState
    {
        public CarryState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            controller.ShowCarrySprite(true);
            controller.PlayAnimation("idle");
        }

        public override void Exit()
        {
            controller.ShowCarrySprite(false);
        }

        public override void LogicUpdate()
        {
            if (controller.IsMenuOpen) return;

            UpdateFacingDirection(controller.MoveInput);

            if (controller.MoveInput.magnitude > 0.1f)
                controller.PlayAnimation("walk");
            else
                controller.PlayAnimation("idle");
        }

        public override void PhysicsUpdate()
        {
            controller.RB.linearVelocity = controller.MoveInput.normalized * controller.WalkSpeed;
        }

        public override void OnMoveCanceled()
        {
        }

        public override void OnInteractPerformed()
        {
            if (controller.CurrentInteractable is DebrisPile pile)
            {
                pile.Interact();
                if (!controller.IsCarrying)
                    ChangeState(new IdleState(controller));
            }
            else if (controller.CurrentInteractable is SellerInteractable
                || controller.CurrentInteractable is ExitDoor
                || controller.CurrentInteractable is Bed
                || controller.CurrentInteractable is Building)
            {
                controller.CurrentInteractable.Interact();
                if (!controller.IsCarryingCrate)
                    ChangeState(new IdleState(controller));
            }
            else
            {
                if (controller.IsCarrying) controller.DropDebrisAtFeet();
                if (controller.IsCarryingCrate) controller.DropCrateAtFeet();
                GameEvents.OnToastRequested("Dropped");
                ChangeState(new IdleState(controller));
            }
        }

        public override void OnSprintPerformed()
        {
        }
    }
}
