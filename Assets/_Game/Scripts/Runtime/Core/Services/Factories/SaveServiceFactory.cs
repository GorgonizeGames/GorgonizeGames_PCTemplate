using UnityEngine;
using System;

namespace Game.Runtime.Core.Services.Factories
{
    /// <summary>
    /// Factory for creating the appropriate save service based on platform
    /// Follows Factory Pattern and Open/Closed Principle
    /// </summary>
    public static class SaveServiceFactory
    {
        /// <summary>
        /// Create the appropriate save service for the current platform
        /// </summary>
        /// <param name="parent">Parent GameObject for the service component</param>
        /// <param name="forceSteam">Force Steam service even in editor</param>
        /// <returns>ISaveService implementation or null if creation fails</returns>
        public static ISaveService CreateSaveService(GameObject parent, bool forceSteam = false)
        {
            if (parent == null)
            {
                Debug.LogError("[SaveServiceFactory] Parent GameObject is null!");
                return null;
            }

#if UNITY_STANDALONE && !UNITY_EDITOR
            // Production build on standalone platforms
            if (IsSteamAvailable())
            {
                return CreateSteamSaveService(parent);
            }
            else
            {
                Debug.LogWarning("[SaveServiceFactory] Steam not available on standalone build, using local save");
                return CreateLocalSaveService(parent);
            }
#elif UNITY_EDITOR
            // Editor mode
            if (forceSteam && IsSteamAvailable())
            {
                Debug.Log("[SaveServiceFactory] Force Steam enabled in Editor");
                return CreateSteamSaveService(parent);
            }
            else
            {
                return CreateLocalSaveService(parent);
            }
#else
            // Other platforms (mobile, web, etc.)
            return CreateLocalSaveService(parent);
#endif
        }
        
        /// <summary>
        /// Create Steam Cloud save service
        /// </summary>
        private static ISaveService CreateSteamSaveService(GameObject parent)
        {
            try
            {
                var steamService = parent.GetComponent<SteamSaveService>();
                if (steamService == null)
                {
                    steamService = parent.AddComponent<SteamSaveService>();
                }
                
                Debug.Log("[SaveServiceFactory] ✅ Created SteamSaveService");
                return steamService;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveServiceFactory] Failed to create SteamSaveService: {e.Message}");
                Debug.LogWarning("[SaveServiceFactory] Falling back to LocalSaveService");
                return CreateLocalSaveService(parent);
            }
        }
        
        /// <summary>
        /// Create local file system save service
        /// </summary>
        private static ISaveService CreateLocalSaveService(GameObject parent)
        {
            try
            {
                var localService = parent.GetComponent<LocalSaveService>();
                if (localService == null)
                {
                    localService = parent.AddComponent<LocalSaveService>();
                }
                
                Debug.Log("[SaveServiceFactory] ✅ Created LocalSaveService");
                return localService;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveServiceFactory] Failed to create LocalSaveService: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if Steam is available in the current build
        /// </summary>
        private static bool IsSteamAvailable()
        {
            try
            {
                // Check if Steamworks assembly is loaded
                Type steamAPIType = Type.GetType("Steamworks.SteamAPI, Assembly-CSharp");
                bool available = steamAPIType != null;
                
                if (available)
                {
                    Debug.Log("[SaveServiceFactory] Steam API detected");
                }
                
                return available;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveServiceFactory] Error checking Steam availability: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get the appropriate save platform enum for current setup
        /// </summary>
        public static SavePlatform GetCurrentPlatform(bool forceSteam = false)
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            return IsSteamAvailable() ? SavePlatform.SteamCloud : SavePlatform.Local;
#elif UNITY_EDITOR
            return (forceSteam && IsSteamAvailable()) ? SavePlatform.SteamCloud : SavePlatform.Local;
#else
            return SavePlatform.Local;
#endif
        }
    }
}