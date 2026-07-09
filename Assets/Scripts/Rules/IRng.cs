public interface IRng
{
    float Value01();
    int Range(int minInclusive, int maxExclusive);
    float Range(float minInclusive, float maxExclusive);
}
