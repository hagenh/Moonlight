using System;

namespace Lamplight.TestSupport.Fakes
{
    public sealed class SeededRng : IRng
    {
        private readonly System.Random _random;

        public SeededRng(int seed)
        {
            _random = new System.Random(seed);
        }

        public float Value01()
        {
            return (float)_random.NextDouble();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxExclusive)
        {
            return minInclusive + (float)_random.NextDouble() * (maxExclusive - minInclusive);
        }
    }
}
