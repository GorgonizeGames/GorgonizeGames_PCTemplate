using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public class EventService : MonoBehaviour, IEventService, IInitializable
    {
        private readonly Dictionary<Type, Delegate> _eventCallbacks = new Dictionary<Type, Delegate>();
        private readonly Queue<QueuedEvent> _eventQueue = new Queue<QueuedEvent>();
        
        [Header("Settings")]
        [SerializeField] private bool _enableEventLogging = false;
        [SerializeField] private int _maxQueueSize = 1000;
        [SerializeField] private bool _processQueuedEventsImmediately = true;
        
        private struct QueuedEvent
        {
            public Type EventType;
            public object EventData;
            public float QueuedTime;
        }
        
        public bool IsInitialized { get; private set; }
        public int InitializationPriority => 0;
        
        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[EventService] Already initialized!");
                return;
            }
            
            _eventCallbacks.Clear();
            _eventQueue.Clear();
            
            await Task.CompletedTask;
            
            IsInitialized = true;
            Debug.Log("[EventService] ✅ Initialized successfully");
        }

        public void Subscribe<T>(Action<T> callback) where T : struct, IGameEvent
        {
            if (callback == null)
            {
                Debug.LogError("[EventService] Cannot subscribe with null callback");
                return;
            }
            
            Type eventType = typeof(T);

            if (_eventCallbacks.ContainsKey(eventType))
            {
                _eventCallbacks[eventType] = Delegate.Combine(_eventCallbacks[eventType], callback);
            }
            else
            {
                _eventCallbacks[eventType] = callback;
            }
            
            if (_enableEventLogging)
            {
                Debug.Log($"[EventService] Subscribed to {eventType.Name} (Total subscribers: {GetSubscriberCount<T>()})");
            }
        }

        public void Unsubscribe<T>(Action<T> callback) where T : struct, IGameEvent
        {
            if (callback == null)
            {
                Debug.LogWarning("[EventService] Cannot unsubscribe null callback");
                return;
            }
            
            Type eventType = typeof(T);

            if (_eventCallbacks.ContainsKey(eventType))
            {
                _eventCallbacks[eventType] = Delegate.Remove(_eventCallbacks[eventType], callback);

                if (_eventCallbacks[eventType] == null)
                {
                    _eventCallbacks.Remove(eventType);
                }
                
                if (_enableEventLogging)
                {
                    Debug.Log($"[EventService] Unsubscribed from {eventType.Name}");
                }
            }
        }

        public void Publish<T>(T eventData) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!IsInitialized)
            {
                Debug.LogWarning($"[EventService] Service not initialized. Queueing event: {eventType.Name}");
                QueueEvent(eventType, eventData);
                return;
            }

            if (_eventCallbacks.TryGetValue(eventType, out Delegate callback))
            {
                try
                {
                    if (_enableEventLogging)
                    {
                        int subscriberCount = callback.GetInvocationList().Length;
                        Debug.Log($"[EventService] Publishing {eventType.Name} to {subscriberCount} subscriber(s)");
                    }
                    
                    Delegate[] invocationList = callback.GetInvocationList();
                    
                    foreach (Delegate handler in invocationList)
                    {
                        try
                        {
                            (handler as Action<T>)?.Invoke(eventData);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[EventService] Error in event handler for {eventType.Name}: {e.Message}\n{e.StackTrace}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventService] Critical error publishing {eventType.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
            else if (_enableEventLogging)
            {
                Debug.Log($"[EventService] No subscribers for {eventType.Name}");
            }
        }
        
        private void QueueEvent(Type eventType, object eventData)
        {
            if (_eventQueue.Count >= _maxQueueSize)
            {
                Debug.LogWarning($"[EventService] Event queue full ({_maxQueueSize})! Dropping {eventType.Name}");
                return;
            }
            
            _eventQueue.Enqueue(new QueuedEvent
            {
                EventType = eventType,
                EventData = eventData,
                QueuedTime = Time.realtimeSinceStartup
            });
            
            if (_enableEventLogging)
            {
                Debug.Log($"[EventService] Queued event: {eventType.Name} (Queue size: {_eventQueue.Count})");
            }
        }
        
        private void ProcessQueuedEvents()
        {
            if (_eventQueue.Count == 0) return;
            
            int processedCount = 0;
            int maxProcessPerFrame = 10;
            
            while (_eventQueue.Count > 0 && processedCount < maxProcessPerFrame)
            {
                QueuedEvent queuedEvent = _eventQueue.Dequeue();
                
                if (_eventCallbacks.TryGetValue(queuedEvent.EventType, out Delegate callback))
                {
                    try
                    {
                        callback.DynamicInvoke(queuedEvent.EventData);
                        processedCount++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventService] Error processing queued event {queuedEvent.EventType.Name}: {e.Message}");
                    }
                }
            }
            
            if (_enableEventLogging && processedCount > 0)
            {
                Debug.Log($"[EventService] Processed {processedCount} queued events (Remaining: {_eventQueue.Count})");
            }
        }
        
        private void Update()
        {
            if (IsInitialized && _eventQueue.Count > 0)
            {
                if (_processQueuedEventsImmediately)
                {
                    ProcessQueuedEvents();
                }
            }
        }

        public void Clear()
        {
            int callbackCount = _eventCallbacks.Count;
            int queueCount = _eventQueue.Count;
            
            _eventCallbacks.Clear();
            _eventQueue.Clear();
            
            if (_enableEventLogging)
            {
                Debug.Log($"[EventService] Cleared {callbackCount} subscriptions and {queueCount} queued events");
            }
        }
        
        public int GetSubscriberCount<T>() where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            if (_eventCallbacks.TryGetValue(eventType, out Delegate callback))
            {
                return callback.GetInvocationList().Length;
            }
            return 0;
        }
        
        public IEnumerable<Type> GetRegisteredEventTypes()
        {
            return _eventCallbacks.Keys;
        }

        /// <summary>
        /// ✅ FIXED: Added memory leak warning
        /// </summary>
        private void OnDestroy()
        {
            if (_eventCallbacks.Count > 0)
            {
                Debug.LogWarning($"[EventService] Destroying with {_eventCallbacks.Count} active subscriptions. Consider manual cleanup to prevent memory leaks.");
            }
            
            Clear();
            IsInitialized = false;
            Debug.Log("[EventService] Destroyed");
        }
        
#if UNITY_EDITOR
        [ContextMenu("Debug: Print Event Statistics")]
        private void DebugPrintStatistics()
        {
            Debug.Log($"=== EVENT SERVICE STATISTICS ===");
            Debug.Log($"Total Event Types: {_eventCallbacks.Count}");
            Debug.Log($"Queued Events: {_eventQueue.Count}");
            Debug.Log($"Initialized: {IsInitialized}");
            
            foreach (var kvp in _eventCallbacks)
            {
                int count = kvp.Value.GetInvocationList().Length;
                Debug.Log($"  {kvp.Key.Name}: {count} subscriber(s)");
            }
        }
#endif
    }
}