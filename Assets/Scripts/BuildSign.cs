using UnityEngine;

public enum BuildStage
{
    Site = 0,
    Foundation = 1,
    Frame = 2,
    Walls = 3
}

public class BuildSign : MonoBehaviour, IInteractable
{
    [SerializeField] internal Building homesteadBuilding;

    private SpriteRenderer _spriteRenderer;
    private BuildStage _stage;
    private static readonly Color[] _stageColors = {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.55f, 0.35f, 0.15f),
        new Color(0.8f, 0.7f, 0.5f),
    };

    public BuildStage Stage => _stage;
    public InteractType InteractType => InteractType.Building;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _spriteRenderer.color = _stageColors[0];
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
        GameEvents.OnHomesteadBuildStageChanged((int)_stage);
        GameEvents.OnToastRequested(toast);
    }

    private void CompleteBuild()
    {
        if (homesteadBuilding != null)
        {
            var facade = homesteadBuilding.transform.Find("Square");
            if (facade != null)
                facade.gameObject.SetActive(false);
            homesteadBuilding.gameObject.SetActive(true);
            homesteadBuilding.SetState(BuildingState.Restored);
        }
        gameObject.SetActive(false);
    }

    public static BuildSign Create(Vector3 position, Building homestead = null)
    {
        var go = new GameObject("BuildSign");
        go.transform.position = position;

        var tex = new Texture2D(16, 16);
        var pixels = new Color32[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels32(pixels);
        tex.Apply();

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, 16, 16),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(0.7f, 0.6f, 0.4f);
        sr.sortingOrder = 5;

        var solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(0.8f, 1.0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.0f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var sign = go.AddComponent<BuildSign>();
        sign._spriteRenderer = sr;
        sign.homesteadBuilding = homestead;

        return sign;
    }
}
