using UnityEngine;

public class TormodInteractable : MonoBehaviour, IInteractable
{
    public InteractType InteractType => InteractType.NPC;
    public bool CanInteract => true;

    private const string FirstConversationLine = "\"For the walls, when you get to them.\"";

    private bool _hasTalked;

    public void Interact()
    {
        if (!_hasTalked)
        {
            bool granted = InventoryManager.Instance != null
                && InventoryManager.Instance.TryAdd(ContentDb.Nails, 3);
            if (granted)
            {
                _hasTalked = true;
                GameEvents.OnToastRequested("+3 Nails from Tormod");
            }
            GameEvents.OnDialogueRequested(ContentDb.Tormod, FirstConversationLine);
            return;
        }

        string line = ContentDb.Tormod.GetDialogueLine(
            GameManager.Instance != null ? GameManager.Instance.Reputation : 0);
        GameEvents.OnDialogueRequested(ContentDb.Tormod, line);
    }
}
