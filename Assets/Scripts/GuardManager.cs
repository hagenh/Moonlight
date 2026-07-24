using System.Collections.Generic;
using UnityEngine;

public class GuardManager : MonoBehaviour
{
    public static GuardManager Instance { get; private set; }

    private readonly List<Guard> _activeGuards = new();

    public IReadOnlyList<Guard> ActiveGuards => _activeGuards;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.SleepCompleted += OnSleepCompleted;
        GameEvents.BribePaid += OnBribePaid;
        GameEvents.BribeRefused += OnBribeRefused;
    }

    private void OnDisable()
    {
        GameEvents.SleepCompleted -= OnSleepCompleted;
        GameEvents.BribePaid -= OnBribePaid;
        GameEvents.BribeRefused -= OnBribeRefused;
    }

    private void Start()
    {
        FindAllGuards();
    }

    private void FindAllGuards()
    {
        _activeGuards.AddRange(FindObjectsByType<Guard>(FindObjectsSortMode.None));
    }

    private void OnSleepCompleted(int newDay)
    {
        foreach (var guard in _activeGuards)
        {
            if (guard != null)
                guard.ResetToStart();
        }
    }

    private void OnBribePaid()
    {
        bool resolved = false;
        foreach (var guard in _activeGuards)
        {
            if (guard == null || !guard.IsCaught) continue;
            if (!resolved)
            {
                guard.OnBribePaid();
                resolved = true;
            }
            else
            {
                guard.ClearCaught();
            }
        }
    }

    private void OnBribeRefused()
    {
        bool resolved = false;
        foreach (var guard in _activeGuards)
        {
            if (guard == null || !guard.IsCaught) continue;
            if (!resolved)
            {
                guard.OnBribeRefused();
                resolved = true;
            }
            else
            {
                guard.ClearCaught();
            }
        }
    }
}
