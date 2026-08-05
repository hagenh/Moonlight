using UnityEngine;

public class InfrastructureManager : MonoBehaviour
{
    public static InfrastructureManager Instance { get; private set; }

    private const int PlaceholderSeedCount = 5;

    private readonly BuildBook _book = new();

    public BuildBook Book => _book;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SeedPlaceholderEntries();
    }

    private void SeedPlaceholderEntries()
    {
        _book.Add(ContentDb.Lamppost, PlaceholderSeedCount);
        _book.Add(ContentDb.PlankSidewalk, PlaceholderSeedCount);
        _book.Add(ContentDb.Bench, PlaceholderSeedCount);
        _book.Add(ContentDb.FlowerBox, PlaceholderSeedCount);
        _book.Add(ContentDb.Sign, PlaceholderSeedCount);
    }

    public bool TryConsume(ItemDef item) => _book.TryConsume(item);
}
