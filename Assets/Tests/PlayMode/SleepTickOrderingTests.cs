using System.Collections;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SleepTickOrderingTests
{
    private GameManager _gameManager;
    private TimeManager _timeManager;
    private SleepManager _sleepManager;
    private ResidentManager _residentManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();

        _gameManager = TestBootstrap.CreateSingleton<GameManager>();
        _timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        _residentManager = TestBootstrap.CreateSingleton<ResidentManager>();
        _sleepManager = TestBootstrap.CreateSingleton<SleepManager>();

        _recorder = new EventRecorder();

        GameEvents.SleepInitiated += (day) => _recorder.Record("SleepInitiated", day);
        GameEvents.DayEnded += (day) => _recorder.Record("DayEnded", day);
        GameEvents.HourChanged += (hour, day) => _recorder.Record("HourChanged", $"{hour}/{day}");
        GameEvents.ResidentMovedIn += (def, b) => _recorder.Record("ResidentMovedIn");
        GameEvents.SleepCompleted += (newDay) => _recorder.Record("SleepCompleted", newDay);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator BeginSleep_FiresEventsInCorrectOrder()
    {
        _timeManager.SetTime(1, 10, 0);
        _recorder.Clear();

        _sleepManager.BeginSleep();

        for (int i = 0; i < 5; i++)
            yield return null;

        int sleepInitiatedIdx = FindFirstIndexStartingWith("SleepInitiated");
        int dayEndedIdx = FindFirstIndexStartingWith("DayEnded");
        int sleepCompletedIdx = FindFirstIndexStartingWith("SleepCompleted");

        Assert.GreaterOrEqual(sleepInitiatedIdx, 0, "SleepInitiated should have fired");
        Assert.GreaterOrEqual(dayEndedIdx, 0, "DayEnded should have fired");
        Assert.GreaterOrEqual(sleepCompletedIdx, 0, "SleepCompleted should have fired");

        Assert.Less(sleepInitiatedIdx, dayEndedIdx, "SleepInitiated must fire before DayEnded");
        Assert.Less(dayEndedIdx, sleepCompletedIdx, "DayEnded must fire before SleepCompleted");
    }

    [UnityTest]
    public IEnumerator BeginSleep_AdvancesDay()
    {
        _timeManager.SetTime(1, 10, 0);

        _sleepManager.BeginSleep();

        for (int i = 0; i < 5; i++)
            yield return null;

        Assert.AreEqual(2, _timeManager.Day, "Day should advance after sleep");
    }

    [UnityTest]
    public IEnumerator BeginSleep_WhenAlreadySleeping_DoesNothing()
    {
        _timeManager.SetTime(1, 10, 0);

        _sleepManager.BeginSleep();
        yield return null;

        _recorder.Clear();

        _sleepManager.BeginSleep();
        yield return null;

        Assert.AreEqual(0, _recorder.Count, "Second BeginSleep should not fire any additional events");
    }

    private int FindFirstIndexStartingWith(string prefix)
    {
        for (int i = 0; i < _recorder.Count; i++)
        {
            if (_recorder.Order[i].StartsWith(prefix))
                return i;
        }
        return -1;
    }
}
