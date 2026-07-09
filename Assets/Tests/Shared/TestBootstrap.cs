using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lamplight.TestSupport
{
    public static class TestBootstrap
    {
        private static readonly List<GameObject> _tracked = new();

        public static T CreateSingleton<T>() where T : MonoBehaviour
        {
            var go = new GameObject(typeof(T).Name);
            _tracked.Add(go);
            var comp = go.AddComponent<T>();
            EnsureInstanceSet(comp);
            return comp;
        }

        private static void EnsureInstanceSet<T>(T comp) where T : MonoBehaviour
        {
            var prop = typeof(T).GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);
            if (prop != null)
            {
                var setter = prop.GetSetMethod(nonPublic: true);
                setter?.Invoke(null, new object[] { comp });
            }
        }

        public static GameObject CreateGameObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            _tracked.Add(go);
            return go;
        }

        public static T AddComponent<T>(GameObject parent = null) where T : MonoBehaviour
        {
            var go = parent ?? CreateGameObject(typeof(T).Name);
            if (!_tracked.Contains(go)) _tracked.Add(go);
            return go.AddComponent<T>();
        }

        public static void Track(GameObject go)
        {
            if (!_tracked.Contains(go)) _tracked.Add(go);
        }

        public static void DestroyAll()
        {
            foreach (var go in _tracked)
            {
                if (go != null)
                {
                    ClearInstanceFields(go);
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
            _tracked.Clear();
        }

        private static void ClearInstanceFields(GameObject go)
        {
            foreach (var comp in go.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                var prop = comp.GetType().GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    var setter = prop.GetSetMethod(nonPublic: true);
                    setter?.Invoke(null, new object[] { null });
                }
            }
        }

        public static void DestroyAllDelayed()
        {
            foreach (var go in _tracked)
            {
                if (go != null) UnityEngine.Object.Destroy(go);
            }
            _tracked.Clear();
        }
    }
}
