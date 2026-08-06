using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The roadside stand: a shelf that sells whatever is left on it, and a book of
/// written orders that pay a premium for making what was asked.
///
/// The player is never summoned here. Shelf trade resolves itself overnight and
/// notes wait indefinitely, so the stand fits Act 0's proven shape — start
/// something, go do something else.
/// </summary>
public class StandManager : MonoBehaviour
{
    public static StandManager Instance { get; private set; }

    [SerializeField] private int startingSlots = 3;

    private readonly Dictionary<ItemDef, int> _shelf = new();
    private RequestBook _book;
    private IRng _rng = UnityRng.Instance;
    private int _noteSequence;

    public RequestBook Book => _book;

    internal void SetRng(IRng rng) => _rng = rng;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _book = new RequestBook(startingSlots);
        PostNightNotes(0);
    }

    private void OnEnable()
    {
        GameEvents.DayEnded += OnDayEnded;
    }

    private void OnDisable()
    {
        GameEvents.DayEnded -= OnDayEnded;
    }

    private void OnDayEnded(int day)
    {
        SellShelf();
        PostNightNotes(day);
    }

    private void SellShelf()
    {
        if (GameManager.Instance == null || _shelf.Count == 0) return;

        var sold = new List<KeyValuePair<ItemDef, int>>(_shelf);
        _shelf.Clear();

        foreach (var entry in sold)
        {
            int payment = entry.Key.basePrice * entry.Value;
            GameManager.Instance.AddCash(payment);
            GameEvents.OnShelfSold(entry.Key, entry.Value, payment);
        }
    }

    private void PostNightNotes(int day)
    {
        var available = AvailableRecipes();
        if (available.Count == 0) return;

        int wanted = RequestArrivalRules.NotesPerNight(_book.SlotCount);

        for (int i = 0; i < wanted; i++)
        {
            if (_book.FreeSlots <= 0) return;

            var request = RequestArrivalRules.Generate(available, _rng, $"day{day}-note{_noteSequence++}");
            if (request == null) return;

            if (_book.TryPost(request))
                GameEvents.OnRequestPosted(request);
        }
    }

    /// <summary>
    /// Unlocked and discovered both matter: the book may only ask for something
    /// the player can actually brew tonight. Requests that point past the
    /// player's current recipes are a later plan's job.
    /// </summary>
    private List<RecipeData> AvailableRecipes()
    {
        var list = new List<RecipeData>();
        if (FermentManager.Instance == null)
        {
            list.Add(new RecipeData("Berry Shine", 3, 2, ContentDb.BerryShine));
            return list;
        }

        foreach (var recipe in FermentManager.Instance.UnlockedRecipes)
            if (recipe?.outputItem != null && FermentManager.Instance.IsRecipeDiscovered(recipe))
                list.Add(recipe);

        return list;
    }

    public bool TryFill(string requestId, ItemDef with)
    {
        if (InventoryManager.Instance == null || GameManager.Instance == null) return false;

        var request = FindActive(requestId);
        if (request == null) return false;
        if (!RequestBookRules.Accepts(request, with)) return false;
        if (!InventoryManager.Instance.Has(with, request.Units)) return false;

        if (!InventoryManager.Instance.TryRemove(with, request.Units)) return false;

        _book.Take(requestId);
        int payment = RequestBookRules.Payment(request, with);
        GameManager.Instance.AddCash(payment);
        GameEvents.OnRequestFilled(request, payment);
        return true;
    }

    /// <summary>
    /// Declining costs nothing and is the intended way to clear a note the player
    /// cannot or will not fill. It exists so an unfillable request can never
    /// occupy a slot permanently.
    /// </summary>
    public bool Decline(string requestId)
    {
        var request = _book.Take(requestId);
        if (request == null) return false;

        GameEvents.OnRequestDeclined(request);
        return true;
    }

    public void StockShelf(ItemDef item, int count)
    {
        if (item == null || count <= 0 || InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.TryRemove(item, count)) return;

        _shelf[item] = ShelfCount(item) + count;
    }

    public int ShelfCount(ItemDef item)
    {
        if (item == null) return 0;
        return _shelf.GetValueOrDefault(item, 0);
    }

    private StandRequest FindActive(string id)
    {
        foreach (var request in _book.Active)
            if (request.Id == id) return request;

        return null;
    }
}
