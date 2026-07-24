using UnityEngine;

[System.Serializable]
public class DirectionalClip
{
    public Sprite[] down;
    public Sprite[] up;
    public Sprite[] left;
    public Sprite[] right;
    public float framesPerSecond = 8f;
    public bool loop = true;

    public Sprite[] GetSprites(FacingDirection facing)
    {
        return facing switch
        {
            FacingDirection.Down => down,
            FacingDirection.Up => up,
            FacingDirection.Left => left,
            FacingDirection.Right => right,
            _ => down
        };
    }
}
