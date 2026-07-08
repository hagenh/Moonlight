using UnityEngine;

namespace Player.States
{
    public class HammerState : PlayerState
    {
        private readonly Building _target;
        private float _progress;
        private bool _completed;

        public HammerState(PlayerController controller, Building target) : base(controller)
        {
            _target = target;
        }

        public override void Enter()
        {
            controller.RB.linearVelocity = Vector2.zero;
            _progress = 0f;
            _completed = false;
            GameEvents.OnHammerStarted(_target);
        }

        public override void Exit()
        {
            GameEvents.OnHammerEnded(_target);
        }

        public override void LogicUpdate()
        {
            if (_completed) return;

            if (controller.IsMenuOpen)
            {
                ChangeState(new IdleState(controller));
                return;
            }

            if (controller.IsInteractHeld)
            {
                _progress += Time.deltaTime / _target.hammerDuration;
                GameEvents.OnHammerProgress(_target, _progress);

                if (_progress >= 1f)
                {
                    _progress = 1f;
                    CompleteRepairPoint();
                }
            }
        }

        public override void PhysicsUpdate()
        {
            controller.RB.linearVelocity = Vector2.zero;
        }

        public override void OnInteractCanceled()
        {
            if (_completed) return;

            GameEvents.OnToastRequested("Repair interrupted");
            ChangeState(new IdleState(controller));
        }

        private void CompleteRepairPoint()
        {
            _completed = true;

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.TryHammerHit(_target);

            ChangeState(new IdleState(controller));
        }
    }
}
