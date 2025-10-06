using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Services.Factories;
using Game.Runtime.UI.Windows;

namespace Game.Runtime.Core.Bootstrap
{
    /// <summary>
    /// Game Bootstrap - Clean and simple initialization
    /// Perfect for solo development
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
        [SerializeField] private bool _forceSteamInEditor = false;
        
        [Header("Startup")]
        [SerializeField] private string _initialScene = "MainMenu";
        [SerializeField] private bool _loadSceneOnStart = true;
        
        private ISaveService _saveService;
        private bool _ready = false;

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[Bootstrap] 🚀 Starting...");
            
            // Create save service from factory
            _saveService = SaveServiceFactory.CreateSaveService(gameObject, _forceSteamInEditor);
            
            // Register all services
            RegisterServices();
            
            // Initialize in order
            await InitializeServices();
            
            // Apply settings
            ConfigureApp();
            
            _ready = true;
            Debug.Log($"[Bootstrap] ✅ Ready ({Dependencies.Container.ServiceCount} services)");
        }

        private void RegisterServices()
        {
            var c = Dependencies.Container;
            
            // Core services
            if (_eventService) c.Register<IEventService, EventService>(_eventService);
            if (_saveService != null) c.Register<ISaveService>(_saveService);
            if (_audioService) c.Register<IAudioService, AudioService>(_audioService);
            if (_inputService) c.Register<IInputService, InputService>(_inputService);
            if (_sceneService) c.Register<ISceneService, SceneService>(_sceneService);
            
            // UI services
            if (_windowManager) c.Register<IWindowManager, WindowManager>(_windowManager);
            
            // Settings
            if (_gameSettings) c.Register(_gameSettings);
        }

        private async Task InitializeServices()
        {
            var services = Dependencies.Container
                .GetAll<IInitializable>()
                .OrderBy(s => s.InitializationPriority)
                .ToList();

            foreach (var service in services)
            {
                if (service.IsInitialized) continue;
                
                try
                {
                    await service.InitializeAsync();
                    Debug.Log($"[Bootstrap] ✅ {service.GetType().Name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Bootstrap] ❌ {service.GetType().Name} failed: {e.Message}");
                }
            }
        }

        private void ConfigureApp()
        {
            if (_gameSettings == null) return;
            
            Application.targetFrameRate = _gameSettings.TargetFrameRate;
            QualitySettings.vSyncCount = _gameSettings.VSync ? 1 : 0;
            
            if (_audioService && _audioService.IsInitialized)
            {
                _audioService.SetMasterVolume(_gameSettings.MasterVolume);
                _audioService.SetMusicVolume(_gameSettings.MusicVolume);
                _audioService.SetSFXVolume(_gameSettings.SFXVolume);
            }
        }

        private async void Start()
        {
            while (!_ready) await Task.Yield();
            
            if (_loadSceneOnStart && !string.IsNullOrEmpty(_initialScene))
            {
                await _sceneService.LoadSceneAsync(_initialScene, false);
            }
        }

        private void OnApplicationQuit()
        {
            Dependencies.Container.Clear();
        }

#if UNITY_EDITOR
        [ContextMenu("Show Services")]
        private void ShowServices()
        {
            Dependencies.Container.DebugPrint();
        }
#endif
    }
}