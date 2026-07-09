using System.Collections;
using UnityEngine;

public static class JuiceUtils
{
    public static IEnumerator PunchScale(Transform target)
    {
        Vector3 baseScale = target.localScale;
        float t;

        t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.08f);
            target.localScale = new Vector3(
                Mathf.Lerp(baseScale.x, baseScale.x * 0.7f, p),
                Mathf.Lerp(baseScale.y, baseScale.y * 1.3f, p),
                baseScale.z
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            target.localScale = new Vector3(
                Mathf.Lerp(baseScale.x * 0.7f, baseScale.x * 1.15f, p),
                Mathf.Lerp(baseScale.y * 1.3f, baseScale.y * 0.9f, p),
                baseScale.z
            );
            yield return null;
        }

        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.1f);
            target.localScale = new Vector3(
                Mathf.Lerp(baseScale.x * 1.15f, baseScale.x, p),
                Mathf.Lerp(baseScale.y * 0.9f, baseScale.y, p),
                baseScale.z
            );
            yield return null;
        }

        target.localScale = baseScale;
    }
}
