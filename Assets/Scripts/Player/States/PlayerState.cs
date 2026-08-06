using UnityEngine;
using Player.States;

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
    public virtual void OnInteractCanceled() { }

    protected void ChangeState(PlayerState newState)
    {
        controller.ChangeState(newState);
    }

    protected void UpdateFacingDirection(Vector2 input)
    {
        controller.UpdateFacingDirection(input);
    }

    protected bool TryEnterHammerState()
    {
        if (controller.CurrentInteractable is Building b
            && b.State == BuildingState.Cleared
            && (b.LastHitTrigger == b.BoardTrigger || b.BoardTrigger == null))
        {
            if (BuildingManager.Instance != null
                && BuildingManager.Instance.CanHammer(b))
            {
                ChangeState(new HammerState(controller, b));
                return true;
            }

            GameEvents.OnToastRequested(
                $"Need {b.TimberPerRepair} Timber & {b.NailsPerRepair} Nails");
            return true;
        }

        return false;
    }

    protected bool TryEnterForageState()
    {
        if (controller.CurrentInteractable is IForageable forageable && !forageable.IsHarvested)
        {
            if (forageable.RequiredTool != null
                && (InventoryManager.Instance == null || !InventoryManager.Instance.Has(forageable.RequiredTool, 1)))
            {
                GameEvents.OnToastRequested($"Need a {forageable.RequiredTool.displayName}");
                return true;
            }

            ChangeState(new ForageState(controller, forageable));
            return true;
        }

        return false;
    }
}
