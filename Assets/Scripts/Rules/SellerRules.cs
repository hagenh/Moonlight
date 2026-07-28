/// <summary>
/// Scheduling for sellers who keep hours.
///
/// Windows may wrap past midnight — Tormod arrives at dusk and leaves at dawn,
/// so his window is 18:00 to 06:00 and spans the day boundary.
/// </summary>
public static class SellerRules
{
    /// <summary>
    /// True when a seller keeping <paramref name="arriveHour"/> to
    /// <paramref name="leaveHour"/> should be present at <paramref name="hour"/>.
    /// Arrival is inclusive, departure exclusive: an 18-to-6 seller is present at
    /// 18:00 and 05:00, absent at 06:00.
    ///
    /// Hours outside 0-23 are wrapped into range. A zero-length window
    /// (arrive == leave) means the seller never appears.
    /// </summary>
    public static bool IsPresent(int hour, int arriveHour, int leaveHour)
    {
        hour = NormalizeHour(hour);
        arriveHour = NormalizeHour(arriveHour);
        leaveHour = NormalizeHour(leaveHour);

        if (arriveHour == leaveHour) return false;

        if (arriveHour < leaveHour)
            return hour >= arriveHour && hour < leaveHour;

        // Window wraps midnight.
        return hour >= arriveHour || hour < leaveHour;
    }

    private static int NormalizeHour(int hour) => ((hour % 24) + 24) % 24;
}
