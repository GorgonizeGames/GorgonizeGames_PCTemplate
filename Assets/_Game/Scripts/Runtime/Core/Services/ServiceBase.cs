using System;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public abstract class ServiceBase : MonoBehaviour, IInitializable
    {
        [Inject(required: false)] protected IEventService _eventService;
        
        [Header("Service Debug")]
        [SerializeField] protected bool _enableServiceLogs = false;
        
        public bool IsInitialized { get; protected set; }
        public abstract int InitializationPriority { get; }
        protected abstract string ServiceName { get; }
        
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
                Dependencies.Inject(this);
                
                if (!ValidateDependencies())
                {
                    LogError("Dependency validation failed!");
                    PublishFailureEvent("Dependency validation failed");
                    return;
                }
                
                await OnInitializeAsync();
                
                IsInitialized = true;
                
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
        
        protected abstract Task OnInitializeAsync();
        
        /// <summary>
        /// ✅ FIXED: EventService is optional, not critical
        /// </summary>
        protected virtual bool ValidateDependencies()
        {
            if (_eventService == null)
            {
                LogWarning("EventService not available - events will not be published");
            }
            
            return true;
        }
        
        /// <summary>
        /// ✅ FIXED: Null check added
        /// </summary>
        private void PublishSuccessEvent(float initTime)
        {
            if (_eventService != null)
            {
                _eventService.Publish(new ServiceInitializedEvent
                {
                    ServiceName = ServiceName,
                    InitializationTime = initTime
                });
            }
        }
        
        /// <summary>
        /// ✅ FIXED: Null check added
        /// </summary>
        private void PublishFailureEvent(string errorMessage)
        {
            if (_eventService != null)
            {
                _eventService.Publish(new ServiceFailedEvent
                {
                    ServiceName = ServiceName,
                    ErrorMessage = errorMessage
                });
            }
        }
        
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