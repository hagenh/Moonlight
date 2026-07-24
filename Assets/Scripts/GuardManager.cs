using System.Collections.Generic;
using UnityEngine;

public class GuardManager : MonoBehaviour
{
    public static GuardManager Instance { get; private set; }

    [Header("Patrol Routes")]
    [SerializeField] private Transform[] route0Waypoints;
    [SerializeField] private Transform[] route1Waypoints;
    [SerializeField] private Transform[] route2Waypoints;
    [SerializeField] private Transform[] route3Waypoints;

    private readonly List<Guard> _activeGuards = new();
    [SerializeField] private int guardCount = 1;

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
        SyncGuardCount();
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

    private void SyncGuardCount()
    {
        while (_activeGuards.Count < guardCount)
        {
            int routeIndex = _activeGuards.Count;
            SpawnGuard(routeIndex);
        }
        while (_activeGuards.Count > guardCount)
        {
            DespawnGuard();
        }
    }

    private void SpawnGuard(int routeIndex)
    {
        var go = new GameObject($"Guard_{_activeGuards.Count}");
        var route = GetRoute(routeIndex);
        if (route != null && route.Length > 0 && route[0] != null)
            go.transform.position = route[0].position;
        else
            go.transform.position = Vector3.zero;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var animator = go.AddComponent<DirectionalSpriteAnimator>();
        animator.animationSet = ContentDb.Instance != null ? ContentDb.Instance.GuardAnimations : null;
        if (animator.animationSet != null) animator.Initialize();

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        var guard = go.AddComponent<Guard>();
        if (route != null) guard.SetWaypoints(route);
        _activeGuards.Add(guard);
    }

    private void DespawnGuard()
    {
        if (_activeGuards.Count == 0) return;
        int last = _activeGuards.Count - 1;
        if (_activeGuards[last] != null)
            Destroy(_activeGuards[last].gameObject);
        _activeGuards.RemoveAt(last);
    }

    private Transform[] GetRoute(int index)
    {
        return index switch
        {
            0 => route0Waypoints,
            1 => route1Waypoints,
            2 => route2Waypoints,
            3 => route3Waypoints,
            _ => route0Waypoints
        };
    }
}
