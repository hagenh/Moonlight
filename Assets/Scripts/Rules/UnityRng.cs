using UnityEngine;

public sealed class UnityRng : IRng
{
    public static readonly UnityRng Instance = new();

    public float Value01() => Random.value;

    public int Range(int minInclusive, int maxExclusive) => Random.Range(minInclusive, maxExclusive);

    public float Range(float minInclusive, float maxExclusive) => Random.Range(minInclusive, maxExclusive);
}
