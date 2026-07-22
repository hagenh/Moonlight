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
    private int _targetGuardCount = 1;
    private static Sprite _guardSprite;

    public IReadOnlyList<Guard> ActiveGuards => _activeGuards;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.HeatChanged += OnHeatChanged;
        GameEvents.SleepCompleted += OnSleepCompleted;
        GameEvents.BribePaid += OnBribePaid;
        GameEvents.BribeRefused += OnBribeRefused;
    }

    private void OnDisable()
    {
        GameEvents.HeatChanged -= OnHeatChanged;
        GameEvents.SleepCompleted -= OnSleepCompleted;
        GameEvents.BribePaid -= OnBribePaid;
        GameEvents.BribeRefused -= OnBribeRefused;
    }

    private void Start()
    {
        int suspicion = GameManager.Instance != null ? GameManager.Instance.Heat : 0;
        _targetGuardCount = EconomyRules.GetGuardCountForSuspicion(suspicion);
        SyncGuardCount();
    }

    private void OnHeatChanged(int newHeat, int oldHeat)
    {
        int newCount = EconomyRules.GetGuardCountForSuspicion(newHeat);
        if (newCount != _targetGuardCount)
        {
            _targetGuardCount = newCount;
            SyncGuardCount();
        }
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
        while (_activeGuards.Count < _targetGuardCount)
        {
            int routeIndex = _activeGuards.Count;
            SpawnGuard(routeIndex);
        }
        while (_activeGuards.Count > _targetGuardCount)
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
        sr.sprite = GetGuardSprite();
        sr.color = new Color(0.3f, 0.4f, 0.6f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        var guard = go.AddComponent<Guard>();
        if (route != null) guard.SetWaypoints(route);
        _activeGuards.Add(guard);
    }

    private static Sprite GetGuardSprite()
    {
        if (_guardSprite == null)
        {
            _guardSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f),
                16f);
        }
        return _guardSprite;
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
