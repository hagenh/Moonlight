using UnityEngine;

public class ItemDef
{
    public string id;
    public string displayName;
    public bool isIngredient = true;
    public int basePrice;
    public bool isBottle;
    public Sprite icon;
    public bool isPlaceable;
    public GameObject placedPrefab;
    public int footprintWidth = 1;
    public int footprintHeight = 1;

    public ItemDef(string id, string displayName, bool isIngredient = true, int basePrice = 0, bool isBottle = false, Sprite icon = null,
        bool isPlaceable = false, GameObject placedPrefab = null, int footprintWidth = 1, int footprintHeight = 1)
    {
        this.id = id;
        this.displayName = displayName;
        this.isIngredient = isIngredient;
        this.basePrice = basePrice;
        this.isBottle = isBottle;
        this.icon = icon;
        this.isPlaceable = isPlaceable;
        this.placedPrefab = placedPrefab;
        this.footprintWidth = footprintWidth;
        this.footprintHeight = footprintHeight;
    }
}
