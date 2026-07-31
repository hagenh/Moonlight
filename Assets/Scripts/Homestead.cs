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
    [SerializeField] private Sprite builtSprite;
    [SerializeField] private GameObject siteVisual;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _triggerCollider;
    private BuildStage _stage;

    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };
    private static readonly float[] _stageScales = { 1f, 2f, 6f, 6f };

    public bool IsBuilt { get; private set; }
    public BuildStage Stage => _stage;
    public InteractType InteractType => InteractType.Building;
    public bool CanInteract => true;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
            {
                _triggerCollider = col;
                break;
            }
        }
        if (!IsBuilt && _triggerCollider != null)
            _triggerCollider.enabled = false;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
        RefreshSiteVisual();
    }

    public void Interact()
    {
        if (InventoryManager.Instance == null) return;

        switch (_stage)
        {
            case BuildStage.Site:
                if (!InventoryManager.Instance.Has(ContentDb.Stone, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Stone to build the foundation");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Stone, 3);
                AdvanceStage(BuildStage.Foundation, "Foundation built!");
                break;

            case BuildStage.Foundation:
                if (!InventoryManager.Instance.Has(ContentDb.Wood, 3))
                {
                    GameEvents.OnToastRequested("Need 3 Wood to build the frame");
                    return;
                }
                InventoryManager.Instance.TryRemove(ContentDb.Wood, 3);
                AdvanceStage(BuildStage.Frame, "Frame built!");
                break;

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
        if ((int)_stage < _stageScales.Length)
        {
            float s = _stageScales[(int)_stage];
            transform.localScale = new Vector3(s, s, 1f);
            foreach (var c in GetComponents<BoxCollider2D>())
                c.size = new Vector2(0.8f, 1.0f);
        }
        RefreshSiteVisual();
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void RefreshSiteVisual()
    {
        bool showSiteVisual = _stage == BuildStage.Site;
        if (siteVisual != null)
            siteVisual.SetActive(showSiteVisual);
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = !showSiteVisual;
    }

    private void CompleteBuild()
    {
        IsBuilt = true;
        if (_triggerCollider != null)
            _triggerCollider.enabled = true;
        if (_spriteRenderer != null && builtSprite != null)
            _spriteRenderer.sprite = builtSprite;
    }

    public void SetBuiltSpriteForTest(Sprite sprite)
    {
        builtSprite = sprite;
    }
}
