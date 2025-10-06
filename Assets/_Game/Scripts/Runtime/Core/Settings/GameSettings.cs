using UnityEngine;

namespace Game.Runtime.Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Display")]
        public int TargetFrameRate = 60;
        public bool VSync = true;
        public bool Fullscreen = true;
        
        [Header("Audio")]
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SFXVolume = 1f;
        
        [Header("Gameplay")]
        public float AutoSaveInterval = 300f;
        public bool EnableTutorial = true;
        
        [Header("Debug")]
        public bool ShowDebugInfo = false;
        public bool EnableCheats = false;
        public bool EnableConsole = false;
        
        [Header("Accessibility")]
        public bool EnableSubtitles = true;
        public bool EnableColorblindMode = false;
    }
}