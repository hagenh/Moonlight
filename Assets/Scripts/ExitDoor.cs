using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.ExitDoor;
    public bool CanInteract => true;

    public void Interact()
    {
        if (InteriorManager.Instance != null)
            InteriorManager.Instance.ExitInterior();
    }
}
