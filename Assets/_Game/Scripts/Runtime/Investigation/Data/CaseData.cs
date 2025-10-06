using UnityEngine;
using System.Collections.Generic;
using Game.Runtime.Investigation.Services;

namespace Game.Runtime.Investigation.Data
{
    [CreateAssetMenu(fileName = "New Case", menuName = "Game/Investigation/Case")]
    public class CaseData : ScriptableObject
    {
        [Header("Case Information")]
        public string caseId;
        public string caseName;
        [TextArea(3, 10)]
        public string description;
        public CaseDifficulty difficulty;
        
        [Header("Case Content")]
        public List<Evidence> requiredEvidence;
        public List<Clue> availableClues;
        
        [Header("Completion")]
        public int minimumEvidenceRequired = 3;
        public List<string> correctConclusionKeywords;
    }
    
    public enum CaseDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
}