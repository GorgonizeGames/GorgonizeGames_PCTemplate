using System;
using System.Collections.Generic;

namespace Game.Runtime.Core.Data
{
    [Serializable]
    public class SaveData
    {
        public string SaveVersion = "1.0.0";
        public string SaveId;
        public DateTime SaveTime;
        public int SlotIndex = -1;
        
        public PlayerData Player;
        public List<CaseProgress> Cases;
        public List<string> DiscoveredEvidenceIds;
        public List<string> DiscoveredClueIds;
        public List<string> CompletedHackingChallengeIds;
        
        public string CurrentCaseId;
        public float TotalPlayTime;
        
        public GameplaySettings Settings;
        
        public SaveData()
        {
            SaveId = Guid.NewGuid().ToString();
            SaveTime = DateTime.Now;
            Player = new PlayerData();
            Cases = new List<CaseProgress>();
            DiscoveredEvidenceIds = new List<string>();
            DiscoveredClueIds = new List<string>();
            CompletedHackingChallengeIds = new List<string>();
            Settings = new GameplaySettings();
        }
    }
    
    [Serializable]
    public class PlayerData
    {
        public string PlayerName = "Detective";
        public int Level = 1;
        public int Experience = 0;
        public int CasesCompleted = 0;
        public int EvidenceFound = 0;
        public int HacksCompleted = 0;
    }
    
    [Serializable]
    public class CaseProgress
    {
        public string CaseId;
        public CaseStatus Status;
        public float ProgressPercentage;
        public DateTime StartedAt;
        public DateTime? CompletedAt;
        public List<string> UnlockedClueIds;
        public List<string> CollectedEvidenceIds;
    }
    
    public enum CaseStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
    
    [Serializable]
    public class GameplaySettings
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.7f;
        public float SFXVolume = 1f;
        public bool Subtitles = true;
        public bool Hints = true;
    }
}