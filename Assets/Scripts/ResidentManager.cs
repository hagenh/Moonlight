using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentManager : MonoBehaviour
{
    public static ResidentManager Instance { get; private set; }

    [Header("Schedule Markers")]
    [SerializeField] private Transform bertaHomeMarker;
    [SerializeField] private Transform bertaMarketMarker;
    [SerializeField] private Transform bertaWellMarker;

    [Header("Move-in Sequence")]
    [SerializeField] private Transform bertaMoveInStart;
    [SerializeField] private Transform bertaMoveInDoor;
    [SerializeField] private float moveInWalkDuration = 4f;
    [SerializeField] private float moveInLineDelay = 1f;
    [SerializeField] private float moveInLightDelay = 0.5f;

    private readonly Dictionary<string, ResidentDef> _movedInResidents = new();
    private readonly Dictionary<string, Resident> _activeResidents = new();
    private readonly Dictionary<string, Transform> _markers = new();

    private bool _moveInPending;
    private ResidentDef _pendingMoveInDef;
    private Building _pendingMoveInBuilding;

    public bool IsMoveInSequencePlaying { get; private set; }

    public IReadOnlyDictionary<string, ResidentDef> MovedInResidents => _movedInResidents;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _markers["Berta_Home"] = bertaHomeMarker;
        _markers["Berta_Market"] = bertaMarketMarker;
        _markers["Berta_Well"] = bertaWellMarker;
    }

    private void OnEnable()
    {
        GameEvents.HourChanged += OnHourChanged;
        GameEvents.SleepCompleted += OnSleepCompleted;
    }

    private void OnDisable()
    {
        GameEvents.HourChanged -= OnHourChanged;
        GameEvents.SleepCompleted -= OnSleepCompleted;
    }

    private void OnHourChanged(int hour, int day)
    {
        foreach (var kvp in _activeResidents)
        {
            var resident = kvp.Value;
            if (resident.def == null || !resident.HasMovedIn) continue;

            ScheduleEntry? target = null;
            foreach (var entry in resident.def.schedule)
            {
                if (entry.hour == hour)
                {
                    target = entry;
                    break;
                }
            }

            if (target == null) continue;

            Transform marker = GetMarker(target.Value.markerName);
            if (marker == null) continue;

            if (Vector3.Distance(resident.transform.position, marker.position) < 0.1f) continue;

            resident.TeleportTo(marker.position);
            GameEvents.OnResidentTeleported(resident.def, hour);
        }
    }

    public bool RunMoveInChecks()
    {
        if (BuildingManager.Instance == null) return false;

        _moveInPending = false;
        _pendingMoveInDef = null;
        _pendingMoveInBuilding = null;

        foreach (var building in BuildingManager.Instance.Buildings)
        {
            if (building.State != BuildingState.Restored) continue;

            ResidentDef def = FindResidentForBuilding(building.BuildingName);
            if (def == null)
                continue;

            if (_movedInResidents.ContainsKey(def.id)) continue;
            if (_activeResidents.ContainsKey(def.id)) continue;

            _movedInResidents[def.id] = def;

            _moveInPending = true;
            _pendingMoveInDef = def;
            _pendingMoveInBuilding = building;

            GameEvents.OnResidentMovedIn(def, building);

            break;
        }

        return _moveInPending;
    }

    public void PlayPendingMoveInSequence()
    {
        if (!_moveInPending || _pendingMoveInDef == null || _pendingMoveInBuilding == null) return;

        StartCoroutine(MoveInSequenceRoutine(_pendingMoveInDef, _pendingMoveInBuilding));

        _moveInPending = false;
        _pendingMoveInDef = null;
        _pendingMoveInBuilding = null;
    }

    private IEnumerator MoveInSequenceRoutine(ResidentDef def, Building building)
    {
        IsMoveInSequencePlaying = true;
        GameEvents.OnMoveInSequenceStarted(def, building);

        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = true;

        if (_activeResidents.TryGetValue(def.id, out var existingResident))
        {
            Destroy(existingResident.gameObject);
            _activeResidents.Remove(def.id);
        }

        Vector3 startPos = bertaMoveInStart != null
            ? bertaMoveInStart.position
            : building.transform.position + new Vector3(-3f, 0f, 0f);
        var resident = Resident.Create(def, startPos);
        _activeResidents[def.id] = resident;
        resident.SetMovedIn();

        resident.ShowAt(startPos, 0.5f);
        yield return new WaitForSeconds(0.6f);

        GameEvents.OnToastRequested("*cart wheels creaking*");
        yield return new WaitForSeconds(1.0f);

        Vector3 doorPos = bertaMoveInDoor != null
            ? bertaMoveInDoor.position
            : building.transform.position + new Vector3(-0.5f, 0f, 0f);
        resident.WalkTo(doorPos, moveInWalkDuration);
        yield return new WaitForSeconds(moveInWalkDuration + 0.2f);

        yield return new WaitForSeconds(moveInLineDelay);
        GameEvents.OnDialogueRequested(def, def.moveInLine);

        yield return WaitForDialogueClose();

        yield return new WaitForSeconds(moveInLightDelay);
        building.FlashWindowLights();
        GameEvents.OnToastRequested($"{def.displayName} has moved into {building.BuildingName}!");

        resident.Hide(0.3f);
        yield return new WaitForSeconds(0.4f);

        Transform homeMarker = GetMarker("Berta_Home");
        if (homeMarker != null)
        {
            resident.SetPosition(homeMarker.position);
            resident.SetVisibleImmediate(true);
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.IsMenuOpen = false;

        IsMoveInSequencePlaying = false;
        GameEvents.OnMoveInSequenceCompleted(def, building);
    }

    private bool _dialogueIsOpen;

    private IEnumerator WaitForDialogueClose()
    {
        _dialogueIsOpen = true;

        System.Action handler = null;
        handler = () =>
        {
            _dialogueIsOpen = false;
            GameEvents.DialogueClosed -= handler;
        };
        GameEvents.DialogueClosed += handler;

        while (_dialogueIsOpen)
            yield return null;
    }

    private void OnSleepCompleted(int newDay)
    {
        SpawnResidentsForCurrentHour();
    }

    private void SpawnResidentsForCurrentHour()
    {
        if (TimeManager.Instance == null) return;
        int currentHour = TimeManager.Instance.Hour;

        foreach (var kvp in _movedInResidents)
        {
            var def = kvp.Value;

            if (_activeResidents.ContainsKey(def.id)) continue;

            if (_moveInPending && _pendingMoveInDef != null && def.id == _pendingMoveInDef.id) continue;

            string targetMarker = null;
            foreach (var entry in def.schedule)
            {
                if (entry.hour <= currentHour)
                    targetMarker = entry.markerName;
            }

            if (targetMarker == null) continue;

            Transform marker = GetMarker(targetMarker);
            if (marker == null) continue;

            var resident = Resident.Create(def, marker.position);
            resident.SetMovedIn();
            resident.SetVisibleImmediate(true);
            _activeResidents[def.id] = resident;
        }
    }

    private Transform GetMarker(string markerName)
    {
        if (_markers.TryGetValue(markerName, out var t)) return t;

        var go = GameObject.Find(markerName);
        if (go != null)
        {
            _markers[markerName] = go.transform;
            return go.transform;
        }

        Debug.LogWarning($"ResidentManager: no marker named '{markerName}' found in scene.");
        return null;
    }

    private ResidentDef FindResidentForBuilding(string buildingName)
    {
        if (ContentDb.Berta != null && ContentDb.Berta.buildingId == buildingName)
            return ContentDb.Berta;
        if (ContentDb.Instance != null)
        {
            foreach (var kvp in ContentDb.Instance.Residents)
            {
                if (kvp.Value.buildingId == buildingName)
                    return kvp.Value;
            }
        }
        return null;
    }

    public void ForceBertaMoveIn()
    {
        if (ContentDb.Instance == null || BuildingManager.Instance == null) return;

        var def = ContentDb.Berta;
        if (def == null) return;

        Building building = null;
        foreach (var b in BuildingManager.Instance.Buildings)
        {
            if (b.BuildingName == "Bakery") { building = b; break; }
        }
        if (building == null) return;

        if (building.State != BuildingState.Restored)
            BuildingManager.Instance.ForceCompleteRepair(building);

        if (_movedInResidents.ContainsKey(def.id)) return;

        _movedInResidents[def.id] = def;
        GameEvents.OnResidentMovedIn(def, building);

        SpawnResidentsForCurrentHour();
        GameEvents.OnToastRequested("Berta forced move-in!");
    }

    public void ForceBertaMoveInSequence()
    {
        if (ContentDb.Instance == null || BuildingManager.Instance == null) return;

        var def = ContentDb.Berta;
        Building building = null;
        foreach (var b in BuildingManager.Instance.Buildings)
        {
            if (b.BuildingName == "Bakery") { building = b; break; }
        }
        if (building == null) return;

        if (building.State != BuildingState.Restored)
            BuildingManager.Instance.ForceCompleteRepair(building);

        if (!_movedInResidents.ContainsKey(def.id))
        {
            _movedInResidents[def.id] = def;
            GameEvents.OnResidentMovedIn(def, building);
        }

        if (_activeResidents.TryGetValue(def.id, out var existing))
        {
            Destroy(existing.gameObject);
            _activeResidents.Remove(def.id);
        }

        StartCoroutine(MoveInSequenceRoutine(def, building));
    }

    public bool IsResidentMovedIn(string residentId)
    {
        return _movedInResidents.ContainsKey(residentId);
    }

    public void ResetResident(string residentId)
    {
        _movedInResidents.Remove(residentId);

        if (_activeResidents.TryGetValue(residentId, out var resident))
        {
            Destroy(resident.gameObject);
            _activeResidents.Remove(residentId);
        }

        GameEvents.OnToastRequested($"Resident {residentId} reset.");
    }
}
