using UnityEngine;

public class Stand : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Stand;
    public bool CanInteract => true;

    public void Interact()
    {
        GameEvents.OnRequestBookRequested();
    }


}
