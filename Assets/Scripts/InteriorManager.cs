using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteriorManager : MonoBehaviour
{
    public static InteriorManager Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.4f;

    internal static bool SkipDefaultBuild;

    private Image _fadeImage;
    private bool _isInside;
    private bool _isTransitioning;
    private Vector2 _exteriorPosition;
    private Color _exteriorBgColor;

    public bool IsInside => _isInside;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateFadeImage();
    }

    // --- Transition API ---

    public void EnterInterior(Building b)
    {
        if (b == null) return;
        bool found = TryResolveSpawn(b, out Vector3 spawn);
        EnterInterior(spawn, found);
    }

    public void EnterInterior(Vector3 spawn) => EnterInterior(spawn, true);

    private void EnterInterior(Vector3 spawn, bool found)
    {
        if (_isInside || _isTransitioning) return;
        if (!found) { GameEvents.OnToastRequested("It's empty inside..."); return; }
        if (PlayerController.Instance == null) return;
        _exteriorPosition = PlayerController.Instance.RB.position;
        var cam = Camera.main;
        if (cam != null) _exteriorBgColor = cam.backgroundColor;
        StartCoroutine(TransitionRoutine(spawn, enter: true));
    }

    public void ExitInterior()
    {
        if (!_isInside || _isTransitioning) return;
        if (PlayerController.Instance == null) return;
        StartCoroutine(TransitionRoutine(_exteriorPosition, enter: false));
    }

    private bool TryResolveSpawn(Building b, out Vector3 spawn)
    {
        if (b.InteriorSpawn != null) { spawn = b.InteriorSpawn.position; return true; }
        var found = GameObject.Find(b.BuildingName + "_InteriorSpawn");
        if (found != null) { spawn = found.transform.position; return true; }
        spawn = Vector3.zero;
        return false;
    }

    private IEnumerator TransitionRoutine(Vector3 target, bool enter)
    {
        _isTransitioning = true;
        var player = PlayerController.Instance;
        bool wasCarryingCrate = player.IsCarryingCrate;
        Crate carriedCrate = player.CarriedCrate;
        if (wasCarryingCrate) player.DropCrate();
        player.IsMenuOpen = true;
        player.ForceIdle();
        yield return FadeToBlack();
        player.RB.position = target;
        if (wasCarryingCrate && carriedCrate != null)
        {
            player.PickUpCrate(carriedCrate);
            player.ChangeState(new Player.States.CarryState(player));
        }
        _isInside = enter;

        var cam = Camera.main;
        if (cam != null)
        {
            if (enter) cam.backgroundColor = Color.black;
            else cam.backgroundColor = _exteriorBgColor;
        }

        yield return FadeFromBlack();
        player.IsMenuOpen = false;
        _isTransitioning = false;
    }

    // --- Fade (mirrors SleepManager pattern, self-contained) ---

    private void CreateFadeImage()
    {
        var go = new GameObject("InteriorFade");
        go.transform.SetParent(transform);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        var imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(go.transform, false);
        _fadeImage = imgGo.AddComponent<Image>();
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = false;
        var rt = _fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeToBlack()
    {
        if (_fadeImage == null) yield break;
        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / fadeDuration));
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
            _fadeImage.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        _fadeImage.raycastTarget = false;
    }
}
