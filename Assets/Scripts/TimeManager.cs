using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float realSecondsPerGameMinute = 0.77f;
    [SerializeField] private int dayStartHour = 8;
    [SerializeField] private int dayEndHour = 24;

    public int Day { get; private set; } = 1;
    public int Hour { get; private set; } = 8;
    public int Minute { get; private set; }
    public float TotalGameMinutes { get; private set; }
    public float HourF => Hour + Minute / 60f;

    private float _fractionalMinute;
    private int _lastHour;
    private int _lastDay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        RecalcTotal();
        _lastHour = Hour;
        _lastDay = Day;
    }

    private void Update()
    {
        _fractionalMinute += Time.deltaTime / realSecondsPerGameMinute;

        while (_fractionalMinute >= 1f)
        {
            _fractionalMinute -= 1f;
            Minute++;
            TotalGameMinutes++;

            if (Minute >= 60)
            {
                Minute = 0;
                Hour++;

                if (Hour >= 24)
                {
                    Hour = 0;
                    Day++;
                }
            }

            if (Hour == 23 && Minute == 0)
                GameEvents.OnToastRequested("Getting late...");

            if (Hour == 0 && Minute == 0)
            {
                GameEvents.OnCurfewReached(Day);
                return;
            }
        }

        if (Hour != _lastHour || Day != _lastDay)
        {
            GameEvents.OnHourChanged(Hour, Day);
            _lastHour = Hour;
            _lastDay = Day;
        }
    }

    public void AdvanceHour()
    {
        TotalGameMinutes += 60;
        Hour++;
        if (Hour >= 24)
        {
            Hour = 0;
            Day++;
        }

        GameEvents.OnHourChanged(Hour, Day);
        _lastHour = Hour;
        _lastDay = Day;

        if (Hour == 0)
            AdvanceToDayEnd();
    }

    public void AdvanceToDayEnd()
    {
        int minutesUntilEnd = (dayEndHour - Hour) * 60 - Minute;
        if (minutesUntilEnd > 0)
            TotalGameMinutes += minutesUntilEnd;

        GameEvents.OnDayEnded(Day);
        Day++;
        Hour = dayStartHour;
        Minute = 0;
        _fractionalMinute = 0f;
        RecalcTotal();

        GameEvents.OnHourChanged(Hour, Day);
        _lastHour = Hour;
        _lastDay = Day;
        GameEvents.OnToastRequested($"Day {Day} begins");
    }

    public void SetTime(int day, int hour, int minute)
    {
        Day = day;
        Hour = hour;
        Minute = minute;
        _fractionalMinute = 0f;
        RecalcTotal();
        _lastHour = Hour;
        _lastDay = Day;
    }

    private void RecalcTotal()
    {
        TotalGameMinutes = (Day - 1) * 24 * 60 + Hour * 60 + Minute;
    }
}
