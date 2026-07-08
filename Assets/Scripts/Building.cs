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
    public string buildingName = "Bakery";
    public int purchaseCost = 100;
    public int dailyIncome = 20;

    [Header("Renovation")]
    public bool isFacadeOnly = false;
    public int smashHitsRequired = 3;
    public int debrisCount = 3;
    public int totalRepairPoints = 3;
    public int timberPerRepair = 1;
    public int nailsPerRepair = 1;
    public float hammerDuration = 2f;

    [Header("Visuals")]
    [SerializeField] private Light2D[] windowLights;

    [SerializeField] private SpriteRenderer facadeRenderer;

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
        _punchCoroutine = StartCoroutine(PunchScale());
    }

    private IEnumerator PunchScale()
    {
        var vt = VisualTransform;
        Vector3 baseScale = vt.localScale;
        float t;

        t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.08f);
            vt.localScale = new Vector3(
                Mathf.Lerp(baseScale.x, baseScale.x * 0.7f, p),
                Mathf.Lerp(baseScale.y, baseScale.y * 1.3f, p),
                baseScale.z
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            vt.localScale = new Vector3(
                Mathf.Lerp(baseScale.x * 0.7f, baseScale.x * 1.15f, p),
                Mathf.Lerp(baseScale.y * 1.3f, baseScale.y * 0.9f, p),
                baseScale.z
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            vt.localScale = new Vector3(
                Mathf.Lerp(baseScale.x * 1.15f, baseScale.x, p),
                Mathf.Lerp(baseScale.y * 0.9f, baseScale.y, p),
                baseScale.z
            );
            yield return null;
        }

        vt.localScale = baseScale;
        _punchCoroutine = null;
    }

    public void Interact()
    {
        if (BuildingManager.Instance == null) return;

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
        else if (LastHitTrigger == doorTrigger)
        {
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
