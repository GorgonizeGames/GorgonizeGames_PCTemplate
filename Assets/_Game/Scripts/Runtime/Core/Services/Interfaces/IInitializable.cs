using System.Threading.Tasks;

namespace Game.Runtime.Core.Services
{
    /// <summary>
    /// Services that need async initialization should implement this interface.
    /// Ensures proper initialization order and async loading.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Initialize the service asynchronously.
        /// Called by GameBootstrap in controlled order.
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Check if the service is fully initialized and ready to use.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initialization priority. Lower values = earlier initialization.
        /// Core services: 0-10
        /// Game services: 11-50
        /// UI services: 51-100
        /// </summary>
        int InitializationPriority { get; }
    }
}