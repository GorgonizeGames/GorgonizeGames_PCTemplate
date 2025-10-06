using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    /// <summary>
    /// Thread-safe Dependency Injection Container
    /// Manages service registration and resolution
    /// </summary>
    public class DIContainer
    {
        private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Register a service instance for a specific type
        /// </summary>
        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[DIContainer] Cannot register null service for type {typeof(T).Name}");
                throw new ArgumentNullException(nameof(service), $"Cannot register null service for type {typeof(T)}");
            }

            Type serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out object existingService))
            {
                Debug.LogWarning($"[DIContainer] Service {serviceType.Name} is being replaced. " +
                    $"Old: {existingService.GetType().Name}, New: {service.GetType().Name}");
            }

            _services.AddOrUpdate(serviceType, service, (key, oldValue) => service);
            Debug.Log($"[DIContainer] ✅ Registered {serviceType.Name}");
        }

        /// <summary>
        /// FIX: Register a service with both interface and implementation types
        /// Proper type safety with constraints
        /// </summary>
        public void Register<TInterface, TImplementation>(TImplementation service) 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            if (service == null)
            {
                Debug.LogError($"[DIContainer] Cannot register null service");
                throw new ArgumentNullException(nameof(service));
            }

            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TImplementation);
            
            // FIX: Tip kontrolü yapılıyor
            if (!interfaceType.IsAssignableFrom(implementationType))
            {
                Debug.LogError($"[DIContainer] Type mismatch: {implementationType.Name} does not implement {interfaceType.Name}");
                throw new InvalidOperationException($"Type {implementationType} does not implement {interfaceType}");
            }

            // Register by interface
            if (_services.TryGetValue(interfaceType, out object existingInterface))
            {
                Debug.LogWarning($"[DIContainer] Interface {interfaceType.Name} is being replaced. " +
                    $"Old: {existingInterface.GetType().Name}, New: {implementationType.Name}");
            }
            _services.AddOrUpdate(interfaceType, service, (key, oldValue) => service);
            
            // Also register by implementation type for flexibility
            if (_services.TryGetValue(implementationType, out object existingImpl))
            {
                Debug.LogWarning($"[DIContainer] Implementation {implementationType.Name} is being replaced.");
            }
            _services.AddOrUpdate(implementationType, service, (key, oldValue) => service);
            
            Debug.Log($"[DIContainer] ✅ Registered {interfaceType.Name} -> {implementationType.Name}");
        }

        /// <summary>
        /// Resolve a service by type. Throws exception if not found.
        /// </summary>
        public T Resolve<T>() where T : class
        {
            Type serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out var service))
            {
                try
                {
                    T result = service as T;
                    if (result == null)
                    {
                        Debug.LogError($"[DIContainer] Service registered for type {serviceType.Name} cannot be cast to {typeof(T).Name}. " +
                            $"Registered type: {service.GetType().Name}");
                        throw new InvalidCastException(
                            $"Service registered for type {serviceType} (actual type: {service.GetType()}) cannot be cast to {typeof(T)}");
                    }
                    return result;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"[DIContainer] Service registered for type {serviceType.Name} cannot be cast to {typeof(T).Name}");
                    throw new InvalidOperationException(
                        $"Service registered for type {serviceType} cannot be cast to {typeof(T)}", e);
                }
            }

            Debug.LogError($"[DIContainer] Service of type {serviceType.Name} not registered. " +
                $"Available services: {string.Join(", ", _services.Keys.Select(k => k.Name))}");
            
            throw new InvalidOperationException($"Service of type {serviceType} not registered.");
        }

        /// <summary>
        /// Try to resolve a service. Returns false if not found.
        /// </summary>
        public bool TryResolve<T>(out T service) where T : class
        {
            service = default(T);

            if (_services.TryGetValue(typeof(T), out var obj))
            {
                try
                {
                    service = obj as T;
                    if (service == null)
                    {
                        Debug.LogWarning($"[DIContainer] Service of type {typeof(T).Name} found but cannot be cast. " +
                            $"Registered type: {obj.GetType().Name}");
                        return false;
                    }
                    return true;
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"[DIContainer] Service of type {typeof(T).Name} found but cannot be cast");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a service type is registered
        /// </summary>
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Check if a service type is registered
        /// </summary>
        public bool IsRegistered(Type type)
        {
            return _services.ContainsKey(type);
        }

        /// <summary>
        /// Unregister a service
        /// </summary>
        public void Unregister<T>()
        {
            if (_services.TryRemove(typeof(T), out _))
            {
                Debug.Log($"[DIContainer] Unregistered {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Clear all registered services
        /// </summary>
        public void Clear()
        {
            int count = _services.Count;
            _services.Clear();
            Debug.Log($"[DIContainer] Cleared {count} services");
        }

        /// <summary>
        /// Get all services that implement a specific interface
        /// Useful for IInitializable pattern
        /// </summary>
        public IEnumerable<T> GetAll<T>() where T : class
        {
            return _services.Values
                .Where(service => service is T)
                .Cast<T>();
        }

        /// <summary>
        /// Get all registered services (for debugging)
        /// </summary>
        public IEnumerable<object> GetAllServices()
        {
            return _services.Values;
        }

        /// <summary>
        /// Get all registered service types (for debugging)
        /// </summary>
        public IEnumerable<Type> GetAllServiceTypes()
        {
            return _services.Keys;
        }

        /// <summary>
        /// Total number of registered services
        /// </summary>
        public int ServiceCount => _services.Count;

        /// <summary>
        /// Debug: Print all registered services
        /// </summary>
        public void DebugPrintServices()
        {
            Debug.Log($"[DIContainer] === Registered Services ({ServiceCount}) ===");
            foreach (var kvp in _services)
            {
                Debug.Log($"  {kvp.Key.Name} -> {kvp.Value.GetType().Name}");
            }
        }
    }
}