using UnityEngine;

public class DeliveryPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private DeliveryType deliveryType;

    public DeliveryType DeliveryType => deliveryType;
    public InteractType InteractType => InteractType.DeliveryPoint;

    internal void SetDeliveryType(DeliveryType type) => deliveryType = type;

    public void Interact()
    {
        if (PlayerController.Instance == null) return;

        if (!PlayerController.Instance.IsCarryingCrate)
        {
            if (deliveryType == DeliveryType.Cart && SellManager.Instance != null)
            {
                SellManager.Instance.OpenSellMenu(SellerType.TravelingCart);
            }
            else
            {
                GameEvents.OnToastRequested("Nothing to deliver");
            }
            return;
        }

        var crate = PlayerController.Instance.CarriedCrate;
        if (crate == null) return;

        int price = EconomyRules.GetDeliveryPrice(crate.item, deliveryType) * crate.count;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCash(price);
            GameEvents.OnToastRequested($"+{price}g");
        }

        Destroy(crate.gameObject);
        PlayerController.Instance.DropCrate();
        GameEvents.OnDeliveryMade(deliveryType, crate.item, crate.count, price);
    }
}
