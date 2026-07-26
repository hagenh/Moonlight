using UnityEngine;
using UnityEngine.InputSystem;

public class RequestBookUI : MonoBehaviour
{
    private bool _visible;
    private Vector2 _scrollPos;
    private Rect _windowRect = new Rect(0, 0, 420, 460);

    private void OnEnable()
    {
        GameEvents.RequestBookRequested += OnRequestBookRequested;
        GameEvents.MenuCloseRequested += Close;
    }

    private void OnDisable()
    {
        GameEvents.RequestBookRequested -= OnRequestBookRequested;
        GameEvents.MenuCloseRequested -= Close;
    }

    private void OnRequestBookRequested()
    {
        _visible = true;
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        _windowRect.x = (Screen.width - _windowRect.width) / 2;
        _windowRect.y = (Screen.height - _windowRect.height) / 2;
    }

    private void Update()
    {
        if (_visible && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
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
        _windowRect = GUI.Window(3, _windowRect, DrawWindow, "The Request Book");
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));

        if (StandManager.Instance == null) return;

        var book = StandManager.Instance.Book;
        GUILayout.Label($"{book.Active.Count} of {book.SlotCount} slots used");
        GUILayout.Space(6);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(360));

        if (book.Active.Count == 0)
            GUILayout.Label("Nothing yet. Notes arrive overnight.");

        for (int i = book.Active.Count - 1; i >= 0; i--)
        {
            var request = book.Active[i];
            DrawRequest(request);
            GUILayout.Space(10);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);
        if (GUILayout.Button("Close"))
            Close();
    }

    private void DrawRequest(StandRequest request)
    {
        GUILayout.Label($"\"{request.Text}\"");
        GUILayout.Label($"— {request.Signature}");

        foreach (var item in request.Accepts)
        {
            int have = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetCount(item) : 0;
            int payment = RequestBookRules.Payment(request, item);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{request.Units}x {item.displayName}  ({payment}g)  have: {have}",
                GUILayout.Width(280));

            GUI.enabled = have >= request.Units;
            if (GUILayout.Button("Fill", GUILayout.Width(60)))
                StandManager.Instance.TryFill(request.Id, item);
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Decline", GUILayout.Width(80)))
            StandManager.Instance.Decline(request.Id);
    }
}
