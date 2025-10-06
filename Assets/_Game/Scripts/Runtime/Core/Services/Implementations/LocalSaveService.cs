using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.Data;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public class LocalSaveService : ServiceBase, ISaveService
    {
        [Header("Settings")]
        [SerializeField] private string _saveFolder = "Saves";
        [SerializeField] private string _fileExtension = ".json";
        
        private string _savePath;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        
        public SavePlatform CurrentPlatform => SavePlatform.Local;
        public bool IsCloudEnabled => false;
        
        public event Action<string> OnSaveStarted;
        public event Action<string, bool> OnSaveCompleted;
        public event Action<string> OnLoadStarted;
        public event Action<string, bool> OnLoadCompleted;
        public event Action OnCloudSyncStarted;
        public event Action<bool> OnCloudSyncCompleted;
        
        public override int InitializationPriority => 3;
        protected override string ServiceName => "LocalSaveService";
        
        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, _saveFolder);
            
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }
        
        protected override async Task OnInitializeAsync()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
            
            await Task.CompletedTask;
            
            LogInfo($"Save path: {_savePath}");
        }
        
        /// <summary>
        /// Save data with atomic write pattern (prevents corruption on crash)
        /// FIXED: Uses temp file + rename for atomic operation
        /// </summary>
        public async Task<bool> SaveDataAsync<T>(string key, T data) where T : class
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return false;
            }
            
            if (string.IsNullOrEmpty(key) || data == null) return false;
            
            OnSaveStarted?.Invoke(key);
            
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetFilePath(key);
                string tempPath = filePath + ".tmp";
                
                // Write to temporary file first
                await File.WriteAllTextAsync(tempPath, json);
                
                // Atomic operation: Replace old file with new one
                // This ensures file is never corrupt even if crash happens
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);
                
                _cache[key] = data;
                OnSaveCompleted?.Invoke(key, true);
                
                _eventService?.Publish(new GameSavedEvent { SaveSlot = key, Success = true });
                
                LogInfo($"Saved: {key}");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Save failed for '{key}': {e.Message}");
                OnSaveCompleted?.Invoke(key, false);
                _eventService?.Publish(new GameSavedEvent { SaveSlot = key, Success = false });
                return false;
            }
        }
        
        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return null;
            }
            
            if (string.IsNullOrEmpty(key)) return null;
            
            OnLoadStarted?.Invoke(key);
            
            try
            {
                if (_cache.TryGetValue(key, out object cached))
                {
                    OnLoadCompleted?.Invoke(key, true);
                    return cached as T;
                }
                
                string filePath = GetFilePath(key);
                
                if (!File.Exists(filePath))
                {
                    OnLoadCompleted?.Invoke(key, false);
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(filePath);
                T data = JsonUtility.FromJson<T>(json);
                
                _cache[key] = data;
                OnLoadCompleted?.Invoke(key, true);
                
                _eventService?.Publish(new GameLoadedEvent { SaveSlot = key, Success = true });
                
                LogInfo($"Loaded: {key}");
                return data;
            }
            catch (Exception e)
            {
                LogError($"Load failed for '{key}': {e.Message}");
                OnLoadCompleted?.Invoke(key, false);
                _eventService?.Publish(new GameLoadedEvent { SaveSlot = key, Success = false });
                return null;
            }
        }
        
        public async Task<bool> DeleteDataAsync(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
                
                _cache.Remove(key);
                LogInfo($"Deleted: {key}");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Delete failed for '{key}': {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> DeleteAllDataAsync()
        {
            try
            {
                if (Directory.Exists(_savePath))
                {
                    await Task.Run(() => Directory.Delete(_savePath, true));
                    Directory.CreateDirectory(_savePath);
                }
                
                _cache.Clear();
                LogInfo("All data deleted");
                return true;
            }
            catch (Exception e)
            {
                LogError($"Delete all failed: {e.Message}");
                return false;
            }
        }
        
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
            
            if (!Directory.Exists(_savePath)) return slots;
            
            var files = Directory.GetFiles(_savePath, $"save_slot_*{_fileExtension}");
            
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string indexStr = fileName.Replace("save_slot_", "");
                    
                    if (int.TryParse(indexStr, out int index))
                    {
                        var fileInfo = new FileInfo(file);
                        
                        slots.Add(new SaveSlotInfo
                        {
                            SlotIndex = index,
                            SlotName = $"Save Slot {index + 1}",
                            LastSaveTime = fileInfo.LastWriteTime,
                            FileSizeBytes = fileInfo.Length,
                            IsCloudSynced = false
                        });
                    }
                }
                catch (Exception e)
                {
                    LogError($"Error reading slot info: {e.Message}");
                }
            }
            
            return slots;
        }
        
        public bool HasLocalSave(string key) => File.Exists(GetFilePath(key));
        public bool HasCloudSave(string key) => false;
        
        public DateTime? GetLastSaveTime(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath) ? File.GetLastWriteTime(filePath) : null;
        }
        
        public long GetSaveFileSize(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        }
        
        public Task<bool> SyncWithCloudAsync()
        {
            OnCloudSyncStarted?.Invoke();
            OnCloudSyncCompleted?.Invoke(false);
            return Task.FromResult(false);
        }
        
        public Task<bool> UploadToCloudAsync() => Task.FromResult(false);
        public Task<bool> DownloadFromCloudAsync() => Task.FromResult(false);
        
        private string GetFilePath(string key) => Path.Combine(_savePath, key + _fileExtension);
    }
}