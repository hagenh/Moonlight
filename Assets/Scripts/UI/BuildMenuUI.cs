using UnityEngine;
using UnityEngine.InputSystem;

public class BuildMenuUI : MonoBehaviour
{
    private bool _visible;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 360, 420);

    private InputSystem_Actions _input;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        GameEvents.MenuCloseRequested += Close;
        _input.Menus.Enable();
        _input.Menus.BuildMenu.performed += OnBuildMenuKey;
    }

    private void OnDisable()
    {
        GameEvents.MenuCloseRequested -= Close;
        _input.Menus.BuildMenu.performed -= OnBuildMenuKey;
        _input.Menus.Disable();
    }

    private void OnBuildMenuKey(InputAction.CallbackContext _)
    {
        if (_visible) Close();
        else Open();
    }

    private void Open()
    {
        if (BuildModeController.Instance != null)
            BuildModeController.Instance.Cancel();

        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void Close()
    {
        if (!_visible) return;
        _visible = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        _windowRect = GUI.Window(4, _windowRect, DrawWindow, "Build");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (InfrastructureManager.Instance == null) return;

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(320));

        var entries = InfrastructureManager.Instance.Book.Entries;
        bool anyAvailable = false;

        foreach (var entry in entries)
        {
            if (entry.Available <= 0) continue;
            anyAvailable = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{entry.Item.displayName} (x{entry.Available})", GUILayout.Width(220));

            GUI.enabled = BuildModeController.Instance != null;
            if (GUILayout.Button("Place", GUILayout.Width(80)))
            {
                BuildModeController.Instance.Enter(entry.Item);
                Close();
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        if (!anyAvailable)
            GUILayout.Label("Nothing to build yet.");

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }
}
