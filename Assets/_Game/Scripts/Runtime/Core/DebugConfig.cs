using UnityEngine;

namespace Game.Runtime.Core
{
    public static class DebugConfig
    {
        private static bool _isConfigured = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Configure()
        {
            if (_isConfigured) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[DebugConfig] 📝 Debug Logging ENABLED (Editor Mode)");
#else
            Debug.unityLogger.logEnabled = false;
            // No logs in builds
#endif

            _isConfigured = true;
        }
    }
}