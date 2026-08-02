using UnityEngine;
using UnityEngine.Tilemaps;

public enum FootstepSurface
{
    Dirt,
    Sand,
    Stone,
    Water
}

[CreateAssetMenu(fileName = "FootstepTile", menuName = "Tiles/FootstepTile")]
public class FootstepTile : TileBase
{
    public Sprite sprite;
    public FootstepSurface Surface = FootstepSurface.Dirt;
    public Tile.ColliderType colliderType = Tile.ColliderType.Sprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.colliderType = colliderType;
    }
}
