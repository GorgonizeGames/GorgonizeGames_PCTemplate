using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;
using Game.Runtime.Investigation.Services;

namespace Game.Runtime.Hacking.Services
{
    public class HackingService : ServiceBase, IHackingService
    {
        [Inject(required: false)] private IAudioService _audioService;
        [Inject(required: false)] private IEvidenceService _evidenceService;
        
        private HackingChallenge _currentChallenge;
        private readonly HashSet<string> _completedChallenges = new HashSet<string>();
        
        public event Action<HackingChallenge> OnChallengeStarted;
        public event Action<string, bool> OnChallengeCompleted;
        
        public override int InitializationPriority => 22;
        protected override string ServiceName => "HackingService";
        
        protected override async Task OnInitializeAsync()
        {
            await Task.CompletedTask;
        }
        
        protected override bool ValidateDependencies()
        {
            if (_eventService == null)
            {
                LogWarning("EventService not available");
            }
            
            if (_audioService == null)
            {
                LogWarning("AudioService not available - no sound effects");
            }
            
            if (_evidenceService == null)
            {
                LogWarning("EvidenceService not available - rewards won't work");
            }
            
            return true;
        }
        
        public void StartHackingChallenge(HackingChallenge challenge)
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return;
            }
            
            if (challenge == null)
            {
                LogError("Invalid challenge");
                return;
            }
            
            if (_currentChallenge != null)
            {
                LogWarning("Already in a challenge");
                return;
            }
            
            _currentChallenge = challenge;
            
            OnChallengeStarted?.Invoke(challenge);
            _eventService?.Publish(new HackingStartedEvent 
            { 
                TargetId = challenge.TargetId, 
                HackType = challenge.Type.ToString() 
            });
            
            LogInfo($"Started challenge: {challenge.Name}");
        }
        
        public void CompleteChallenge(string challengeId, bool success)
        {
            if (_currentChallenge == null || _currentChallenge.Id != challengeId)
            {
                LogWarning("No active challenge or ID mismatch");
                return;
            }
            
            if (success)
            {
                _completedChallenges.Add(challengeId);
                
                if (!string.IsNullOrEmpty(_currentChallenge.RewardEvidenceId) && _evidenceService != null)
                {
                    LogInfo($"Reward evidence unlocked: {_currentChallenge.RewardEvidenceId}");
                }
                
                _audioService?.PlayUISound(UISoundType.Success);
                _eventService?.Publish(new HackingCompletedEvent 
                { 
                    TargetId = _currentChallenge.TargetId, 
                    Success = true 
                });
                
                LogInfo($"Challenge completed: {challengeId}");
            }
            else
            {
                _audioService?.PlayUISound(UISoundType.Error);
                _eventService?.Publish(new HackingCompletedEvent 
                { 
                    TargetId = _currentChallenge.TargetId, 
                    Success = false 
                });
                
                LogInfo($"Challenge failed: {challengeId}");
            }
            
            OnChallengeCompleted?.Invoke(challengeId, success);
            _currentChallenge = null;
        }
        
        public void FailChallenge(string challengeId, string reason)
        {
            if (_currentChallenge == null || _currentChallenge.Id != challengeId) return;
            
            _audioService?.PlayUISound(UISoundType.Error);
            _eventService?.Publish(new HackingFailedEvent 
            { 
                TargetId = _currentChallenge.TargetId, 
                Reason = reason 
            });
            
            _currentChallenge = null;
            
            LogInfo($"Challenge failed: {challengeId} - Reason: {reason}");
        }
        
        public bool IsChallengeCompleted(string challengeId)
        {
            return _completedChallenges.Contains(challengeId);
        }
        
        public List<HackingChallenge> GetCompletedChallenges()
        {
            return new List<HackingChallenge>();
        }
    }
}