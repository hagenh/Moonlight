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
            UpdateFacingDirection(controller.MoveInput);
        }

        public override void PhysicsUpdate()
        {
            float speed = controller.WalkSpeed * 0.8f;
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
            else
            {
                GameEvents.OnToastRequested("Bring debris to the pile");
            }
        }

        public override void OnSprintPerformed()
        {
        }
    }
}
