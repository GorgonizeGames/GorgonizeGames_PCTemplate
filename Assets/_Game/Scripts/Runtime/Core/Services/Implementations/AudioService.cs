using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.Events;
using Game.Runtime.Utilities;

namespace Game.Runtime.Core.Services
{
    public class AudioService : ServiceBase, IAudioService
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        
        [Header("UI Sounds")]
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _hoverSound;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _errorSound;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] private AudioClip _typingSound;
        
        [Header("Settings")]
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private bool _loadAudioFromResources = true;
        
        private Dictionary<string, AudioClip> _musicLibrary = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _sfxLibrary = new Dictionary<string, AudioClip>();
        private Coroutine _musicFadeCoroutine;
        
        public override int InitializationPriority => 5;
        protected override string ServiceName => "AudioService";
        
        private void Awake()
        {
            CreateAudioSources();
        }
        
        protected override async Task OnInitializeAsync()
        {
            if (_loadAudioFromResources)
            {
                LoadAudioLibraries();  // ✅ FIXED: Sync method on main thread
            }
            
            await Task.CompletedTask;
            
            LogInfo($"Initialized with {_musicLibrary.Count} music, {_sfxLibrary.Count} SFX");
        }
        
        protected override bool ValidateDependencies()
        {
            if (_eventService == null)
            {
                LogWarning("EventService not available - some features may not work");
            }
            
            return true;
        }
        
        private void CreateAudioSources()
        {
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }
            
            if (_sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                _sfxSource = sfxObj.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }
            
            UpdateVolumes();
        }
        
        /// <summary>
        /// ✅ FIXED: Sync method - runs on main thread
        /// Resources.LoadAll MUST run on Unity main thread
        /// </summary>
        private void LoadAudioLibraries()
        {
            // Load music from Resources/Audio/Music folder
            AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
            foreach (var clip in musicClips)
            {
                _musicLibrary[clip.name] = clip;
            }
            
            // Load SFX from Resources/Audio/SFX folder
            AudioClip[] sfxClips = Resources.LoadAll<AudioClip>("Audio/SFX");
            foreach (var clip in sfxClips)
            {
                _sfxLibrary[clip.name] = clip;
            }
        }
        
        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
            }
        }
        
        public void PlayMusic(string musicId, bool loop = true, float fadeInDuration = 1f)
        {
            EnsureInitialized();
            
            if (!_musicLibrary.TryGetValue(musicId, out AudioClip clip))
            {
                LogWarning($"Music '{musicId}' not found in library");
                return;
            }
            
            if (_musicFadeCoroutine != null)
            {
                Coroutines.Stop(_musicFadeCoroutine);
            }
            
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
            
            float targetVolume = _musicVolume * _masterVolume;
            _musicFadeCoroutine = Coroutines.Lerp(
                fadeInDuration,
                t => _musicSource.volume = Mathf.Lerp(0f, targetVolume, t)
            );
            
            _eventService?.Publish(new MusicStartedEvent { MusicId = musicId });
        }
        
        public void StopMusic(float fadeOutDuration = 1f)
        {
            if (_musicSource.isPlaying)
            {
                string currentMusic = _musicSource.clip?.name;
                
                if (_musicFadeCoroutine != null)
                {
                    Coroutines.Stop(_musicFadeCoroutine);
                }
                
                float startVolume = _musicSource.volume;
                
                _musicFadeCoroutine = Coroutines.Lerp(
                    fadeOutDuration,
                    t => _musicSource.volume = Mathf.Lerp(startVolume, 0f, t),
                    () =>
                    {
                        _musicSource.Stop();
                        _eventService?.Publish(new MusicStoppedEvent { MusicId = currentMusic });
                    }
                );
            }
        }
        
        public void PlaySound(string soundId, float volume = 1f)
        {
            EnsureInitialized();
            
            if (!_sfxLibrary.TryGetValue(soundId, out AudioClip clip))
            {
                LogWarning($"Sound '{soundId}' not found in library");
                return;
            }
            
            _sfxSource.PlayOneShot(clip, volume * _sfxVolume * _masterVolume);
            _eventService?.Publish(new SoundPlayedEvent { SoundId = soundId });
        }
        
        public void PlayUISound(UISoundType soundType)
        {
            AudioClip clip = soundType switch
            {
                UISoundType.Click => _clickSound,
                UISoundType.Hover => _hoverSound,
                UISoundType.Open => _openSound,
                UISoundType.Close => _closeSound,
                UISoundType.Error => _errorSound,
                UISoundType.Success => _successSound,
                UISoundType.Notification => _notificationSound,
                UISoundType.Typing => _typingSound,
                _ => null
            };
            
            if (clip != null) 
            {
                _sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public float GetMasterVolume() => _masterVolume;
        public float GetMusicVolume() => _musicVolume;
        public float GetSFXVolume() => _sfxVolume;
        
        private void UpdateVolumes()
        {
            if (_musicSource != null && !_musicSource.isPlaying)
            {
                _musicSource.volume = _musicVolume * _masterVolume;
            }
        }
    }
}