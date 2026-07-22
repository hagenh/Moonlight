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

        int suspicion = GameManager.Instance != null ? GameManager.Instance.Heat : 0;
        int price = EconomyRules.GetDeliveryPrice(crate.item, deliveryType, suspicion) * crate.count;

        if (price <= 0 && deliveryType == DeliveryType.Cart)
        {
            GameEvents.OnToastRequested("The cart driver won't deal with you — too much heat.");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCash(price);
            GameEvents.OnToastRequested($"+{price}g");

            if (deliveryType == DeliveryType.Backwoods && TimeManager.Instance != null)
            {
                var recipe = FermentManager.Instance != null
                    ? FermentManager.Instance.FindRecipeForItem(crate.item)
                    : null;
                if (recipe != null)
                {
                    int suspicionGain = EconomyRules.GetSuspicionForDrop(recipe, TimeManager.Instance.Hour);
                    if (suspicionGain > 0)
                        GameManager.Instance.AddHeat(suspicionGain);
                }
            }
        }

        Destroy(crate.gameObject);
        PlayerController.Instance.DropCrate();
        GameEvents.OnDeliveryMade(deliveryType, crate.item, crate.count, price);
    }
}
