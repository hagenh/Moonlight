using UnityEngine;

public static class FacingMath
{
    public static FacingDirection FromVector(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        if (angle < 45f || angle >= 315f)
            return FacingDirection.Right;
        else if (angle < 135f)
            return FacingDirection.Up;
        else if (angle < 225f)
            return FacingDirection.Left;
        else
            return FacingDirection.Down;
    }

    public static Vector2 GetFacingOffset(FacingDirection facing)
    {
        return facing switch
        {
            FacingDirection.Down => new Vector2(0, -0.5f),
            FacingDirection.Up => new Vector2(0, 0.5f),
            FacingDirection.Left => new Vector2(-0.5f, 0),
            FacingDirection.Right => new Vector2(0.5f, 0),
            _ => Vector2.zero
        };
    }
}
