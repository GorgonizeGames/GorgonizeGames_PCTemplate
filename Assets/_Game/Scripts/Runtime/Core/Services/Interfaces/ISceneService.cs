using System.Threading.Tasks;

namespace Game.Runtime.Core.Services
{
    public interface ISceneService
    {
        Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true);
        Task UnloadSceneAsync(string sceneName);
        Task ReloadCurrentScene();
        string GetCurrentSceneName();
        bool IsSceneLoaded(string sceneName);
    }
}