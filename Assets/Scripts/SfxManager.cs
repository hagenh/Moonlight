using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource loopAudioSource;

    [SerializeField] internal AudioClip[] coinClips;
    [SerializeField] internal AudioClip[] pickupClips;
    [SerializeField] internal AudioClip[] dropClips;
    [SerializeField] internal AudioClip[] hammerClips;
    [SerializeField] internal AudioClip[] bookOpenClips;
    [SerializeField] internal AudioClip[] bagOpenClips;
    [SerializeField] internal AudioClip[] bagCloseClips;
    [SerializeField] internal AudioClip[] selectClips;
    [SerializeField] internal AudioClip[] buttonClips;

    private IRng _rng = UnityRng.Instance;

    internal AudioClip LastPlayedClip { get; private set; }
    internal bool IsForageLoopPlaying { get; private set; }

    internal void SetRng(IRng rng) => _rng = rng;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.CashChanged += OnCashChanged;
        GameEvents.InventoryChanged += OnInventoryChanged;
        GameEvents.ItemDropped += OnItemDropped;
        GameEvents.SmashHit += OnSmashHit;
        GameEvents.RequestBookRequested += OnBookRequested;
        GameEvents.RecipeBookRequested += OnBookRequested;
        GameEvents.InventoryOpened += OnInventoryOpened;
        GameEvents.InventoryClosed += OnInventoryClosed;
        GameEvents.SellMenuRequested += OnSellMenuRequested;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
        GameEvents.ForageStarted += OnForageStarted;
        GameEvents.ForageEnded += OnForageEnded;
    }

    private void OnDisable()
    {
        GameEvents.CashChanged -= OnCashChanged;
        GameEvents.InventoryChanged -= OnInventoryChanged;
        GameEvents.ItemDropped -= OnItemDropped;
        GameEvents.SmashHit -= OnSmashHit;
        GameEvents.RequestBookRequested -= OnBookRequested;
        GameEvents.RecipeBookRequested -= OnBookRequested;
        GameEvents.InventoryOpened -= OnInventoryOpened;
        GameEvents.InventoryClosed -= OnInventoryClosed;
        GameEvents.SellMenuRequested -= OnSellMenuRequested;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
        GameEvents.ForageStarted -= OnForageStarted;
        GameEvents.ForageEnded -= OnForageEnded;
        StopForageLoop();
    }

    private void OnCashChanged(int newCash) => Play(coinClips);

    private void OnInventoryChanged(ItemDef def, int oldCount, int newCount)
    {
        if (newCount > oldCount)
            Play(pickupClips);
    }

    private void OnItemDropped(int slotIndex, ItemDef def, int count) => Play(dropClips);
    private void OnSmashHit(Building b, int done, int required) => Play(hammerClips);
    private void OnBookRequested() => Play(bookOpenClips);
    private void OnInventoryOpened() => Play(bagOpenClips);
    private void OnInventoryClosed() => Play(bagCloseClips);
    private void OnSellMenuRequested(SellerType type) => Play(selectClips);
    private void OnMenuCloseRequested() => Play(buttonClips);

    private void OnForageStarted(IForageable target)
    {
        var clip = PickClip(hammerClips);
        LastPlayedClip = clip;
        IsForageLoopPlaying = clip != null;
        if (clip == null || loopAudioSource == null) return;
        loopAudioSource.clip = clip;
        loopAudioSource.loop = true;
        loopAudioSource.Play();
    }

    private void OnForageEnded(IForageable target) => StopForageLoop();

    private void StopForageLoop()
    {
        IsForageLoopPlaying = false;
        if (loopAudioSource != null)
            loopAudioSource.Stop();
    }

    internal AudioClip PickClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[_rng.Range(0, clips.Length)];
    }

    private void Play(AudioClip[] clips)
    {
        var clip = PickClip(clips);
        LastPlayedClip = clip;
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}
