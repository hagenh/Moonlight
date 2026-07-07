using System.Collections.Generic;
using UnityEngine;

public class ContentDb : MonoBehaviour
{
    public static ContentDb Instance { get; private set; }

    public readonly Dictionary<string, ItemDef> Items = new();

    public static readonly ItemDef Grain = new ItemDef("grain", "Grain", true, 5);
    public static readonly ItemDef Sugar = new ItemDef("sugar", "Sugar", true, 5);
    public static readonly ItemDef Yeast = new ItemDef("yeast", "Yeast", true, 8);
    public static readonly ItemDef Water = new ItemDef("water", "Water", true, 2);
    public static readonly ItemDef BasicMoonshine = new ItemDef("basic_moonshine", "Basic Moonshine", false, 25, true);
    public static readonly ItemDef SweetMoonshine = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);
    public static readonly ItemDef Timber = new ItemDef("timber", "Timber", true, 10);
    public static readonly ItemDef Nails = new ItemDef("nails", "Nails", true, 8);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Register(Grain);
        Register(Sugar);
        Register(Yeast);
        Register(Water);
        Register(BasicMoonshine);
        Register(SweetMoonshine);
        Register(Timber);
        Register(Nails);
    }

    private void Register(ItemDef def)
    {
        Items[def.id] = def;
    }

    public ItemDef GetItem(string id)
    {
        return Items.GetValueOrDefault(id);
    }
}
