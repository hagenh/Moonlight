using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    private bool _visible;
    private ResidentDef _currentDef;
    private string _currentLine;
    private string _displayedText;
    private int _charIndex;
    private float _typewriterTimer;
    private bool _typewriterComplete;

    [SerializeField] private float typewriterSpeed = 0.03f;

    private void OnEnable()
    {
        GameEvents.DialogueRequested += OnDialogueRequested;
        GameEvents.DialogueClosed += OnDialogueClosed;
        GameEvents.MenuCloseRequested += OnMenuCloseRequested;
    }

    private void OnDisable()
    {
        GameEvents.DialogueRequested -= OnDialogueRequested;
        GameEvents.DialogueClosed -= OnDialogueClosed;
        GameEvents.MenuCloseRequested -= OnMenuCloseRequested;
    }

    private void Update()
    {
        if (!_visible) return;

        if (!_typewriterComplete)
        {
            _typewriterTimer += Time.deltaTime;
            while (_typewriterTimer >= typewriterSpeed && _charIndex < _currentLine.Length)
            {
                _typewriterTimer -= typewriterSpeed;
                _charIndex++;
            }
            _displayedText = _currentLine.Substring(0, _charIndex);
            if (_charIndex >= _currentLine.Length)
                _typewriterComplete = true;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!_typewriterComplete)
            {
                _displayedText = _currentLine;
                _charIndex = _currentLine.Length;
                _typewriterComplete = true;
            }
            else
            {
                Close();
            }
        }
    }

    private void OnDialogueRequested(ResidentDef def, string line)
    {
        _currentDef = def;
        _currentLine = line;
        _displayedText = "";
        _charIndex = 0;
        _typewriterTimer = 0f;
        _typewriterComplete = false;
        _visible = true;

        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;
    }

    private void OnDialogueClosed()
    {
        Close();
    }

    private void OnMenuCloseRequested()
    {
        if (_visible) Close();
    }

    private void Close()
    {
        _visible = false;
        _currentDef = null;
        _currentLine = "";
        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;
    }

    private void OnGUI()
    {
        if (!_visible || _currentDef == null) return;

        float w = 400f;
        float h = 100f;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - h - 40f;

        GUI.Box(new Rect(x, y, w, h), "");

        Color oldColor = GUI.color;
        GUI.color = _currentDef.portraitColor;
        GUI.DrawTexture(new Rect(x + 8, y + 8, 60, 60), Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUI.Label(new Rect(x + 8, y + 72, 60, 20), _currentDef.displayName);
        GUI.Label(new Rect(x + 78, y + 12, w - 90, h - 24), _displayedText);

        if (_typewriterComplete)
            GUI.Label(new Rect(x + w - 80, y + h - 20, 70, 18), "[E] Close");
    }
}
