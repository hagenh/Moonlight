public interface IInteractable
{
    InteractType InteractType { get; }
    
    void Interact();
}

public enum InteractType
{
    Building,
    FermentVat,
    Seller,
    Bed
}
