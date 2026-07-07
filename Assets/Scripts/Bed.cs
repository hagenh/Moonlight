using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Bed;

    public void Interact()
    {
        if (SleepManager.Instance != null)
            SleepManager.Instance.BeginSleep();
    }
}
