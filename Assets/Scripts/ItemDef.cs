using UnityEngine;

public class ItemDef
{
    public string id;
    public string displayName;
    public bool isIngredient = true;
    public int basePrice;
    public bool isBottle;
    public Sprite icon;

    public ItemDef(string id, string displayName, bool isIngredient = true, int basePrice = 0, bool isBottle = false, Sprite icon = null)
    {
        this.id = id;
        this.displayName = displayName;
        this.isIngredient = isIngredient;
        this.basePrice = basePrice;
        this.isBottle = isBottle;
        this.icon = icon;
    }
}
