using UnityEngine;

public class HomesteadFillGrid : MonoBehaviour
{
    private const int Columns = 5;
    private const int Rows = 4;
    private const float GridWidth = 2.4f;
    private const float GridHeight = 2.4f;
    private const float OriginX = -GridWidth / 2f + (GridWidth / Columns) / 2f;
    private const float OriginY = -GridHeight / 2f + (GridHeight / Rows) / 2f;

    private SpriteRenderer[] _cells;

    public int CellCount => EnsureCells().Length;

    private void Awake() => EnsureCells();

    private SpriteRenderer[] EnsureCells()
    {
        if (_cells != null) return _cells;

        _cells = new SpriteRenderer[Columns * Rows];
        float spacingX = GridWidth / Columns;
        float spacingY = GridHeight / Rows;

        for (int i = 0; i < _cells.Length; i++)
        {
            var cellGo = new GameObject($"Cell{i}");
            cellGo.transform.SetParent(transform, false);
            int col = i % Columns;
            int row = i / Columns;
            cellGo.transform.localPosition = new Vector3(
                OriginX + col * spacingX,
                OriginY + row * spacingY,
                -0.01f);
            var sr = cellGo.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            _cells[i] = sr;
        }

        return _cells;
    }

    public void SetFilled(int filledCount, Sprite sprite)
    {
        var cells = EnsureCells();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].sprite = sprite;
            cells[i].enabled = i < filledCount;
        }
    }
}
