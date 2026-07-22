using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerEventTests
{
    private GameManager _gameManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _recorder = new EventRecorder();

        GameEvents.CashChanged += (cash) => _recorder.Record("CashChanged", cash);
        GameEvents.RepChanged += (newRep, oldRep) => _recorder.Record("RepChanged", $"{newRep}/{oldRep}");
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator TrySpend_FiresCashChanged()
    {
        bool result = _gameManager.TrySpend(100);

        Assert.IsTrue(result);
        Assert.AreEqual(400, _gameManager.Cash);
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("CashChanged"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TrySpend_Insufficient_DoesNotFireEvent()
    {
        bool result = _gameManager.TrySpend(1000);

        Assert.IsFalse(result);
        Assert.AreEqual(500, _gameManager.Cash);
        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator AddCash_FiresCashChanged()
    {
        _gameManager.AddCash(50);

        Assert.AreEqual(550, _gameManager.Cash);
        Assert.AreEqual(1, _recorder.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetReputation_FiresRepChanged()
    {
        _gameManager.SetReputation(10);

        Assert.AreEqual(10, _gameManager.Reputation);
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("RepChanged"));
        yield return null;
    }
}
