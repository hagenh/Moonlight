using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.Bed;

    public void Interact()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.HourF < 21f)
        {
            GameEvents.OnToastRequested("It's too early to sleep.");
            return;
        }

        if (SleepManager.Instance != null)
            SleepManager.Instance.BeginSleep();
    }
}
