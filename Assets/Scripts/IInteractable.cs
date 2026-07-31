public interface IInteractable
{
    InteractType InteractType { get; }
    bool CanInteract { get; }
    
    void Interact();
}

public enum InteractType
{
    Building,
    FermentVat,
    Seller,
    Bed,
    Debris,
    DebrisPile,
    NPC,
    ExitDoor,
    Crate,
    DeliveryPoint,
    Forage,
    Stand
}
