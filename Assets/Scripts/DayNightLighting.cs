using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLighting : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;

    private readonly (float hour, Color color, float intensity)[] _keyframes = new[]
    {
        (0f,    new Color(0.1f, 0.1f, 0.3f), 0.2f),
        (5f,    new Color(0.1f, 0.1f, 0.3f), 0.2f),
        (6f,    new Color(1f, 0.5f, 0.2f), 0.4f),
        (7f,    new Color(1f, 0.7f, 0.3f), 0.7f),
        (8f,    new Color(1f, 0.9f, 0.7f), 1f),
        (12f,   new Color(1f, 1f, 0.9f), 1.1f),
        (16f,   new Color(1f, 1f, 0.9f), 1.1f),
        (17f,   new Color(1f, 0.9f, 0.6f), 1f),
        (19f,   new Color(1f, 0.6f, 0.2f), 0.8f),
        (21f,   new Color(0.7f, 0.3f, 0.5f), 0.5f),
        (23f,   new Color(0.1f, 0.1f, 0.3f), 0.2f),
        (24f,   new Color(0.1f, 0.1f, 0.3f), 0.2f),
    };

    private void Update()
    {
        if (globalLight == null || TimeManager.Instance == null) return;

        float hour = TimeManager.Instance.HourF;

        int i = 0;
        while (i < _keyframes.Length - 1 && _keyframes[i + 1].hour <= hour)
            i++;

        if (i >= _keyframes.Length - 1)
        {
            globalLight.color = _keyframes[^1].color;
            globalLight.intensity = _keyframes[^1].intensity;
            return;
        }

        var from = _keyframes[i];
        var to = _keyframes[i + 1];
        float t = (hour - from.hour) / (to.hour - from.hour);

        globalLight.color = Color.Lerp(from.color, to.color, t);
        globalLight.intensity = Mathf.Lerp(from.intensity, to.intensity, t);
    }
}
