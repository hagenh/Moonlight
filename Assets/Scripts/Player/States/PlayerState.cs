using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController controller;

    public PlayerState(PlayerController controller)
    {
        this.controller = controller;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();

    public virtual void OnMovePerformed(Vector2 input) { }
    public virtual void OnMoveCanceled() { }
    public virtual void OnSprintPerformed() { }
    public virtual void OnSprintCanceled() { }
    public virtual void OnInteractPerformed() { }

    protected void ChangeState(PlayerState newState)
    {
        controller.ChangeState(newState);
    }

    protected void UpdateFacingDirection(Vector2 input)
    {
        controller.UpdateFacingDirection(input);
    }
}
