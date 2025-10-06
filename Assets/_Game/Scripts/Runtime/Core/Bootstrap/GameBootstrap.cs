using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Services.Factories;
using Game.Runtime.UI.Windows;

namespace Game.Runtime.Core.Bootstrap
{
    /// <summary>
    /// Game Bootstrap - Initializes all core systems
    /// Follows Dependency Injection and Service Locator patterns
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private EventService _eventService;
        [SerializeField] private AudioService _audioService;
        [SerializeField] private SceneService _sceneService;
        [SerializeField] private InputService _inputService;
        
        [Header("UI Services")]
        [SerializeField] private WindowManager _windowManager;
        
        [Header("Settings")]
        [SerializeField] private GameSettings _gameSettings;
        
        [Header("Save System Settings")]
        [SerializeField] private bool _forceSteamInEditor = false;
        
        [Header("Bootstrap Settings")]
        [SerializeField] private string _initialSceneName = "MainMenu";
        [SerializeField] private bool _loadInitialSceneOnStart = true;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        private ISaveService _saveService;
        private bool _isInitialized = false;
        private bool _isInitializing = false;
        
        private async void Awake()
        {
            if (_isInitializing || _isInitialized)
            {
                Debug.LogWarning("[GameBootstrap] Already initialized or initializing!");
                return;
            }
            
            _isInitializing = true;
            DontDestroyOnLoad(gameObject);
            
            LogBootstrapStart();
            
            try
            {
                CreateSaveService();
                ValidateServices();
                RegisterAllServices();
                await InitializeAllServicesWithPriority();
                ConfigureApplication();
                
                _isInitialized = true;
                LogBootstrapComplete();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameBootstrap] ❌ CRITICAL ERROR during bootstrap: {e.Message}\n{e.StackTrace}");
                _isInitialized = false;
            }
            finally
            {
                _isInitializing = false;
            }
        }
        
        private void LogBootstrapStart()
        {
            if (_enableDebugLogs)
            {
                Debug.Log("========================================");
                Debug.Log("[GameBootstrap] 🚀 Starting bootstrap...");
                Debug.Log($"[GameBootstrap] Unity Version: {Application.unityVersion}");
                Debug.Log($"[GameBootstrap] Platform: {Application.platform}");
                Debug.Log("========================================");
            }
        }
        
        private void LogBootstrapComplete()
        {
            if (_enableDebugLogs)
            {
                Debug.Log("========================================");
                Debug.Log($"[GameBootstrap] ✅ Bootstrap completed!");
                Debug.Log($"[GameBootstrap] Services registered: {Dependencies.Container.ServiceCount}");
                Debug.Log($"[GameBootstrap] Time: {Time.realtimeSinceStartup:F2}s");
                Debug.Log("========================================");
            }
        }
        
        /// <summary>
        /// Create appropriate save service based on platform
        /// </summary>
        private void CreateSaveService()
        {
            _saveService = SaveServiceFactory.CreateSaveService(gameObject, _forceSteamInEditor);
            
            if (_saveService == null)
            {
                Debug.LogError("[GameBootstrap] ❌ CRITICAL: Failed to create SaveService!");
                return;
            }
            
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameBootstrap] Save Service: {_saveService.GetType().Name}");
                Debug.Log($"[GameBootstrap] Platform: {_saveService.CurrentPlatform}");
                Debug.Log($"[GameBootstrap] Cloud Enabled: {_saveService.IsCloudEnabled}");
            }
        }
        
        /// <summary>
        /// Validate that all required services are assigned
        /// </summary>
        private void ValidateServices()
        {
            List<string> missingServices = new List<string>();
            
            if (_eventService == null) missingServices.Add("EventService");
            if (_saveService == null) missingServices.Add("SaveService");
            if (_audioService == null) missingServices.Add("AudioService");
            if (_sceneService == null) missingServices.Add("SceneService");
            if (_inputService == null) missingServices.Add("InputService");
            if (_windowManager == null) missingServices.Add("WindowManager");
            
            if (missingServices.Count > 0)
            {
                string missing = string.Join(", ", missingServices);
                Debug.LogError($"[GameBootstrap] ❌ CRITICAL: Missing services: {missing}");
                Debug.LogError("[GameBootstrap] Please assign all services in the Inspector!");
            }
            
            if (_gameSettings == null)
            {
                Debug.LogWarning("[GameBootstrap] ⚠️ GameSettings not assigned - using defaults");
            }
        }
        
        /// <summary>
        /// FIX: Register all services with the DI container using helper methods
        /// CRITICAL: Register by INTERFACE type for SOLID principles
        /// </summary>
        private void RegisterAllServices()
        {
            var container = Dependencies.Container;
            
            // Core Services (Priority 0-10)
            // FIX: Helper metodlar kullanılıyor, kod tekrarı yok
            RegisterServiceByInterface<IEventService, EventService>(container, _eventService);
            RegisterServiceByInterface<ISaveService>(container, _saveService);
            RegisterServiceByInterface<IAudioService, AudioService>(container, _audioService);
            RegisterServiceByInterface<ISceneService, SceneService>(container, _sceneService);
            RegisterServiceByInterface<IInputService, InputService>(container, _inputService);
            
            // UI Services (Priority 51-100)
            RegisterServiceByInterface<IWindowManager, WindowManager>(container, _windowManager);
            
            // Settings (concrete type is fine)
            if (_gameSettings != null)
            {
                container.Register(_gameSettings);
                LogRegistration("GameSettings");
            }
        }
        
        /// <summary>
        /// FIX: Helper method to register a service with interface and implementation types
        /// </summary>
        private void RegisterServiceByInterface<TInterface, TImplementation>(DIContainer container, TImplementation service)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (service == null)
            {
                Debug.LogError($"[GameBootstrap] ❌ {typeof(TImplementation).Name} is null - cannot register!");
                return;
            }
            
            try
            {
                container.Register<TInterface, TImplementation>(service);
                LogRegistration($"{typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameBootstrap] ❌ Failed to register {typeof(TImplementation).Name}: {e.Message}");
            }
        }
        
        /// <summary>
        /// FIX: Helper method for services where we only have the instance (like SaveService from factory)
        /// </summary>
        private void RegisterServiceByInterface<TInterface>(DIContainer container, object service)
            where TInterface : class
        {
            if (service == null)
            {
                Debug.LogError($"[GameBootstrap] ❌ Service for {typeof(TInterface).Name} is null - cannot register!");
                return;
            }
            
            if (!(service is TInterface))
            {
                Debug.LogError($"[GameBootstrap] ❌ Service does not implement {typeof(TInterface).Name}");
                return;
            }
            
            try
            {
                // Use reflection to call the generic Register method
                var registerMethod = typeof(DIContainer)
                    .GetMethods()
                    .FirstOrDefault(m => 
                        m.Name == "Register" && 
                        m.GetGenericArguments().Length == 2 &&
                        m.GetParameters().Length == 1);
                
                if (registerMethod != null)
                {
                    var genericMethod = registerMethod.MakeGenericMethod(typeof(TInterface), service.GetType());
                    genericMethod.Invoke(container, new[] { service });
                    LogRegistration($"{typeof(TInterface).Name} -> {service.GetType().Name}");
                }
                else
                {
                    Debug.LogError($"[GameBootstrap] ❌ Could not find Register method");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameBootstrap] ❌ Failed to register {service.GetType().Name}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Initialize all services in priority order
        /// </summary>
        private async Task InitializeAllServicesWithPriority()
        {
            Debug.Log("[GameBootstrap] 🔄 Initializing services with priority order...");
            
            var initializableServices = Dependencies.Container
                .GetAll<IInitializable>()
                .Where(s => s != null)
                .OrderBy(s => s.InitializationPriority)
                .ToList();
            
            if (initializableServices.Count == 0)
            {
                Debug.LogWarning("[GameBootstrap] ⚠️ No IInitializable services found!");
                return;
            }
            
            int initializedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;
            
            foreach (var service in initializableServices)
            {
                if (service.IsInitialized)
                {
                    skippedCount++;
                    if (_enableDebugLogs)
                    {
                        Debug.Log($"[GameBootstrap] ⏭️  [{service.InitializationPriority}] {service.GetType().Name} already initialized");
                    }
                    continue;
                }
                
                string serviceName = service.GetType().Name;
                int priority = service.InitializationPriority;
                
                if (_enableDebugLogs)
                {
                    Debug.Log($"[GameBootstrap] 🔧 [{priority}] Initializing {serviceName}...");
                }
                
                try
                {
                    float startTime = Time.realtimeSinceStartup;
                    await service.InitializeAsync();
                    float duration = Time.realtimeSinceStartup - startTime;
                    
                    initializedCount++;
                    
                    if (_enableDebugLogs)
                    {
                        Debug.Log($"[GameBootstrap] ✅ [{priority}] {serviceName} initialized ({duration:F3}s)");
                    }
                }
                catch (Exception e)
                {
                    failedCount++;
                    Debug.LogError($"[GameBootstrap] ❌ Failed to initialize {serviceName}: {e.Message}\n{e.StackTrace}");
                }
            }
            
            Debug.Log($"[GameBootstrap] ✅ Initialization complete: {initializedCount} initialized, {skippedCount} skipped, {failedCount} failed");
            
            if (failedCount > 0)
            {
                Debug.LogError($"[GameBootstrap] ⚠️  {failedCount} service(s) failed to initialize! Game may not work correctly.");
            }
        }
        
        /// <summary>
        /// Configure application settings
        /// </summary>
        private void ConfigureApplication()
        {
            if (_gameSettings == null)
            {
                Debug.LogWarning("[GameBootstrap] GameSettings not available - skipping configuration");
                return;
            }
            
            Application.targetFrameRate = _gameSettings.TargetFrameRate;
            QualitySettings.vSyncCount = _gameSettings.VSync ? 1 : 0;
            
            if (_audioService != null && _audioService.IsInitialized)
            {
                _audioService.SetMasterVolume(_gameSettings.MasterVolume);
                _audioService.SetMusicVolume(_gameSettings.MusicVolume);
                _audioService.SetSFXVolume(_gameSettings.SFXVolume);
            }
            
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameBootstrap] ⚙️  Application configured (FPS: {_gameSettings.TargetFrameRate}, VSync: {_gameSettings.VSync})");
            }
        }
        
        private async void Start()
        {
            while (!_isInitialized)
            {
                await Task.Yield();
            }
            
            if (_loadInitialSceneOnStart)
            {
                await LoadInitialScene();
            }
        }
        
        /// <summary>
        /// Load the initial scene
        /// </summary>
        private async Task LoadInitialScene()
        {
            if (_sceneService == null || !_sceneService.IsInitialized)
            {
                Debug.LogError("[GameBootstrap] Cannot load initial scene - SceneService not ready!");
                return;
            }
            
            if (string.IsNullOrEmpty(_initialSceneName))
            {
                Debug.LogWarning("[GameBootstrap] Initial scene name not set - skipping scene load");
                return;
            }
            
            try
            {
                Debug.Log($"[GameBootstrap] Loading initial scene: {_initialSceneName}");
                await _sceneService.LoadSceneAsync(_initialSceneName, showLoadingScreen: false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameBootstrap] Failed to load initial scene: {e.Message}");
            }
        }
        
        private void LogRegistration(string serviceName)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameBootstrap] ✅ Registered: {serviceName}");
            }
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("[GameBootstrap] Application quitting - cleaning up...");
            Dependencies.Container.Clear();
        }
        
#if UNITY_EDITOR
        
        [ContextMenu("Show Service Info")]
        private void ShowServiceInfo()
        {
            Debug.Log("==================== SERVICE INFO ====================");
            Debug.Log($"Container Services: {Dependencies.Container.ServiceCount}");
            Debug.Log($"Initialized: {_isInitialized}");
            
            if (_saveService != null)
            {
                Debug.Log($"Save Platform: {_saveService.CurrentPlatform}");
                Debug.Log($"Cloud Enabled: {_saveService.IsCloudEnabled}");
            }
            
            Dependencies.Container.DebugPrintServices();
            Debug.Log("======================================================");
        }
        
        [ContextMenu("Validate All Services")]
        private void ValidateAllServicesEditor()
        {
            Debug.Log("==================== SERVICE VALIDATION ====================");
            
            bool allValid = true;
            
            allValid &= ValidateServiceEditor(_eventService, "EventService");
            allValid &= ValidateServiceEditor(_saveService as MonoBehaviour, "SaveService");
            allValid &= ValidateServiceEditor(_audioService, "AudioService");
            allValid &= ValidateServiceEditor(_sceneService, "SceneService");
            allValid &= ValidateServiceEditor(_inputService, "InputService");
            allValid &= ValidateServiceEditor(_windowManager, "WindowManager");
            allValid &= ValidateServiceEditor(_gameSettings, "GameSettings");
            
            Debug.Log("===========================================================");
            
            if (allValid)
            {
                Debug.Log("✅ ALL SERVICES VALIDATED!");
            }
            else
            {
                Debug.LogError("❌ SOME SERVICES MISSING! Check the Inspector.");
            }
        }
        
        private bool ValidateServiceEditor(UnityEngine.Object service, string serviceName)
        {
            bool isValid = service != null;
            Debug.Log($"  {(isValid ? "✅" : "❌")} {serviceName}");
            return isValid;
        }
        
        [ContextMenu("Show Initialization Order")]
        private void ShowInitializationOrder()
        {
            Debug.Log("==================== INITIALIZATION ORDER ====================");
            
            var services = Dependencies.Container.GetAll<IInitializable>()
                .Where(s => s != null)
                .OrderBy(s => s.InitializationPriority)
                .ToList();
            
            if (services.Count == 0)
            {
                Debug.LogWarning("No IInitializable services found!");
                return;
            }
            
            for (int i = 0; i < services.Count; i++)
            {
                var service = services[i];
                string status = service.IsInitialized ? "✅" : "❌";
                Debug.Log($"{i + 1}. [{service.InitializationPriority}] {service.GetType().Name} {status}");
            }
            
            Debug.Log("===============================================================");
        }
        
        [ContextMenu("Force Reinitialize All")]
        private async void ForceReinitializeAll()
        {
            Debug.LogWarning("[GameBootstrap] ⚠️ Force reinitializing all services...");
            
            _isInitialized = false;
            
            var services = Dependencies.Container.GetAll<IInitializable>()
                .Where(s => s != null)
                .OrderBy(s => s.InitializationPriority)
                .ToList();
            
            foreach (var service in services)
            {
                try
                {
                    await service.InitializeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to initialize {service.GetType().Name}: {e.Message}");
                }
            }
            
            _isInitialized = true;
            Debug.Log("[GameBootstrap] ✅ Reinitialization complete");
        }
#endif
    }
}