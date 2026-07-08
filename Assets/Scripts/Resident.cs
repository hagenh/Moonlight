using System.Collections;
using UnityEngine;

public class Resident : MonoBehaviour, IInteractable
{
    public ResidentDef def;
    public bool HasMovedIn { get; private set; }

    public InteractType InteractType => InteractType.NPC;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _fadeCoroutine;
    private bool _isVisible;

    public static Resident Create(ResidentDef def, Vector3 position)
    {
        var go = new GameObject($"Resident_{def.id}");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f),
            16f);
        sr.color = new Color(def.spriteColor.r, def.spriteColor.g, def.spriteColor.b, 0f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        go.layer = LayerMask.NameToLayer("Interactable");

        var resident = go.AddComponent<Resident>();
        resident.def = def;
        resident._spriteRenderer = sr;
        resident._isVisible = false;

        return resident;
    }

    public void Interact()
    {
        if (def == null || !_isVisible) return;

        string line = def.GetDialogueLine(
            GameManager.Instance != null ? GameManager.Instance.Reputation : 0);

        GameEvents.OnDialogueRequested(def, line);
    }

    public void ShowAt(Vector3 position, float duration = 0.4f)
    {
        transform.position = position;
        _isVisible = true;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeAlpha(0f, 1f, duration));
        GameEvents.OnResidentVisible(def);
    }

    public void Hide(float duration = 0.3f)
    {
        _isVisible = false;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeAlpha(_spriteRenderer.color.a, 0f, duration,
            () => GameEvents.OnResidentHidden(def)));
    }

    public void TeleportTo(Vector3 position, float fadeOutDuration = 0.3f, float fadeInDuration = 0.4f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(TeleportRoutine(position, fadeOutDuration, fadeInDuration));
    }

    public Coroutine WalkTo(Vector3 target, float duration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(WalkRoutine(target, duration));
        return _fadeCoroutine;
    }

    public void SetMovedIn()
    {
        HasMovedIn = true;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetVisibleImmediate(bool visible)
    {
        _isVisible = visible;
        var c = _spriteRenderer.color;
        _spriteRenderer.color = new Color(c.r, c.g, c.b, visible ? 1f : 0f);
    }

    private IEnumerator FadeAlpha(float from, float to, float duration, System.Action onComplete = null)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            var c = _spriteRenderer.color;
            _spriteRenderer.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        var fc = _spriteRenderer.color;
        _spriteRenderer.color = new Color(fc.r, fc.g, fc.b, to);
        _fadeCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator TeleportRoutine(Vector3 target, float fadeOutDuration, float fadeInDuration)
    {
        yield return FadeAlpha(_spriteRenderer.color.a, 0f, fadeOutDuration,
            () => GameEvents.OnResidentHidden(def));

        transform.position = target;

        _isVisible = true;
        yield return FadeAlpha(0f, 1f, fadeInDuration);
        GameEvents.OnResidentVisible(def);
    }

    private IEnumerator WalkRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(start, target, p);
            yield return null;
        }
        transform.position = target;
        _fadeCoroutine = null;
    }
}
