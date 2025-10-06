using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// Static debug configuration utility
    /// Automatically configures Unity's debug logging system
    /// Runs before any scene loads
    /// </summary>
    public static class DebugConfig
    {
        private static bool _isConfigured = false;
        
        /// <summary>
        /// Configure Unity's debug logging system
        /// Automatically called by Unity before scene load
        /// - Editor: Logging ENABLED
        /// - Build: Logging DISABLED
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Configure()
        {
            if (_isConfigured) return;
            
#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[DebugConfig] 📝 Debug Logging ENABLED (Editor Mode)");
#else
            Debug.unityLogger.logEnabled = false;
            // No logs in builds
#endif
            
            _isConfigured = true;
        }
        
        /// <summary>
        /// Manually enable debug logging (Editor only)
        /// Useful for runtime debugging
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void EnableLogging()
        {
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[DebugConfig] 📝 Debug Logging ENABLED");
        }
        
        /// <summary>
        /// Manually disable debug logging (Editor only)
        /// Useful for testing build behavior
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DisableLogging()
        {
            Debug.Log("[DebugConfig] 🔇 Debug Logging DISABLED");
            Debug.unityLogger.logEnabled = false;
        }
        
        /// <summary>
        /// Check if logging is currently enabled
        /// </summary>
        public static bool IsLoggingEnabled()
        {
            return Debug.unityLogger.logEnabled;
        }
    }
}