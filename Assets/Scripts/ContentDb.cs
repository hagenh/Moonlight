using System.Collections.Generic;
using UnityEngine;

public class ContentDb : MonoBehaviour
{
    public static ContentDb Instance { get; private set; }

    public readonly Dictionary<string, ItemDef> Items = new();
    public readonly Dictionary<string, ResidentDef> Residents = new();
    public DirectionalAnimationSet GuardAnimations;
    public GameObject CratePrefab;
    public Sprite CrateCarrySprite;

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
    public static readonly ItemDef Stone = new ItemDef("stone", "Stone", true, 1);
    public static readonly ItemDef Wood = new ItemDef("wood", "Wood", true, 2);

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
        Register(Stone);
        Register(Wood);
        RegisterResident(Berta);
        GuardAnimations = BuildGuardAnimations();
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

    private static Sprite MakePlaceholderSprite(Color color)
    {
        var tex = new Texture2D(4, 4);
        var pixels = new Color32[16];
        for (int i = 0; i < 16; i++) pixels[i] = color;
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
    }

    private DirectionalAnimationSet BuildGuardAnimations()
    {
        var set = new DirectionalAnimationSet();
        Color guardColor = new Color(0.3f, 0.4f, 0.6f);
        Color guardColorDark = new Color(0.25f, 0.35f, 0.55f);
        Color guardColorLight = new Color(0.35f, 0.45f, 0.65f);

        Sprite idleD = MakePlaceholderSprite(guardColor);
        Sprite idleU = MakePlaceholderSprite(guardColorDark);
        Sprite idleL = MakePlaceholderSprite(guardColorLight);
        Sprite idleR = MakePlaceholderSprite(guardColor);

        var idle = new DirectionalClip
        {
            down = new Sprite[] { idleD },
            up = new Sprite[] { idleU },
            left = new Sprite[] { idleL },
            right = new Sprite[] { idleR },
            framesPerSecond = 2f,
            loop = true
        };
        set.AddClip("idle", idle);

        Sprite walkD0 = MakePlaceholderSprite(guardColor);
        Sprite walkD1 = MakePlaceholderSprite(guardColorDark);
        Sprite walkD2 = MakePlaceholderSprite(guardColorLight);
        Sprite walkU0 = MakePlaceholderSprite(guardColorDark);
        Sprite walkU1 = MakePlaceholderSprite(guardColorLight);
        Sprite walkU2 = MakePlaceholderSprite(guardColor);
        Sprite walkL0 = MakePlaceholderSprite(guardColorLight);
        Sprite walkL1 = MakePlaceholderSprite(guardColor);
        Sprite walkL2 = MakePlaceholderSprite(guardColorDark);
        Sprite walkR0 = MakePlaceholderSprite(guardColor);
        Sprite walkR1 = MakePlaceholderSprite(guardColorLight);
        Sprite walkR2 = MakePlaceholderSprite(guardColorDark);

        var walk = new DirectionalClip
        {
            down = new Sprite[] { walkD0, walkD1, walkD2 },
            up = new Sprite[] { walkU0, walkU1, walkU2 },
            left = new Sprite[] { walkL0, walkL1, walkL2 },
            right = new Sprite[] { walkR0, walkR1, walkR2 },
            framesPerSecond = 8f,
            loop = true
        };
        set.AddClip("walk", walk);

        return set;
    }
}
