using UnityEngine;

public class Homestead : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite builtSprite;

    public bool IsBuilt { get; private set; }

    public InteractType InteractType => InteractType.Building;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetBuilt()
    {
        IsBuilt = true;
        if (_spriteRenderer != null && builtSprite != null)
            _spriteRenderer.sprite = builtSprite;
    }

    public void Interact() { }

    public void SetBuiltSpriteForTest(Sprite sprite)
    {
        builtSprite = sprite;
    }
}
