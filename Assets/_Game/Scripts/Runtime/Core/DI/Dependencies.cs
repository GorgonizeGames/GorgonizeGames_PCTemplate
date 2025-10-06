using System;
using System.Reflection;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    /// <summary>
    /// Static dependency injection helper
    /// Simple, clean, gets the job done
    /// </summary>
    public static class Dependencies
    {
        private static DIContainer _container;
        
        public static DIContainer Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new DIContainer();
                    Debug.Log("[Dependencies] Container created");
                }
                return _container;
            }
        }

        /// <summary>
        /// Inject dependencies into a MonoBehaviour
        /// Scans for [Inject] fields and fills them
        /// </summary>
        public static void Inject(MonoBehaviour obj)
        {
            if (obj == null) return;

            Type objType = obj.GetType();
            FieldInfo[] fields = objType.GetFields(
                BindingFlags.NonPublic | 
                BindingFlags.Public | 
                BindingFlags.Instance);

            int injected = 0;
            
            foreach (FieldInfo field in fields)
            {
                InjectAttribute attr = field.GetCustomAttribute<InjectAttribute>();
                if (attr == null) continue;

                // Skip if already set
                if (field.GetValue(obj) != null) continue;

                // Try to resolve service
                MethodInfo resolveMethod = typeof(DIContainer)
                    .GetMethod(nameof(DIContainer.TryResolve))
                    ?.MakeGenericMethod(field.FieldType);

                if (resolveMethod == null) continue;

                object[] parameters = new object[] { null };
                bool success = (bool)resolveMethod.Invoke(Container, parameters);

                if (success && parameters[0] != null)
                {
                    field.SetValue(obj, parameters[0]);
                    injected++;
                }
                else if (attr.Required)
                {
                    Debug.LogError(
                        $"[Dependencies] Required dependency '{field.FieldType.Name}' " +
                        $"not found for '{objType.Name}'", obj);
                }
            }

            if (injected > 0)
            {
                Debug.Log($"[Dependencies] Injected {injected} dependencies into {objType.Name}");
            }
        }

        /// <summary>
        /// Reset container (for scene reloads, tests)
        /// </summary>
        public static void Reset()
        {
            _container?.Clear();
            _container = null;
        }
    }

    /// <summary>
    /// Mark fields for automatic injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute 
    {
        public bool Required { get; set; }
        
        public InjectAttribute(bool required = true)
        {
            Required = required;
        }
    }
}