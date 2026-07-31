using System.Collections;
using UnityEngine;

public class FermentVat : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer vatRenderer;

    public VatState State { get; private set; } = VatState.Empty;
    public FermentBatch CurrentBatch { get; private set; }

    public InteractType InteractType => InteractType.FermentVat;
    public bool CanInteract => true;

    private Coroutine _punchCoroutine;

    private void Start() => RefreshVisuals();

    private void OnEnable()
    {
        if (FermentManager.Instance != null)
            FermentManager.Instance.Register(this);
        GameEvents.VatStateChanged += OnVatStateChanged;
    }

    private void OnDisable()
    {
        if (FermentManager.Instance != null)
            FermentManager.Instance.Unregister(this);
        GameEvents.VatStateChanged -= OnVatStateChanged;
    }

    public void Interact()
    {
        if (FermentManager.Instance == null) return;

        switch (State)
        {
            case VatState.Empty:
                GameEvents.OnRecipeSelectionRequested(this);
                break;
            case VatState.Fermenting:
                GameEvents.OnToastRequested(
                    $"Fermenting... {CurrentBatch?.Progress * 100:F0}%");
                break;
            case VatState.Ready:
                if (PlayerController.Instance != null && PlayerController.Instance.IsCarryingAnything)
                    GameEvents.OnToastRequested("Already carrying something");
                else
                    FermentManager.Instance.TryCollectBatchAsCrate(this);
                break;
        }
    }

    public void SetBatch(FermentBatch batch)
    {
        CurrentBatch = batch;
        State = VatState.Fermenting;
        RefreshVisuals();
    }

    public void MarkReady()
    {
        State = VatState.Ready;
        RefreshVisuals();
        StartPunchScale();
    }

    public void ClearBatch()
    {
        CurrentBatch = null;
        State = VatState.Empty;
        RefreshVisuals();
    }

    private void OnVatStateChanged(FermentVat vat, VatState oldState, VatState newState)
    {
        if (vat == this) StartPunchScale();
    }

    private void RefreshVisuals()
    {
        if (vatRenderer != null)
        {
            vatRenderer.color = State switch
            {
                VatState.Empty => new Color(0.6f, 0.6f, 0.6f),
                VatState.Fermenting => new Color(0.5f, 0.7f, 1f),
                VatState.Ready => new Color(0.4f, 1f, 0.5f),
                _ => Color.white
            };
        }
    }

    private void StartPunchScale()
    {
        if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
        _punchCoroutine = StartCoroutine(JuiceUtils.PunchScale(transform));
    }
}
