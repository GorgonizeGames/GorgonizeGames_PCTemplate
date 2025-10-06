using UnityEngine;
using Game.Runtime.Core.DI;

namespace Game.Runtime.Core.Extensions
{
    /// <summary>
    /// Extension methods for MonoBehaviour
    /// Provides convenient access to common functionality
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Inject dependencies into this MonoBehaviour
        /// Scans for [Inject] attributes and resolves services from the DI container
        /// </summary>
        public static void InjectDependencies(this MonoBehaviour obj)
        {
            Dependencies.Inject(obj);
        }
        
        /// <summary>
        /// Try to inject dependencies, returns false if any required dependency is missing
        /// </summary>
        public static bool TryInjectDependencies(this MonoBehaviour obj)
        {
            try
            {
                Dependencies.Inject(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get a service from the DI container
        /// </summary>
        public static T GetService<T>(this MonoBehaviour obj) where T : class
        {
            return Dependencies.Container.Resolve<T>();
        }
        
        /// <summary>
        /// Try to get a service from the DI container
        /// </summary>
        public static bool TryGetService<T>(this MonoBehaviour obj, out T service) where T : class
        {
            return Dependencies.Container.TryResolve(out service);
        }
        
        /// <summary>
        /// Check if a service is registered in the DI container
        /// </summary>
        public static bool HasService<T>(this MonoBehaviour obj) where T : class
        {
            return Dependencies.Container.IsRegistered<T>();
        }
    }
}