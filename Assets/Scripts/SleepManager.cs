using System.Collections;
using UnityEngine;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    [SerializeField] private int heatDecayPerNight = 5;
    [SerializeField] private float fadeDuration = 0.5f;

    private Canvas _fadeCanvas;
    private UnityEngine.UI.Image _fadeImage;
    private bool _isSleeping;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _fadeCanvas = GetComponentInChildren<Canvas>();
        if (_fadeCanvas != null)
            _fadeImage = _fadeCanvas.GetComponentInChildren<UnityEngine.UI.Image>();
    }

    private void OnEnable()
    {
        GameEvents.CurfewReached += OnCurfewReached;
    }

    private void OnDisable()
    {
        GameEvents.CurfewReached -= OnCurfewReached;
    }

    private void OnCurfewReached(int day)
    {
        if (!_isSleeping)
            BeginSleep();
    }

    public void BeginSleep()
    {
        if (_isSleeping) return;
        _isSleeping = true;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ForceIdle();
            PlayerController.Instance.IsMenuOpen = true;
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        yield return FadeToBlack();

        int day = TimeManager.Instance != null ? TimeManager.Instance.Day : 0;
        GameEvents.OnSleepInitiated(day);

        if (TimeManager.Instance != null)
            TimeManager.Instance.AdvanceToDayEnd();

        if (GameManager.Instance != null)
            GameManager.Instance.AddHeat(-heatDecayPerNight);

        bool moveInPending = false;
        Debug.Log($"[Sleep] Move-in check — ResidentManager.Instance = {ResidentManager.Instance != null}");
        if (ResidentManager.Instance != null)
            moveInPending = ResidentManager.Instance.RunMoveInChecks();

        int newDay = TimeManager.Instance != null ? TimeManager.Instance.Day : day + 1;
        GameEvents.OnSleepCompleted(newDay);

        yield return FadeFromBlack();

        if (moveInPending && ResidentManager.Instance != null)
        {
            yield return null;
            ResidentManager.Instance.PlayPendingMoveInSequence();
            yield return new WaitWhile(() => ResidentManager.Instance.IsMoveInSequencePlaying);
        }

        _isSleeping = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private IEnumerator FadeToBlack()
    {
        if (_fadeImage == null) yield break;
        _fadeImage.color = new Color(0, 0, 0, 0);
        _fadeImage.raycastTarget = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            _fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        _fadeImage.color = Color.black;
    }

    private IEnumerator FadeFromBlack()
    {
        if (_fadeImage == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeDuration);
            _fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        _fadeImage.color = new Color(0, 0, 0, 0);
        _fadeImage.raycastTarget = false;
    }
}
