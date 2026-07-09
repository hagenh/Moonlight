using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class JuiceUtilsTests
{
    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator PunchScale_RestoresBaseScale_OnCompletion()
    {
        var go = TestBootstrap.CreateGameObject("JuiceTest");
        go.transform.localScale = new Vector3(2f, 3f, 1f);
        Vector3 baseScale = go.transform.localScale;

        yield return JuiceUtils.PunchScale(go.transform);

        Assert.AreEqual(baseScale, go.transform.localScale);
    }

    [UnityTest]
    public IEnumerator PunchScale_ChangesScaleDuringAnimation()
    {
        var go = TestBootstrap.CreateGameObject("JuiceTest");
        go.transform.localScale = Vector3.one;
        Vector3 baseScale = go.transform.localScale;

        var runner = go.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(JuiceUtils.PunchScale(go.transform));

        for (int i = 0; i < 3; i++)
            yield return null;

        Assert.AreNotEqual(baseScale, go.transform.localScale);

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(baseScale, go.transform.localScale);
    }

    [UnityTest]
    public IEnumerator PunchScale_PreservesNonUniformBaseScale()
    {
        var go = TestBootstrap.CreateGameObject("JuiceTest");
        Vector3 nonUniform = new Vector3(1.5f, 0.8f, 1f);
        go.transform.localScale = nonUniform;

        yield return JuiceUtils.PunchScale(go.transform);

        Assert.AreEqual(nonUniform, go.transform.localScale);
    }
}

internal class CoroutineRunner : MonoBehaviour { }
