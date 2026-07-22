using UnityEngine;

public static class EconomyRules
{
    public static int GetSellPrice(ItemDef item) => item.basePrice;

    public static int GetBuyPrice(ItemDef item) => item.basePrice;

    public static bool IsCartDay(int day) => day % 3 != 0;

    public static bool IsSellable(ItemDef item, SellerType seller)
    {
        return seller == SellerType.TravelingCart ? item.isBottle : !item.isIngredient;
    }

    public static int GetDeliveryPrice(ItemDef item, DeliveryType type)
    {
        float mult = type == DeliveryType.Backwoods ? 1.5f : 1f;
        return Mathf.RoundToInt(item.basePrice * mult);
    }
}
