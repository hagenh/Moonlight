using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float realSecondsPerGameMinute = 0.77f;
    [SerializeField] private int dayStartHour = 8;
    [SerializeField] private int dayEndHour = 24;

    private GameClock _clock;

    private GameClock Clock
    {
        get
        {
            if (_clock == null)
                _clock = new GameClock(dayStartHour, dayEndHour);
            return _clock;
        }
    }

    public int Day => Clock.Day;
    public int Hour => Clock.Hour;
    public int Minute => Clock.Minute;
    public float TotalGameMinutes => Clock.TotalGameMinutes;
    public float HourF => Clock.HourF;

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
        _clock = new GameClock(dayStartHour, dayEndHour);
        _lastHour = Hour;
        _lastDay = Day;
    }

    private void Update()
    {
        _fractionalMinute += Time.deltaTime / realSecondsPerGameMinute;

        while (_fractionalMinute >= 1f)
        {
            _fractionalMinute -= 1f;
            Clock.AdvanceMinute();

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
        Clock.AdvanceHour();

        GameEvents.OnHourChanged(Hour, Day);
        _lastHour = Hour;
        _lastDay = Day;

        if (Hour == 0)
            AdvanceToDayEnd();
    }

    public void AdvanceToDayEnd()
    {
        GameEvents.OnDayEnded(Clock.Day);
        Clock.AdvanceToDayEnd();
        _fractionalMinute = 0f;

        GameEvents.OnHourChanged(Hour, Day);
        _lastHour = Hour;
        _lastDay = Day;
        GameEvents.OnToastRequested($"Day {Day} begins");
    }

    public void SetTime(int day, int hour, int minute)
    {
        Clock.SetTime(day, hour, minute);
        _fractionalMinute = 0f;
        _lastHour = Hour;
        _lastDay = Day;
    }
}
