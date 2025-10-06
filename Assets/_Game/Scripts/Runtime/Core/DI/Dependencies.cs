using System;
using System.Reflection;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    /// <summary>
    /// Static wrapper for dependency injection
    /// Provides easy access to the DI container
    /// </summary>
    public static class Dependencies
    {
        private static readonly object _lock = new object();
        private static DIContainer _container;
        
        public static DIContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (_lock)
                    {
                        if (_container == null)
                        {
                            _container = new DIContainer();
                            Debug.Log("[Dependencies] ✅ Container created");
                        }
                    }
                }
                return _container;
            }
        }
        
        /// <summary>
        /// Inject dependencies into a MonoBehaviour
        /// Scans for [Inject] attributes and resolves services
        /// </summary>
        public static void Inject(MonoBehaviour obj)
        {
            if (obj == null)
            {
                Debug.LogError("[Dependencies] Cannot inject into null object");
                return;
            }
            
            try
            {
                Type objType = obj.GetType();
                FieldInfo[] fields = objType.GetFields(
                    BindingFlags.NonPublic | 
                    BindingFlags.Public |
                    BindingFlags.Instance);
                
                int injectedCount = 0;
                int skippedCount = 0;
                
                foreach (FieldInfo field in fields)
                {
                    InjectAttribute injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr == null) continue;
                    
                    try
                    {
                        // Check if already injected (non-null)
                        object currentValue = field.GetValue(obj);
                        if (currentValue != null)
                        {
                            skippedCount++;
                            continue;
                        }
                        
                        // Use reflection to call Resolve<T>
                        MethodInfo resolveMethod = typeof(DIContainer)
                            .GetMethod("Resolve")
                            ?.MakeGenericMethod(field.FieldType);
                        
                        if (resolveMethod == null)
                        {
                            Debug.LogError($"[Dependencies] Could not find Resolve method for type {field.FieldType.Name}");
                            continue;
                        }
                        
                        object service = resolveMethod.Invoke(Container, null);
                        field.SetValue(obj, service);
                        injectedCount++;
                    }
                    catch (TargetInvocationException e)
                    {
                        Exception innerException = e.InnerException ?? e;
                        
                        if (injectAttr.Required)
                        {
                            Debug.LogError(
                                $"[Dependencies] ❌ REQUIRED dependency '{field.Name}' ({field.FieldType.Name}) " +
                                $"not found in '{objType.Name}': {innerException.Message}", obj);
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"[Dependencies] ⚠️ Optional dependency '{field.Name}' ({field.FieldType.Name}) " +
                                $"not found in '{objType.Name}'", obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"[Dependencies] Failed to inject '{field.Name}' in '{objType.Name}': {e.Message}", obj);
                    }
                }
                
                if (injectedCount > 0)
                {
                    Debug.Log($"[Dependencies] ✅ Injected {injectedCount} dependencies into {objType.Name}" +
                        (skippedCount > 0 ? $" (skipped {skippedCount} already injected)" : ""));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Dependencies] Failed to inject dependencies into '{obj.GetType().Name}': {e.Message}", obj);
            }
        }
        
        /// <summary>
        /// Reset the container (useful for tests)
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _container?.Clear();
                _container = null;
                Debug.Log("[Dependencies] Container reset");
            }
        }
    }
    
    /// <summary>
    /// Attribute to mark fields for dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute 
    {
        /// <summary>
        /// If true, throws error when dependency not found
        /// If false, only logs warning
        /// </summary>
        public bool Required { get; set; }
        
        public InjectAttribute(bool required = true)
        {
            Required = required;
        }
    }
}