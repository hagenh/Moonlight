using UnityEngine;

namespace Player.States
{
    public class CarryState : PlayerState
    {
        public CarryState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            controller.ShowCarrySprite(true);
        }

        public override void Exit()
        {
            controller.ShowCarrySprite(false);
        }

        public override void LogicUpdate()
        {
            if (controller.IsMenuOpen) return;

            UpdateFacingDirection(controller.MoveInput);
        }

        public override void PhysicsUpdate()
        {
            float speed = controller.WalkSpeed;
            Vector2 velocity = controller.MoveInput.normalized * speed;
            controller.RB.linearVelocity = velocity;
            controller.SetAnimatorFloat(AnimatorParams.SpeedX, velocity.x);
            controller.SetAnimatorFloat(AnimatorParams.SpeedY, velocity.y);
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
            else if (controller.CurrentInteractable is DeliveryPoint dp)
            {
                dp.Interact();
                if (!controller.IsCarryingCrate)
                    ChangeState(new IdleState(controller));
            }
            else if (controller.CurrentInteractable is ExitDoor
                || controller.CurrentInteractable is Bed
                || controller.CurrentInteractable is Building)
            {
                controller.CurrentInteractable.Interact();
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
