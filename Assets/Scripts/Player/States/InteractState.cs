using UnityEngine;

namespace Player.States
{
    public class InteractState : PlayerState
    {
        public InteractState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            controller.PlayAnimation("idle");

            if (controller.CurrentInteractable != null)
                controller.CurrentInteractable.Interact();

            if (controller.IsCarrying || controller.IsCarryingCrate)
                ChangeState(new CarryState(controller));
            else
                ChangeState(new IdleState(controller));
        }

        public override void Exit() { }

        public override void LogicUpdate() { }

        public override void PhysicsUpdate() { }
    }
}
