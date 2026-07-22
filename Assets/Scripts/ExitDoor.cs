using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.ExitDoor;

    public void Interact()
    {
        if (InteriorManager.Instance != null)
            InteriorManager.Instance.ExitInterior();
    }
}
