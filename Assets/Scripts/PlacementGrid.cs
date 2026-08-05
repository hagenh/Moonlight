using System.Collections.Generic;
using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    public static PlacementGrid Instance { get; private set; }

    [SerializeField] private Grid grid;
    [SerializeField] private float cellSizeMultiplier = 1f;

    private readonly Dictionary<Vector3Int, PlacedInfrastructure> _occupied = new();

    internal void SetGrid(Grid g) => grid = g;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // The Grid's own cellSize doesn't match the game's actual tile size here:
    // Ground/Collision/SurfaceMap render at half that size via their own transform
    // scale (see FootstepPlayer, which reads tiles through that same 0.5 scale).
    // cellSizeMultiplier corrects for that so placement snaps to real tiles.
    public Vector3 CellSize => grid.cellSize * cellSizeMultiplier;

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = grid.transform.InverseTransformPoint(worldPos);
        Vector3 size = CellSize;
        return new Vector3Int(Mathf.FloorToInt(local.x / size.x), Mathf.FloorToInt(local.y / size.y), 0);
    }

    public Vector3 CellCenterWorld(Vector3Int cell)
    {
        Vector3 size = CellSize;
        Vector3 local = new Vector3((cell.x + 0.5f) * size.x, (cell.y + 0.5f) * size.y, 0f);
        return grid.transform.TransformPoint(local);
    }

    public bool IsAreaFree(Vector3Int origin, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = origin + new Vector3Int(x, y, 0);
                if (_occupied.ContainsKey(cell)) return false;

                Vector2 center = CellCenterWorld(cell);
                Vector2 size = (Vector2)CellSize * 0.8f;
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
