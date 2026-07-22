using System.Collections.Generic;
using UnityEngine;

public class ContentDb : MonoBehaviour
{
    public static ContentDb Instance { get; private set; }

    public readonly Dictionary<string, ItemDef> Items = new();
    public readonly Dictionary<string, ResidentDef> Residents = new();

    public static readonly ItemDef Grain = new ItemDef("grain", "Grain", true, 5);
    public static readonly ItemDef Sugar = new ItemDef("sugar", "Sugar", true, 5);
    public static readonly ItemDef Yeast = new ItemDef("yeast", "Yeast", true, 8);
    public static readonly ItemDef Water = new ItemDef("water", "Water", true, 2);
    public static readonly ItemDef BasicMoonshine = new ItemDef("basic_moonshine", "Basic Moonshine", false, 25, true);
    public static readonly ItemDef SweetMoonshine = new ItemDef("sweet_moonshine", "Sweet Moonshine", false, 40, true);
    public static readonly ItemDef HighlandMoonshine = new ItemDef("highland_moonshine", "Highland Moonshine", false, 60, true);
    public static readonly ItemDef AgedReserve = new ItemDef("aged_reserve", "Aged Reserve", false, 120, true);
    public static readonly ItemDef Timber = new ItemDef("timber", "Timber", true, 10);
    public static readonly ItemDef Nails = new ItemDef("nails", "Nails", true, 8);
    public static readonly ItemDef Berry = new ItemDef("berry", "Berry", true, 2);
    public static readonly ItemDef BerryShine = new ItemDef("berry_shine", "Berry Shine", false, 15, true);

    public static readonly ResidentDef Berta = new ResidentDef(
        "berta", "Berta", "Bakery",
        new Color(0.85f, 0.65f, 0.45f),
        new Color(0.85f, 0.65f, 0.45f),
        new ScheduleEntry[]
        {
            new ScheduleEntry(8, "Berta_Home"),
            new ScheduleEntry(10, "Berta_Market"),
            new ScheduleEntry(14, "Berta_Well"),
            new ScheduleEntry(17, "Berta_Home"),
        },
        new string[][]
        {
            new string[] { "...", "I don't know you.", "Leave me be." },
            new string[] { "Good morning.", "Business is steady.", "The oven's warm today." },
            new string[] { "You've done right by us.", "Best bread in town, thanks to you.", "Glad to be here." },
        },
        "A fresh start... I can work with this."
    );

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
        Register(HighlandMoonshine);
        Register(AgedReserve);
        Register(Timber);
        Register(Nails);
        Register(Berry);
        Register(BerryShine);
        RegisterResident(Berta);
    }

    private void Register(ItemDef def)
    {
        Items[def.id] = def;
    }

    public ItemDef GetItem(string id)
    {
        return Items.GetValueOrDefault(id);
    }

    private void RegisterResident(ResidentDef def)
    {
        Residents[def.id] = def;
    }

    public ResidentDef GetResident(string id)
    {
        return Residents.GetValueOrDefault(id);
    }
}
