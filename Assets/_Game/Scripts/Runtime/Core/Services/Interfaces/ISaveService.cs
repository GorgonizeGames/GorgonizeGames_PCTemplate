using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Runtime.Core.Data;

namespace Game.Runtime.Core.Services
{
    public interface ISaveService
    {
        SavePlatform CurrentPlatform { get; }
        bool IsCloudEnabled { get; }
        
        Task<bool> SaveDataAsync<T>(string key, T data) where T : class;
        Task<T> LoadDataAsync<T>(string key) where T : class;
        Task<bool> DeleteDataAsync(string key);
        Task<bool> DeleteAllDataAsync();
        
        Task<bool> SyncWithCloudAsync();
        Task<bool> UploadToCloudAsync();
        Task<bool> DownloadFromCloudAsync();
        
        bool HasLocalSave(string key);
        bool HasCloudSave(string key);
        DateTime? GetLastSaveTime(string key);
        long GetSaveFileSize(string key);
        
        Task<bool> SaveToSlotAsync(int slotIndex, SaveData data);
        Task<SaveData> LoadFromSlotAsync(int slotIndex);
        Task<bool> DeleteSlotAsync(int slotIndex);
        List<SaveSlotInfo> GetAllSaveSlots();
        
        event Action<string> OnSaveStarted;
        event Action<string, bool> OnSaveCompleted;
        event Action<string> OnLoadStarted;
        event Action<string, bool> OnLoadCompleted;
        event Action OnCloudSyncStarted;
        event Action<bool> OnCloudSyncCompleted;
    }
    
    public enum SavePlatform
    {
        Local,
        SteamCloud,
        EpicCloud,
        GOGCloud
    }
    
    [Serializable]
    public class SaveSlotInfo
    {
        public int SlotIndex;
        public string SlotName;
        public DateTime LastSaveTime;
        public long FileSizeBytes;
        public bool IsCloudSynced;
        public string CaseName;
        public float ProgressPercentage;
    }
}
