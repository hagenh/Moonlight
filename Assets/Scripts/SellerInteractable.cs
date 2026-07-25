using UnityEngine;

public class SellerInteractable : MonoBehaviour, IInteractable
{
    public SellerType sellerType;

    public InteractType InteractType => InteractType.Seller;

    public void Interact()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.IsCarryingCrate)
        {
            var crate = PlayerController.Instance.CarriedCrate;
            if (crate != null)
            {
                DeliveryType type = sellerType == SellerType.Tormod ? DeliveryType.Tormod : DeliveryType.Cart;
                int price = EconomyRules.GetDeliveryPrice(crate.item, type) * crate.count;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCash(price);
                    GameEvents.OnToastRequested($"+{price}g");
                }

                Destroy(crate.gameObject);
                PlayerController.Instance.DropCrate();
                GameEvents.OnDeliveryMade(type, crate.item, crate.count, price);
                return;
            }
        }

        if (SellManager.Instance != null)
            SellManager.Instance.OpenSellMenu(sellerType);
    }

    public static SellerInteractable Create(SellerType type, Vector3 position)
    {
        var go = new GameObject(type.ToString());
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        sr.color = type switch
        {
            SellerType.Tormod => new Color(0.9f, 0.8f, 0.3f),
            SellerType.TravelingCart => new Color(0.5f, 0.4f, 0.8f),
            _ => Color.white
        };
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        if (BuildingManager.Instance != null && BuildingManager.Instance.Buildings.Count > 0)
            go.layer = BuildingManager.Instance.Buildings[0].DoorTrigger?.gameObject.layer
                ?? LayerMask.NameToLayer("Interactable");
        else
            go.layer = LayerMask.NameToLayer("Interactable");

        var interactable = go.AddComponent<SellerInteractable>();
        interactable.sellerType = type;

        return interactable;
    }
}
