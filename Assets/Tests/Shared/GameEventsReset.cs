using System.Reflection;
using UnityEngine;

namespace Lamplight.TestSupport
{
    public static class GameEventsReset
    {
        public static void ClearAll()
        {
            var fields = typeof(GameEvents)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(System.MulticastDelegate) ||
                    typeof(System.MulticastDelegate).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(null, null);
                }
            }
        }
    }
}
