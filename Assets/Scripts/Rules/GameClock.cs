public sealed class GameClock
{
    public int Day { get; private set; }
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public float TotalGameMinutes { get; private set; }

    private readonly int _dayStartHour;
    private readonly int _dayEndHour;

    public float HourF => Hour + Minute / 60f;

    public GameClock(int dayStartHour, int dayEndHour)
    {
        _dayStartHour = dayStartHour;
        _dayEndHour = dayEndHour;
        Day = 1;
        Hour = dayStartHour;
        RecalcTotal();
    }

    public void RecalcTotal()
    {
        TotalGameMinutes = (Day - 1) * 24 * 60 + Hour * 60 + Minute;
    }

    public bool AdvanceMinute()
    {
        Minute++;
        TotalGameMinutes++;

        bool hourChanged = false;
        if (Minute >= 60)
        {
            Minute = 0;
            Hour++;
            hourChanged = true;

            if (Hour >= 24)
            {
                Hour = 0;
                Day++;
            }
        }
        return hourChanged;
    }

    public bool AdvanceHour()
    {
        TotalGameMinutes += 60;
        Hour++;
        bool wrappedToMidnight = false;
        if (Hour >= 24)
        {
            Hour = 0;
            Day++;
            wrappedToMidnight = true;
        }
        return wrappedToMidnight;
    }

    public void AdvanceToDayEnd()
    {
        int minutesUntilEnd = (_dayEndHour - Hour) * 60 - Minute;
        if (minutesUntilEnd > 0)
            TotalGameMinutes += minutesUntilEnd;

        Day++;
        Hour = _dayStartHour;
        Minute = 0;
        RecalcTotal();
    }

    public void SetTime(int day, int hour, int minute)
    {
        Day = day;
        Hour = hour;
        Minute = minute;
        RecalcTotal();
    }
}
