using UnityEngine;

namespace Player.States
{
    public class InteractState : PlayerState
    {
        public InteractState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            controller.SetAnimatorTrigger(AnimatorParams.Interact);
            
            Debug.Log("Entered interact state");

            if (controller.CurrentInteractable != null)
            {
                Debug.Log("Trying to interact with " + controller.CurrentInteractable);
                controller.CurrentInteractable.Interact();
            }

            ChangeState(new IdleState(controller));
        }

        public override void Exit() { }

        public override void LogicUpdate() { }

        public override void PhysicsUpdate() { }
    }
}
