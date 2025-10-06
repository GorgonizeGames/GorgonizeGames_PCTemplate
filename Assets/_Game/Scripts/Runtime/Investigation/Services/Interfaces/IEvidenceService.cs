using System;
using System.Collections.Generic;

namespace Game.Runtime.Investigation.Services
{
    public interface IEvidenceService
    {
        void AddEvidence(Evidence evidence);
        void RemoveEvidence(string evidenceId);
        Evidence GetEvidence(string evidenceId);
        List<Evidence> GetAllEvidence();
        List<Evidence> GetEvidenceByCase(string caseId);
        bool HasEvidence(string evidenceId);
        int GetEvidenceCount();
        event Action<Evidence> OnEvidenceAdded;
        event Action<string> OnEvidenceRemoved;
    }
    
    [Serializable]
    public class Evidence
    {
        public string Id;
        public string CaseId;
        public string Name;
        public string Description;
        public EvidenceType Type;
        public string SourceUrl;
        public string FilePath;
        public DateTime DiscoveredAt;
        public bool IsAnalyzed;
        public List<string> Tags;
    }
    
    public enum EvidenceType
    {
        Document,
        Image,
        Email,
        WebPage,
        SocialMediaPost,
        PhoneRecord,
        BankTransaction,
        Other
    }
}