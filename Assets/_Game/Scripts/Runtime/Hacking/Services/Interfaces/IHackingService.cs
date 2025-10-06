using System;
using System.Collections.Generic;

namespace Game.Runtime.Hacking.Services
{
    public interface IHackingService
    {
        void StartHackingChallenge(HackingChallenge challenge);
        void CompleteChallenge(string challengeId, bool success);
        void FailChallenge(string challengeId, string reason);
        bool IsChallengeCompleted(string challengeId);
        List<HackingChallenge> GetCompletedChallenges();
        event Action<HackingChallenge> OnChallengeStarted;
        event Action<string, bool> OnChallengeCompleted;
    }
    
    [Serializable]
    public class HackingChallenge
    {
        public string Id;
        public string Name;
        public string Description;
        public HackingType Type;
        public int Difficulty;
        public float TimeLimit;
        public string[] RequiredTools;
        public string TargetId;
        public string RewardEvidenceId;
    }
    
    public enum HackingType
    {
        PasswordCracking,
        NetworkAnalysis,
        SocialEngineering,
        DataMining,
        Phishing,
        SQLInjection,
        PortScanning
    }
}