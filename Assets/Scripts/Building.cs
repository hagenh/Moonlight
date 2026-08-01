using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum BuildingState
{
    Abandoned,
    Purchased,
    Cleared,
    Restored
}

public class Building : MonoBehaviour, IInteractable
{
    [SerializeField] private string buildingName = "Bakery";
    [SerializeField] private int purchaseCost = 100;
    [SerializeField] private int dailyIncome = 20;

    [Header("Renovation")]
    [SerializeField] private bool isFacadeOnly = false;
    [SerializeField] private int smashHitsRequired = 3;
    [SerializeField] private int debrisCount = 3;
    [SerializeField] private int totalRepairPoints = 3;
    [SerializeField] private int timberPerRepair = 1;
    [SerializeField] private int nailsPerRepair = 1;
    [SerializeField] private float hammerDuration = 2f;

    public string BuildingName => buildingName;
    public int PurchaseCost => purchaseCost;
    public int DailyIncome => dailyIncome;
    public bool IsFacadeOnly => isFacadeOnly;
    public int SmashHitsRequired => smashHitsRequired;
    public int DebrisCount => debrisCount;
    public int TotalRepairPoints => totalRepairPoints;
    public int TimberPerRepair => timberPerRepair;
    public int NailsPerRepair => nailsPerRepair;
    public float HammerDuration => hammerDuration;

    public Transform InteriorSpawn => interiorSpawn;

    [Header("Visuals")]
    [SerializeField] private Light2D[] windowLights;

    [SerializeField] private SpriteRenderer facadeRenderer;
    [SerializeField] private GameObject preBuildVisual;

    [Header("Interior")]
    [SerializeField] private Transform interiorSpawn;

    [Header("Interaction")]
    [SerializeField] private Collider2D boardTrigger;
    [SerializeField] private Collider2D doorTrigger;

    public BuildingState State { get; private set; } = BuildingState.Abandoned;
    public int UncollectedIncome { get; private set; }

    public int SmashHitsDone { get; private set; }
    public bool BoardsSmashed { get; private set; }
    public int DebrisRemaining { get; private set; }
    public int RepairPointsDone { get; private set; }

    public InteractType InteractType => InteractType.Building;
    public bool CanInteract => true;

    public Collider2D BoardTrigger => boardTrigger;
    public Collider2D DoorTrigger => doorTrigger;

    public Collider2D LastHitTrigger { get; set; }

    private Coroutine _punchCoroutine;

    private Transform VisualTransform => facadeRenderer != null ? facadeRenderer.transform : transform;

    private void Start()
    {
        RefreshVisuals();
    }

    private void OnEnable()
    {
        if (BuildingManager.Instance != null)
            BuildingManager.Instance.Register(this);
        GameEvents.BuildingStateChanged += OnBuildingStateChanged;
    }

    private void OnDisable()
    {
        if (BuildingManager.Instance != null)
            BuildingManager.Instance.Unregister(this);
        GameEvents.BuildingStateChanged -= OnBuildingStateChanged;
    }

    private void OnBuildingStateChanged(Building b, BuildingState oldState, BuildingState newState)
    {
        if (b == this) StartPunchScale();
    }

    private void StartPunchScale()
    {
        if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
        _punchCoroutine = StartCoroutine(JuiceUtils.PunchScale(VisualTransform));
    }

    public void Interact()
    {
        if (BuildingManager.Instance == null) return;

        if (doorTrigger != null && LastHitTrigger == doorTrigger)
        {
            if (State == BuildingState.Restored)
            {
                if (InteriorManager.Instance != null)
                    InteriorManager.Instance.EnterInterior(this);
                else
                    GameEvents.OnToastRequested("It's empty inside...");
            }
            return;
        }

        if (LastHitTrigger == boardTrigger || (boardTrigger != null && doorTrigger == null))
        {
            switch (State)
            {
                case BuildingState.Abandoned:
                    BuildingManager.Instance.TryPurchase(this);
                    break;
                case BuildingState.Purchased:
                    if (!BoardsSmashed)
                        BuildingManager.Instance.TrySmashHit(this);
                    else
                        GameEvents.OnToastRequested("Clear the debris first");
                    break;
                case BuildingState.Cleared:
                    if (BuildingManager.Instance.CanHammer(this))
                        BuildingManager.Instance.TryHammerHit(this);
                    else
                        GameEvents.OnToastRequested(
                            $"Need {timberPerRepair} Timber & {nailsPerRepair} Nails");
                    break;
                case BuildingState.Restored:
                    BuildingManager.Instance.CollectIncome(this);
                    break;
            }
        }
    }

    public void SetState(BuildingState newState)
    {
        State = newState;
        RefreshVisuals();
    }

    public void IncrementSmashHits() => SmashHitsDone++;

    public void SetBoardsSmashed()
    {
        BoardsSmashed = true;
        SmashHitsDone = smashHitsRequired;
    }

    public void SetDebrisRemaining(int count) => DebrisRemaining = count;

    public void OnDebrisDeposited()
    {
        DebrisRemaining--;
        if (DebrisRemaining <= 0)
        {
            DebrisRemaining = 0;
            BuildingManager.Instance?.OnDebrisCleared(this);
        }
    }

    public void OnRepairPointCompleted() => RepairPointsDone++;

    public void ResetRenovation()
    {
        SmashHitsDone = 0;
        BoardsSmashed = false;
        DebrisRemaining = 0;
        RepairPointsDone = 0;
    }

    public void StartPunchScalePublic() => StartPunchScale();

    public void AddDailyIncome()
    {
        UncollectedIncome += dailyIncome;
    }

    public void ResetIncome()
    {
        UncollectedIncome = 0;
    }

    private void RefreshVisuals()
    {
        if (windowLights != null)
            foreach (var light in windowLights)
                light.enabled = State == BuildingState.Restored;

        bool isAbandoned = State == BuildingState.Abandoned;
        if (preBuildVisual != null)
            preBuildVisual.SetActive(isAbandoned);
        if (facadeRenderer != null)
            facadeRenderer.enabled = !isAbandoned;

        if (facadeRenderer != null)
            facadeRenderer.color = State switch
            {
                BuildingState.Abandoned => new Color(0.55f, 0.45f, 0.65f),
                BuildingState.Purchased => new Color(0.75f, 0.55f, 0.35f),
                BuildingState.Cleared => new Color(0.4f, 0.8f, 0.55f),
                BuildingState.Restored => new Color(1f, 0.85f, 0.4f),
                _ => Color.white
            };
    }

    public void FlashWindowLights()
    {
        if (windowLights == null || windowLights.Length == 0) return;
        StartCoroutine(FlashWindowLightsRoutine());
    }

    private IEnumerator FlashWindowLightsRoutine()
    {
        foreach (var light in windowLights)
            light.enabled = false;

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < windowLights.Length; i++)
        {
            windowLights[i].enabled = true;
            if (i < windowLights.Length - 1)
                yield return new WaitForSeconds(0.2f);
        }
    }
}
