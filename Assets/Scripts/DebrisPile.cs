using UnityEngine;

public class DebrisPile : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.DebrisPile;

    public void Interact()
    {
        if (PlayerController.Instance == null) return;

        if (!PlayerController.Instance.IsCarrying)
        {
            GameEvents.OnToastRequested("Nothing to deposit");
            return;
        }

        var carried = PlayerController.Instance.CarriedDebris;
        if (carried != null)
            Destroy(carried.gameObject);

        var sourceBuilding = carried?.sourceBuilding;
        PlayerController.Instance.DropDebris();

        if (sourceBuilding != null)
        {
            sourceBuilding.OnDebrisDeposited();
            GameEvents.OnDebrisDeposited(sourceBuilding);
        }

        GameEvents.OnToastRequested("Debris deposited");
    }
}
