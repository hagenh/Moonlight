using System.Linq;
using UnityEngine;

public static class EconomyRules
{
    public const float RiskyBuyerMultiplier = 2f;
    public const int RiskyBuyerHeatPerSale = 15;
    public const float ConfiscationChance = 0.1f;
    public const int ConfiscationHeatThreshold = 50;
    public const int ConfiscationHeatPenalty = 15;

    public const int RaidThreshold = 61;
    public const int RaidSuspicionReset = 30;
    public const int RaidFine = 100;
    public const float RaidCrateLossPercent = 0.5f;

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

    public enum SuspicionTier { Clean, Noticed, TalkedAbout, Burning }

    public static SuspicionTier GetSuspicionTier(int suspicion) => suspicion switch
    {
        <= 20 => SuspicionTier.Clean,
        <= 40 => SuspicionTier.Noticed,
        <= 60 => SuspicionTier.TalkedAbout,
        _     => SuspicionTier.Burning
    };

    public static int GetDeliveryPrice(ItemDef item, DeliveryType type, int suspicion)
    {
        float mult = type == DeliveryType.Backwoods ? 1.5f : 1f;
        if (type == DeliveryType.Cart)
        {
            var tier = GetSuspicionTier(suspicion);
            if (tier == SuspicionTier.Noticed) mult = 0.9f;
            if (tier >= SuspicionTier.TalkedAbout) return 0;
        }
        return Mathf.RoundToInt(item.basePrice * mult);
    }

    public static int GetSuspicionForDrop(RecipeData recipe, int hour)
    {
        bool daytime = hour >= 8 && hour < 18;
        return daytime ? recipe.suspicionPerDrop : 0;
    }

    public static int GetGuardCountForSuspicion(int suspicion) => GetSuspicionTier(suspicion) switch
    {
        SuspicionTier.Clean => 1,
        SuspicionTier.Noticed => 2,
        SuspicionTier.TalkedAbout => 3,
        _ => 4
    };
}
