using System;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    /// <summary>
    /// Abstract base class for all services
    /// Provides common initialization and dependency injection pattern
    /// Follows Template Method Pattern and DRY principle
    /// </summary>
    public abstract class ServiceBase : MonoBehaviour, IInitializable
    {
        [Inject(required: false)] protected IEventService _eventService;
        
        [Header("Service Debug")]
        [SerializeField] protected bool _enableServiceLogs = false;
        
        public bool IsInitialized { get; protected set; }
        public abstract int InitializationPriority { get; }
        protected abstract string ServiceName { get; }
        
        /// <summary>
        /// Template method for initialization
        /// Follows Template Method Pattern
        /// </summary>
        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                LogWarning("Already initialized!");
                return;
            }
            
            LogInfo("Initializing...");
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                // Step 1: Inject dependencies
                Dependencies.Inject(this);
                
                // Step 2: Validate dependencies (virtual method)
                if (!ValidateDependencies())
                {
                    LogError("Dependency validation failed!");
                    PublishFailureEvent("Dependency validation failed");
                    return;
                }
                
                // Step 3: Custom initialization logic (abstract method)
                await OnInitializeAsync();
                
                // Step 4: Mark as initialized
                IsInitialized = true;
                
                // Step 5: Publish success event
                float duration = Time.realtimeSinceStartup - startTime;
                PublishSuccessEvent(duration);
                
                LogInfo($"✅ Initialized successfully ({duration:F3}s)");
            }
            catch (Exception e)
            {
                LogError($"Initialization failed: {e.Message}\n{e.StackTrace}");
                PublishFailureEvent(e.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Override this to implement service-specific initialization
        /// </summary>
        protected abstract Task OnInitializeAsync();
        
        /// <summary>
        /// Override this to validate service dependencies
        /// Return false if critical dependencies are missing
        /// </summary>
        protected virtual bool ValidateDependencies()
        {
            // EventService is optional but recommended
            if (_eventService == null)
            {
                LogWarning("EventService not available - events will not be published");
            }
            
            return true;
        }
        
        /// <summary>
        /// Publish initialization success event
        /// </summary>
        private void PublishSuccessEvent(float initTime)
        {
            _eventService?.Publish(new ServiceInitializedEvent
            {
                ServiceName = ServiceName,
                InitializationTime = initTime
            });
        }
        
        /// <summary>
        /// Publish initialization failure event
        /// </summary>
        private void PublishFailureEvent(string errorMessage)
        {
            _eventService?.Publish(new ServiceFailedEvent
            {
                ServiceName = ServiceName,
                ErrorMessage = errorMessage
            });
        }
        
        // Logging helpers
        protected void LogInfo(string message)
        {
            if (_enableServiceLogs)
            {
                Debug.Log($"[{ServiceName}] {message}");
            }
        }
        
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{ServiceName}] ⚠️ {message}");
        }
        
        protected void LogError(string message)
        {
            Debug.LogError($"[{ServiceName}] ❌ {message}");
        }
        
        protected virtual void OnDestroy()
        {
            IsInitialized = false;
            LogInfo("Destroyed");
        }
    }
}