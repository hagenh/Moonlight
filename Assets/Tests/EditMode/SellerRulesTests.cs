using NUnit.Framework;

public class SellerRulesTests
{
    private const int TormodArrive = 18;
    private const int TormodLeave = 6;

    [Test]
    public void Tormod_IsAbsent_AtWakeUp()
    {
        Assert.IsFalse(SellerRules.IsPresent(8, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_IsAbsent_WhenTheFirstFermentFinishes()
    {
        Assert.IsFalse(SellerRules.IsPresent(11, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_ArrivesAtDusk()
    {
        Assert.IsTrue(SellerRules.IsPresent(18, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_IsStillPresent_BeforeCurfew()
    {
        Assert.IsTrue(SellerRules.IsPresent(23, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_IsPresent_AcrossMidnight()
    {
        Assert.IsTrue(SellerRules.IsPresent(0, TormodArrive, TormodLeave));
        Assert.IsTrue(SellerRules.IsPresent(5, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_IsGone_AtDawn()
    {
        Assert.IsFalse(SellerRules.IsPresent(6, TormodArrive, TormodLeave));
    }

    [Test]
    public void Tormod_IsAbsent_TheFollowingMorning()
    {
        Assert.IsFalse(SellerRules.IsPresent(7, TormodArrive, TormodLeave));
    }

    [Test]
    public void DaytimeWindow_DoesNotWrap()
    {
        // The traveling cart's old 10-to-18 window, for the non-wrapping branch.
        Assert.IsFalse(SellerRules.IsPresent(9, 10, 18));
        Assert.IsTrue(SellerRules.IsPresent(10, 10, 18));
        Assert.IsTrue(SellerRules.IsPresent(17, 10, 18));
        Assert.IsFalse(SellerRules.IsPresent(18, 10, 18));
    }

    [Test]
    public void ZeroLengthWindow_MeansNeverPresent()
    {
        for (int hour = 0; hour < 24; hour++)
            Assert.IsFalse(SellerRules.IsPresent(hour, 12, 12), $"hour {hour}");
    }

    [Test]
    public void OutOfRangeHours_AreWrappedIntoRange()
    {
        Assert.IsTrue(SellerRules.IsPresent(24, TormodArrive, TormodLeave), "24:00 is 00:00");
        Assert.IsFalse(SellerRules.IsPresent(-16, TormodArrive, TormodLeave), "-16 is 08:00");
    }

    [Test]
    public void SomeoneIsAlwaysEitherPresentOrAbsent_ForEveryHour()
    {
        // Guards against a window that silently swallows an hour.
        for (int hour = 0; hour < 24; hour++)
        {
            bool present = SellerRules.IsPresent(hour, TormodArrive, TormodLeave);
            bool expected = hour >= TormodArrive || hour < TormodLeave;
            Assert.AreEqual(expected, present, $"hour {hour}");
        }
    }
}
