using UnityEngine;

public enum BuildStage
{
    Site = 0,
    Foundation = 1,
    Frame = 2,
    Walls = 3
}

public class Homestead : MonoBehaviour, IInteractable
{
    public const int FoundationCost = 20;
    public const int FrameCost = 12;

    [SerializeField] private Sprite builtSprite;
    [SerializeField] internal Collider2D signTrigger;
    [SerializeField] internal Collider2D doorTrigger;
    private SpriteRenderer _spriteRenderer;
    private BuildStage _stage;
    private int _stoneDeposited;
    private int _woodDeposited;

    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };

    public bool IsBuilt { get; private set; }
    public BuildStage Stage => _stage;
    public int StoneDeposited => _stoneDeposited;
    public int WoodDeposited => _woodDeposited;
    public InteractType InteractType => InteractType.Building;
    public bool CanInteract => true;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (doorTrigger != null)
            doorTrigger.enabled = false;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
    }

    public void Interact()
    {
        if (InventoryManager.Instance == null) return;

        switch (_stage)
        {
            case BuildStage.Site:
            {
                int needed = FoundationCost - _stoneDeposited;
                int carried = InventoryManager.Instance.GetCount(ContentDb.Stone);
                int toDeposit = Mathf.Min(needed, carried);
                if (toDeposit <= 0)
                {
                    GameEvents.OnToastRequested("Need Stone to keep building");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Stone, toDeposit);
                _stoneDeposited += toDeposit;
                if (_stoneDeposited >= FoundationCost)
                    AdvanceStage(BuildStage.Foundation, "Foundation built!");
                break;
            }

            case BuildStage.Foundation:
            {
                int needed = FrameCost - _woodDeposited;
                int carried = InventoryManager.Instance.GetCount(ContentDb.Wood);
                int toDeposit = Mathf.Min(needed, carried);
                if (toDeposit <= 0)
                {
                    GameEvents.OnToastRequested("Need Wood to keep building");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, toDeposit);
                _woodDeposited += toDeposit;
                if (_woodDeposited >= FrameCost)
                    AdvanceStage(BuildStage.Frame, "Frame built!");
                break;
            }

            case BuildStage.Frame:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 2) ||
                    !InventoryManager.Instance.Has(ContentDb.Nails, 3))
                {
                    GameEvents.OnToastRequested("Need 2 Wood and 3 Nails to build the walls");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 2);
                InventoryManager.Instance.TryRemove(ContentDb.Nails, 3);
                AdvanceStage(BuildStage.Walls, "Homestead built!");
                CompleteBuild();
                break;

            case BuildStage.Walls:
                break;
        }
    }

    private void AdvanceStage(BuildStage newStage, string toast)
    {
        _stage = newStage;
        if (_spriteRenderer != null && (int)_stage < _stageColors.Length)
            _spriteRenderer.color = _stageColors[(int)_stage];
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void CompleteBuild()
    {
        IsBuilt = true;
        if (signTrigger != null)
            signTrigger.enabled = false;
        if (doorTrigger != null)
            doorTrigger.enabled = true;
        if (_spriteRenderer != null && builtSprite != null)
            _spriteRenderer.sprite = builtSprite;
    }

    public void SetBuiltSpriteForTest(Sprite sprite)
    {
        builtSprite = sprite;
    }
}
