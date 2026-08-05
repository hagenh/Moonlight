using System.Collections.Generic;
using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    public static PlacementGrid Instance { get; private set; }

    [SerializeField] private Grid grid;

    private readonly Dictionary<Vector3Int, PlacedInfrastructure> _occupied = new();

    internal void SetGrid(Grid g) => grid = g;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Vector3Int WorldToCell(Vector3 worldPos) => grid.WorldToCell(worldPos);

    public Vector3 CellCenterWorld(Vector3Int cell) => grid.GetCellCenterWorld(cell);

    public Vector3 CellSize => grid.cellSize;

    public bool IsAreaFree(Vector3Int origin, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = origin + new Vector3Int(x, y, 0);
                if (_occupied.ContainsKey(cell)) return false;

                Vector2 center = grid.GetCellCenterWorld(cell);
                Vector2 size = (Vector2)grid.cellSize * 0.8f;
                var hits = Physics2D.OverlapBoxAll(center, size, 0f);
                foreach (var hit in hits)
                    if (!hit.isTrigger) return false;
            }
        }

        return true;
    }

    public void Reserve(Vector3Int origin, int width, int height, PlacedInfrastructure instance)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _occupied[origin + new Vector3Int(x, y, 0)] = instance;

        instance.OriginCell = origin;
        instance.FootprintWidth = width;
        instance.FootprintHeight = height;
    }
}
