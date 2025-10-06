using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    /// <summary>
    /// Simple Dependency Injection Container
    /// Clean, straightforward service registration and resolution
    /// Perfect for solo developers and small-to-medium projects
    /// </summary>
    public class DIContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service with interface and implementation
        /// Compile-time type safety guaranteed
        /// </summary>
        public void Register<TInterface, TImplementation>(TImplementation service) 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), 
                    $"Cannot register null service for {typeof(TInterface).Name}");
            }

            Type interfaceType = typeof(TInterface);
            
            if (_services.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"[DIContainer] Replacing {interfaceType.Name}");
            }

            _services[interfaceType] = service;
            Debug.Log($"[DIContainer] ✅ {interfaceType.Name} -> {typeof(TImplementation).Name}");
        }

        /// <summary>
        /// Register a concrete service (no interface)
        /// Use for settings, data classes, etc.
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"[DIContainer] Replacing {serviceType.Name}");
            }

            _services[serviceType] = service;
            Debug.Log($"[DIContainer] ✅ {serviceType.Name}");
        }

        /// <summary>
        /// Get a service - throws if not found
        /// Use when service MUST exist
        /// </summary>
        public T Resolve<T>() where T : class
        {
            Type serviceType = typeof(T);

            if (!_services.TryGetValue(serviceType, out var service))
            {
                throw new InvalidOperationException(
                    $"[DIContainer] Service '{serviceType.Name}' not registered\n" +
                    $"Available: {string.Join(", ", _services.Keys.Select(k => k.Name))}");
            }

            return service as T;
        }

        /// <summary>
        /// Try to get a service - returns false if not found
        /// Use when service is optional
        /// </summary>
        public bool TryResolve<T>(out T service) where T : class
        {
            service = null;

            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = obj as T;
                return service != null;
            }

            return false;
        }

        /// <summary>
        /// Check if service is registered
        /// </summary>
        public bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get all services implementing an interface
        /// Useful for systems like IInitializable
        /// </summary>
        public IEnumerable<T> GetAll<T>() where T : class
        {
            return _services.Values
                .Where(s => s is T)
                .Cast<T>();
        }

        /// <summary>
        /// Remove a service
        /// </summary>
        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Clear all services
        /// </summary>
        public void Clear()
        {
            int count = _services.Count;
            _services.Clear();
            Debug.Log($"[DIContainer] Cleared {count} services");
        }

        /// <summary>
        /// Total registered services
        /// </summary>
        public int ServiceCount => _services.Count;

        /// <summary>
        /// Debug: Print all services
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"[DIContainer] === {ServiceCount} Services ===");
            foreach (var kvp in _services.OrderBy(k => k.Key.Name))
            {
                Debug.Log($"  • {kvp.Key.Name}");
            }
        }
    }
}