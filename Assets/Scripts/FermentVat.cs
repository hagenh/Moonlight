using System.Collections;
using UnityEngine;

public class FermentVat : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer vatRenderer;

    public VatState State { get; private set; } = VatState.Empty;
    public FermentBatch CurrentBatch { get; private set; }

    public InteractType InteractType => InteractType.FermentVat;

    private Coroutine _punchCoroutine;

    private void Start() => RefreshVisuals();

    private void OnEnable()
    {
        if (FermentManager.Instance != null)
            FermentManager.Instance.Register(this);
        GameEvents.VatStateChanged += OnVatStateChanged;
        GameEvents.BatchProgressed += OnBatchProgressed;
    }

    private void OnDisable()
    {
        if (FermentManager.Instance != null)
            FermentManager.Instance.Unregister(this);
        GameEvents.VatStateChanged -= OnVatStateChanged;
        GameEvents.BatchProgressed -= OnBatchProgressed;
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
                FermentManager.Instance.TryCollectBatch(this);
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

    private void OnBatchProgressed(FermentVat vat, float progress)
    {
        if (vat == this) RefreshVisuals();
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
        _punchCoroutine = StartCoroutine(PunchScale());
    }

    private IEnumerator PunchScale()
    {
        float t;

        t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.08f);
            transform.localScale = new Vector3(
                Mathf.Lerp(1f, 0.7f, p),
                Mathf.Lerp(1f, 1.3f, p),
                1f
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            transform.localScale = new Vector3(
                Mathf.Lerp(0.7f, 1.15f, p),
                Mathf.Lerp(1.3f, 0.9f, p),
                1f
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            transform.localScale = new Vector3(
                Mathf.Lerp(1.15f, 1f, p),
                Mathf.Lerp(0.9f, 1f, p),
                1f
            );
            yield return null;
        }

        transform.localScale = Vector3.one;
        _punchCoroutine = null;
    }
}
