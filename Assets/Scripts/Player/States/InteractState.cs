using UnityEngine;

namespace Player.States
{
    public class InteractState : PlayerState
    {
        public InteractState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            controller.SetAnimatorTrigger(AnimatorParams.Interact);

            if (controller.CurrentInteractable != null)
                controller.CurrentInteractable.Interact();

            if (controller.IsCarrying)
                ChangeState(new CarryState(controller));
            else
                ChangeState(new IdleState(controller));
        }

        public override void Exit() { }

        public override void LogicUpdate() { }

        public override void PhysicsUpdate() { }
    }
}
