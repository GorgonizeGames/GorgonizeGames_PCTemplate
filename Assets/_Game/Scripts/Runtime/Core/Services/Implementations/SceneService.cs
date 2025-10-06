using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public class SceneService : ServiceBase, ISceneService
    {
        [Inject(required: false)] private IAudioService _audioService;
        
        private string _currentSceneName;
        private bool _isLoading;
        
        public override int InitializationPriority => 10;
        protected override string ServiceName => "SceneService";
        
        protected override async Task OnInitializeAsync()
        {
            _currentSceneName = SceneManager.GetActiveScene().name;
            
            await Task.CompletedTask;
            
            LogInfo($"Current scene: {_currentSceneName}");
        }
        
        protected override bool ValidateDependencies()
        {
            if (_eventService == null)
            {
                LogWarning("EventService not available");
            }
            
            if (_audioService == null)
            {
                LogWarning("AudioService not available - music transitions won't work");
            }
            
            return true;
        }
        
        public async Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
        {
            if (!IsInitialized)
            {
                LogError("Service not initialized!");
                return;
            }
            
            if (_isLoading)
            {
                LogWarning("Already loading a scene");
                return;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                LogError("Scene name is null or empty");
                return;
            }
            
            _isLoading = true;
            
            try
            {
                _eventService?.Publish(new SceneLoadStartedEvent { SceneName = sceneName });
                _audioService?.StopMusic(0.5f);
                
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                
                if (asyncLoad == null)
                {
                    LogError($"Failed to load scene: {sceneName}");
                    _isLoading = false;
                    return;
                }
                
                while (!asyncLoad.isDone)
                {
                    await Task.Yield();
                }
                
                _currentSceneName = sceneName;
                _eventService?.Publish(new SceneLoadedEvent { SceneName = sceneName });
                
                LogInfo($"Scene loaded: {sceneName}");
            }
            catch (Exception e)
            {
                LogError($"Error loading scene '{sceneName}': {e.Message}");
                
                _eventService?.Publish(new ServiceFailedEvent
                {
                    ServiceName = ServiceName,
                    ErrorMessage = e.Message
                });
            }
            finally
            {
                _isLoading = false;
            }
        }
        
        public async Task UnloadSceneAsync(string sceneName)
        {
            if (!IsSceneLoaded(sceneName)) return;
            
            try
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                if (asyncUnload == null) return;
                
                while (!asyncUnload.isDone)
                {
                    await Task.Yield();
                }
                
                LogInfo($"Scene unloaded: {sceneName}");
            }
            catch (Exception e)
            {
                LogError($"Error unloading scene '{sceneName}': {e.Message}");
            }
        }
        
        public async Task ReloadCurrentScene()
        {
            await LoadSceneAsync(_currentSceneName, true);
        }
        
        public string GetCurrentSceneName() => _currentSceneName;
        
        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded) return true;
            }
            return false;
        }
    }
}