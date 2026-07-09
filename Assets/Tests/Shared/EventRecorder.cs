using System.Collections.Generic;

namespace Lamplight.TestSupport
{
    public sealed class EventRecorder
    {
        private readonly List<string> _order = new();
        public IReadOnlyList<string> Order => _order;
        public int Count => _order.Count;

        public void Record(string eventName)
        {
            _order.Add(eventName);
        }

        public void Record(string eventName, object payload)
        {
            _order.Add($"{eventName}: {payload}");
        }

        public void Clear() => _order.Clear();

        public string[] ToArray() => _order.ToArray();

        public string Sequence => string.Join(" -> ", _order);
    }
}
