using System;
using System.Collections.Generic;

namespace Game.Runtime.Investigation.Services
{
    public interface IClueService
    {
        void AddClue(Clue clue);
        void RemoveClue(string clueId);
        Clue GetClue(string clueId);
        List<Clue> GetAllClues();
        List<Clue> GetCluesByCase(string caseId);
        void MarkClueAsRead(string clueId);
        bool HasUnreadClues();
        event Action<Clue> OnClueDiscovered;
    }
    
    [Serializable]
    public class Clue
    {
        public string Id;
        public string CaseId;
        public string Title;
        public string Description;
        public string[] RelatedEvidenceIds;
        public bool IsRead;
        public DateTime DiscoveredAt;
    }
}