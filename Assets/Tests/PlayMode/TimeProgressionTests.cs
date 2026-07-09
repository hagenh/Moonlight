using System.Collections;
using System.Reflection;
using Lamplight.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TimeProgressionTests
{
    private TimeManager _timeManager;
    private EventRecorder _recorder;

    [SetUp]
    public void SetUp()
    {
        GameEventsReset.ClearAll();
        _timeManager = TestBootstrap.CreateSingleton<TimeManager>();
        _recorder = new EventRecorder();

        GameEvents.HourChanged += (hour, day) => _recorder.Record("HourChanged", $"{hour}/{day}");
        GameEvents.DayEnded += (day) => _recorder.Record("DayEnded", day);
        GameEvents.CurfewReached += (day) => _recorder.Record("CurfewReached", day);
        GameEvents.ToastRequested += (msg) => _recorder.Record("Toast", msg);
    }

    [TearDown]
    public void TearDown()
    {
        TestBootstrap.DestroyAll();
        GameEventsReset.ClearAll();
    }

    [UnityTest]
    public IEnumerator AdvanceToDayEnd_FiresDayEndedThenHourChanged()
    {
        _timeManager.SetTime(1, 10, 30);
        _recorder.Clear();

        _timeManager.AdvanceToDayEnd();

        Assert.AreEqual(2, _timeManager.Day);
        Assert.AreEqual(8, _timeManager.Hour);
        Assert.AreEqual(0, _timeManager.Minute);
        Assert.GreaterOrEqual(_recorder.Count, 2);
        Assert.IsTrue(_recorder.Order[0].StartsWith("DayEnded"));
        Assert.IsTrue(_recorder.Order[1].StartsWith("HourChanged"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator AdvanceHour_FiresHourChanged()
    {
        _timeManager.SetTime(1, 10, 0);
        _recorder.Clear();

        _timeManager.AdvanceHour();

        Assert.AreEqual(11, _timeManager.Hour);
        Assert.AreEqual(1, _recorder.Count);
        Assert.IsTrue(_recorder.Order[0].StartsWith("HourChanged"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator AdvanceHour_AtMidnight_CascadesToDayEnd()
    {
        _timeManager.SetTime(1, 23, 0);
        _recorder.Clear();

        _timeManager.AdvanceHour();

        Assert.IsTrue(_recorder.Count >= 2);
        Assert.IsTrue(_recorder.Order[0].StartsWith("HourChanged"));
        bool foundDayEnded = false;
        foreach (var entry in _recorder.Order)
        {
            if (entry.StartsWith("DayEnded")) foundDayEnded = true;
        }
        Assert.IsTrue(foundDayEnded);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetTime_DoesNotFireEvents()
    {
        _recorder.Clear();

        _timeManager.SetTime(3, 14, 30);

        Assert.AreEqual(3, _timeManager.Day);
        Assert.AreEqual(14, _timeManager.Hour);
        Assert.AreEqual(30, _timeManager.Minute);
        Assert.AreEqual(0, _recorder.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetTime_RecalcsTotalGameMinutes()
    {
        _timeManager.SetTime(3, 14, 30);

        float expected = (3 - 1) * 24 * 60 + 14 * 60 + 30;
        Assert.AreEqual(expected, _timeManager.TotalGameMinutes);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_AdvancesGameMinutes()
    {
        var field = typeof(TimeManager).GetField("realSecondsPerGameMinute",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_timeManager, 0.001f);

        _timeManager.SetTime(1, 8, 0);
        float before = _timeManager.TotalGameMinutes;

        yield return new WaitForSeconds(0.1f);

        Assert.Greater(_timeManager.TotalGameMinutes, before);
    }
}
