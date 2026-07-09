using System.Collections.Generic;

namespace Lamplight.TestSupport.Fakes
{
    public sealed class StubRng : IRng
    {
        private readonly Queue<float> _values = new();

        public StubRng(params float[] values)
        {
            foreach (var v in values) _values.Enqueue(v);
        }

        public float Value01()
        {
            return _values.Count > 0 ? _values.Dequeue() : 0f;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            float t = Value01();
            return minInclusive + (int)(t * (maxExclusive - minInclusive));
        }

        public float Range(float minInclusive, float maxExclusive)
        {
            float t = Value01();
            return minInclusive + t * (maxExclusive - minInclusive);
        }
    }
}
