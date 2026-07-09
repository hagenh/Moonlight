using NUnit.Framework;

public class GameClockTests
{
    private GameClock _clock;

    [SetUp]
    public void SetUp()
    {
        _clock = new GameClock(8, 24);
    }

    [Test]
    public void RecalcTotal_Formula()
    {
        _clock.SetTime(3, 5, 15);
        Assert.AreEqual((3 - 1) * 24 * 60 + 5 * 60 + 15, _clock.TotalGameMinutes);
    }

    [Test]
    public void AdvanceMinute_WrapsAt60()
    {
        _clock.SetTime(1, 8, 59);
        _clock.AdvanceMinute();
        Assert.AreEqual(0, _clock.Minute);
        Assert.AreEqual(9, _clock.Hour);
    }

    [Test]
    public void AdvanceMinute_WrapsHourAndDay()
    {
        _clock.SetTime(1, 23, 59);
        _clock.AdvanceMinute();
        Assert.AreEqual(0, _clock.Minute);
        Assert.AreEqual(0, _clock.Hour);
        Assert.AreEqual(2, _clock.Day);
    }

    [Test]
    public void AdvanceHour_WrapsAt24()
    {
        _clock.SetTime(1, 23, 0);
        bool wrapped = _clock.AdvanceHour();
        Assert.AreEqual(0, _clock.Hour);
        Assert.AreEqual(2, _clock.Day);
        Assert.IsTrue(wrapped);
    }

    [Test]
    public void AdvanceHour_NoWrap_ReturnsFalse()
    {
        _clock.SetTime(1, 10, 0);
        bool wrapped = _clock.AdvanceHour();
        Assert.AreEqual(11, _clock.Hour);
        Assert.IsFalse(wrapped);
    }

    [Test]
    public void AdvanceToDayEnd_GoesToNextDayStartHour()
    {
        _clock.SetTime(1, 10, 30);
        _clock.AdvanceToDayEnd();
        Assert.AreEqual(2, _clock.Day);
        Assert.AreEqual(8, _clock.Hour);
        Assert.AreEqual(0, _clock.Minute);
    }

    [Test]
    public void AdvanceToDayEnd_AddsMinutesToTotal()
    {
        _clock.SetTime(1, 10, 0);
        float before = _clock.TotalGameMinutes;
        _clock.AdvanceToDayEnd();
        Assert.Greater(_clock.TotalGameMinutes, before);
    }

    [Test]
    public void SetTime_SetsAndRecalcs()
    {
        _clock.SetTime(5, 14, 30);
        Assert.AreEqual(5, _clock.Day);
        Assert.AreEqual(14, _clock.Hour);
        Assert.AreEqual(30, _clock.Minute);
        Assert.AreEqual((5 - 1) * 24 * 60 + 14 * 60 + 30, _clock.TotalGameMinutes);
    }

    [Test]
    public void Constructor_StartsAtDay1Hour8()
    {
        var clock = new GameClock(8, 24);
        Assert.AreEqual(1, clock.Day);
        Assert.AreEqual(8, clock.Hour);
        Assert.AreEqual(0, clock.Minute);
    }
}
