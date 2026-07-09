using UnityEngine;

public static class EconomyRules
{
    public const float RiskyBuyerMultiplier = 2f;
    public const int RiskyBuyerHeatPerSale = 15;
    public const float ConfiscationChance = 0.1f;
    public const int ConfiscationHeatThreshold = 50;
    public const int ConfiscationHeatPenalty = 15;

    public static int GetSellPrice(ItemDef item, SellerType seller)
    {
        float multiplier = seller == SellerType.RiskyBuyer ? RiskyBuyerMultiplier : 1f;
        return Mathf.RoundToInt(item.basePrice * multiplier);
    }

    public static int GetBuyPrice(ItemDef item) => item.basePrice;

    public static bool IsCartDay(int day) => day % 3 != 0;

    public static bool IsSellable(ItemDef item, SellerType seller)
    {
        return seller == SellerType.TravelingCart ? item.isBottle : !item.isIngredient;
    }

    public static bool ShouldConfiscate(int currentHeat, IRng rng)
    {
        return currentHeat > ConfiscationHeatThreshold && rng.Value01() < ConfiscationChance;
    }

    public static bool RiskyBuyerAppearsToday(IRng rng, float chance)
    {
        return rng.Value01() < chance;
    }

    public static int PickHour(IRng rng, int minInclusive, int maxInclusive)
    {
        return rng.Range(minInclusive, maxInclusive + 1);
    }
}
