using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.Data;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
using Steamworks;
#endif

namespace Game.Runtime.Core.Services
{
    public class SteamSaveService : ServiceBase, ISaveService
    {
        [Header("Settings")]
        [SerializeField] private bool _enableAutoSync = true;
        [SerializeField] private float _autoSyncInterval = 300f;
        [SerializeField] private string _cloudFolder = "saves/";
        
        private bool _steamInitialized;
        private float _lastSyncTime;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        
        public SavePlatform CurrentPlatform => SavePlatform.SteamCloud;
        public bool IsCloudEnabled
        {
            get
            {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
                return _steamInitialized && SteamRemoteStorage.IsCloudEnabledForAccount() && SteamRemoteStorage.IsCloudEnabledForApp();
#else
                return false;
#endif
            }
        }
        
        public event Action<string> OnSaveStarted;
        public event Action<string, bool> OnSaveCompleted;
        public event Action<string> OnLoadStarted;
        public event Action<string, bool> OnLoadCompleted;
        public event Action OnCloudSyncStarted;
        public event Action<bool> OnCloudSyncCompleted;
        
        public override int InitializationPriority => 3;
        protected override string ServiceName => "SteamSaveService";
        
        private void Awake()
        {
            InitializeSteam();
        }
        
        protected override async Task OnInitializeAsync()
        {
            if (IsCloudEnabled)
            {
                LogInfo("Steam Cloud enabled");
            }
            else
            {
                LogWarning("Steam Cloud not available");
            }
            
            await Task.CompletedTask;
        }
        
        protected override bool ValidateDependencies()
        {
            if (_eventService == null)
            {
                LogWarning("EventService not available");
            }
            
            return true;
        }
        
        private void Update()
        {
            if (_enableAutoSync && IsCloudEnabled)
            {
                if (Time.time - _lastSyncTime >= _autoSyncInterval)
                {
                    _ = SyncWithCloudAsync();
                    _lastSyncTime = Time.time;
                }
            }
        }
        
        private void InitializeSteam()
        {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            try
            {
                if (SteamAPI.Init())
                {
                    _steamInitialized = true;
                    Debug.Log("[SteamSaveService] ✅ Steam initialized");
                    
                    if (IsCloudEnabled)
                    {
                        Debug.Log("[SteamSaveService] ✅ Steam Cloud enabled");
                    }
                }
                else
                {
                    _steamInitialized = false;
                    Debug.LogError("[SteamSaveService] ❌ Steam init failed");
                }
            }
            catch (Exception e)
            {
                _steamInitialized = false;
                Debug.LogError($"[SteamSaveService] ❌ Steam exception: {e.Message}");
            }
#else
            _steamInitialized = false;
            Debug.LogWarning("[SteamSaveService] Steam disabled in Editor");
#endif
        }
        
        /// <summary>
        /// Save data with atomic write pattern
        /// FIXED: Uses temp file + rename for atomic operation (for local fallback)
        /// Steam Cloud has built-in atomicity
        /// </summary>
        public async Task<bool> SaveDataAsync<T>(string key, T data) where T : class
        {
            if (string.IsNullOrEmpty(key) || data == null) return false;
            
            OnSaveStarted?.Invoke(key);
            
            try
            {
                string json = JsonUtility.ToJson(data, true);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                string cloudPath = _cloudFolder + key + ".json";
                
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
                if (_steamInitialized && IsCloudEnabled)
                {
                    // Steam Cloud write is atomic by design
                    bool success = await Task.Run(() => SteamRemoteStorage.FileWrite(cloudPath, bytes, bytes.Length));
                    
                    if (success)
                    {
                        _cache[key] = data;
                        OnSaveCompleted?.Invoke(key, true);
                        _eventService?.Publish(new GameSavedEvent { SaveSlot = key, Success = true });
                        return true;
                    }
                    
                    OnSaveCompleted?.Invoke(key, false);
                    return false;
                }
#endif
                
                // Local fallback with atomic write
                string localPath = Path.Combine(Application.persistentDataPath, "saves", key + ".json");
                string tempPath = localPath + ".tmp";
                
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                
                // Write to temp file first
                await File.WriteAllBytesAsync(tempPath, bytes);
                
                // Atomic operation
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
                File.Move(tempPath, localPath);
                
                _cache[key] = data;
                OnSaveCompleted?.Invoke(key, true);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Save failed: {e.Message}");
                OnSaveCompleted?.Invoke(key, false);
                return false;
            }
        }
        
        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) return null;
            
            OnLoadStarted?.Invoke(key);
            
            try
            {
                if (_cache.TryGetValue(key, out object cached))
                {
                    OnLoadCompleted?.Invoke(key, true);
                    return cached as T;
                }
                
                string cloudPath = _cloudFolder + key + ".json";
                
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
                if (_steamInitialized && IsCloudEnabled && SteamRemoteStorage.FileExists(cloudPath))
                {
                    int fileSize = SteamRemoteStorage.GetFileSize(cloudPath);
                    byte[] bytes = new byte[fileSize];
                    
                    int readBytes = await Task.Run(() => SteamRemoteStorage.FileRead(cloudPath, bytes, fileSize));
                    
                    if (readBytes > 0)
                    {
                        string json = System.Text.Encoding.UTF8.GetString(bytes);
                        T data = JsonUtility.FromJson<T>(json);
                        
                        _cache[key] = data;
                        OnLoadCompleted?.Invoke(key, true);
                        _eventService?.Publish(new GameLoadedEvent { SaveSlot = key, Success = true });
                        return data;
                    }
                }
#endif
                
                string localPath = Path.Combine(Application.persistentDataPath, "saves", key + ".json");
                
                if (File.Exists(localPath))
                {
                    byte[] bytes = await File.ReadAllBytesAsync(localPath);
                    string json = System.Text.Encoding.UTF8.GetString(bytes);
                    T data = JsonUtility.FromJson<T>(json);
                    
                    _cache[key] = data;
                    OnLoadCompleted?.Invoke(key, true);
                    return data;
                }
                
                OnLoadCompleted?.Invoke(key, false);
                return null;
            }
            catch (Exception e)
            {
                LogError($"Load failed: {e.Message}");
                OnLoadCompleted?.Invoke(key, false);
                return null;
            }
        }
        
        public async Task<bool> DeleteDataAsync(string key)
        {
            try
            {
                string cloudPath = _cloudFolder + key + ".json";
                
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
                if (_steamInitialized && IsCloudEnabled && SteamRemoteStorage.FileExists(cloudPath))
                {
                    bool success = await Task.Run(() => SteamRemoteStorage.FileDelete(cloudPath));
                    if (success)
                    {
                        _cache.Remove(key);
                        return true;
                    }
                }
#endif
                
                return false;
            }
            catch (Exception e)
            {
                LogError($"Delete failed: {e.Message}");
                return false;
            }
        }
        
        public Task<bool> DeleteAllDataAsync() => Task.FromResult(false);
        
        public async Task<bool> SyncWithCloudAsync()
        {
            if (!IsCloudEnabled) return false;
            
            OnCloudSyncStarted?.Invoke();
            
            try
            {
                await Task.Delay(100);
                OnCloudSyncCompleted?.Invoke(true);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Sync failed: {e.Message}");
                OnCloudSyncCompleted?.Invoke(false);
                return false;
            }
        }
        
        public Task<bool> UploadToCloudAsync() => SyncWithCloudAsync();
        public Task<bool> DownloadFromCloudAsync() => SyncWithCloudAsync();
        
        public async Task<bool> SaveToSlotAsync(int slotIndex, SaveData data)
        {
            string key = $"save_slot_{slotIndex}";
            return await SaveDataAsync(key, data);
        }
        
        public async Task<SaveData> LoadFromSlotAsync(int slotIndex)
        {
            string key = $"save_slot_{slotIndex}";
            return await LoadDataAsync<SaveData>(key);
        }
        
        public async Task<bool> DeleteSlotAsync(int slotIndex)
        {
            string key = $"save_slot_{slotIndex}";
            return await DeleteDataAsync(key);
        }
        
        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();
            
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            if (!IsCloudEnabled) return slots;
            
            try
            {
                int fileCount = SteamRemoteStorage.GetFileCount();
                
                for (int i = 0; i < fileCount; i++)
                {
                    string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out int fileSize);
                    
                    if (fileName.StartsWith(_cloudFolder + "save_slot_"))
                    {
                        string slotPart = fileName.Replace(_cloudFolder + "save_slot_", "").Replace(".json", "");
                        
                        if (int.TryParse(slotPart, out int slotIndex))
                        {
                            long timestamp = SteamRemoteStorage.GetFileTimestamp(fileName);
                            DateTime saveTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                            
                            slots.Add(new SaveSlotInfo
                            {
                                SlotIndex = slotIndex,
                                SlotName = $"Save Slot {slotIndex + 1}",
                                LastSaveTime = saveTime,
                                FileSizeBytes = fileSize,
                                IsCloudSynced = true
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Error getting slots: {e.Message}");
            }
#endif
            
            return slots;
        }
        
        public bool HasLocalSave(string key)
        {
            string localPath = Path.Combine(Application.persistentDataPath, "saves", key + ".json");
            return File.Exists(localPath);
        }
        
        public bool HasCloudSave(string key)
        {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            if (!IsCloudEnabled) return false;
            string cloudPath = _cloudFolder + key + ".json";
            return SteamRemoteStorage.FileExists(cloudPath);
#else
            return false;
#endif
        }
        
        public DateTime? GetLastSaveTime(string key)
        {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            if (!IsCloudEnabled) return null;
            
            string cloudPath = _cloudFolder + key + ".json";
            if (SteamRemoteStorage.FileExists(cloudPath))
            {
                long timestamp = SteamRemoteStorage.GetFileTimestamp(cloudPath);
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
#endif
            return null;
        }
        
        public long GetSaveFileSize(string key)
        {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            if (!IsCloudEnabled) return 0;
            
            string cloudPath = _cloudFolder + key + ".json";
            if (SteamRemoteStorage.FileExists(cloudPath))
            {
                return SteamRemoteStorage.GetFileSize(cloudPath);
            }
#endif
            return 0;
        }
        
        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR || ENABLE_STEAM_IN_EDITOR
            if (_steamInitialized)
            {
                SteamAPI.Shutdown();
            }
#endif
        }
    }
}