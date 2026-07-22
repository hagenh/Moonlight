using UnityEngine;
using UnityEngine.InputSystem;

public class BribeUI : MonoBehaviour
{
    private bool _visible;
    private int _cost;
    private Rect _windowRect = new Rect(0, 0, 300, 120);

    private void OnEnable()
    {
        GameEvents.CaughtBribe += OnCaughtBribe;
        GameEvents.BribePaid += OnBribePaid;
        GameEvents.BribeRefused += OnBribeRefused;
    }

    private void OnDisable()
    {
        GameEvents.CaughtBribe -= OnCaughtBribe;
        GameEvents.BribePaid -= OnBribePaid;
        GameEvents.BribeRefused -= OnBribeRefused;
    }

    private void OnCaughtBribe(int cost)
    {
        _cost = cost;
        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void OnBribePaid()
    {
        Close();
    }

    private void OnBribeRefused()
    {
        Close();
    }

    private void Update()
    {
        if (!_visible) return;
        if (Keyboard.current.eKey.wasPressedThisFrame)
            GameEvents.OnBribePaid();
        else if (Keyboard.current.qKey.wasPressedThisFrame)
            GameEvents.OnBribeRefused();
    }

    private void Close()
    {
        _visible = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }
    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUI.Window(3, _windowRect, DrawWindow, "Caught!");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        int cash = GameManager.Instance != null ? GameManager.Instance.Cash : 0;
        bool canAfford = cash >= _cost;

        GUILayout.Label($"A guard caught you carrying moonshine!");
        GUILayout.Label($"Pay {_cost}g? (You have {cash}g)");

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        GUI.enabled = canAfford;
        if (GUILayout.Button("[E] Pay", GUILayout.Width(120)))
            GameEvents.OnBribePaid();
        GUI.enabled = true;

        if (GUILayout.Button("[Q] Refuse", GUILayout.Width(120)))
            GameEvents.OnBribeRefused();
        GUILayout.EndHorizontal();
    }
}
