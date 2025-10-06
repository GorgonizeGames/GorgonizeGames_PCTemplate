using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public interface IAudioService
    {
        void PlayMusic(string musicId, bool loop = true, float fadeInDuration = 1f);
        void StopMusic(float fadeOutDuration = 1f);
        void PlaySound(string soundId, float volume = 1f);
        void PlayUISound(UISoundType soundType);
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        float GetMasterVolume();
        float GetMusicVolume();
        float GetSFXVolume();
    }
    
    public enum UISoundType
    {
        Click,
        Hover,
        Open,
        Close,
        Error,
        Success,
        Notification,
        Typing
    }
}