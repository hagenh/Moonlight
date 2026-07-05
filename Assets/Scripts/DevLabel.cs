using TMPro;
using UnityEngine;

public class DevLabel : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;
    [SerializeField] private Building building;

    private void Start() => Refresh();

    private void OnEnable() => GameEvents.BuildingStateChanged += OnBuildingStateChanged;

    private void OnDisable() => GameEvents.BuildingStateChanged -= OnBuildingStateChanged;

    private void OnBuildingStateChanged(Building b, BuildingState _, BuildingState __)
    {
        if (b == building) Refresh();
    }

    private void Refresh()
    {
        if (label == null || building == null) return;

        string priceInfo = building.State switch
        {
            BuildingState.Abandoned => $"Buy: {building.purchaseCost}g",
            BuildingState.Cleared => $"Repair: {building.repairCost}g",
            BuildingState.Restored => $"+{building.dailyIncome}g/day",
            _ => ""
        };

        label.text = $"{building.buildingName}\n{building.State}\n{priceInfo}";
    }
}
