using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Investigation.Services
{
    public class EvidenceService : ServiceBase, IEvidenceService
    {
        [Inject(required: false)] private ISaveService _saveService;

        private Dictionary<string, List<Evidence>> _evidenceByCase = new();
        private readonly Dictionary<string, Evidence> _evidenceDatabase = new Dictionary<string, Evidence>();

        public event Action<Evidence> OnEvidenceAdded;
        public event Action<string> OnEvidenceRemoved;
        
        public override int InitializationPriority => 20;
        protected override string ServiceName => "EvidenceService";

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
            
            if (_saveService == null)
            {
                LogWarning("SaveService not available - evidence won't be persisted");
            }
            
            return true;
        }

        public void AddEvidence(Evidence evidence)
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return;
            }
            
            if (evidence == null || string.IsNullOrEmpty(evidence.Id))
            {
                LogError("Invalid evidence");
                return;
            }

            if (_evidenceDatabase.ContainsKey(evidence.Id))
            {
                LogWarning($"Evidence '{evidence.Id}' already exists");
                return;
            }

            evidence.DiscoveredAt = DateTime.Now;
            _evidenceDatabase[evidence.Id] = evidence;
            
            _evidenceByCase.Clear();

            OnEvidenceAdded?.Invoke(evidence);
            _eventService?.Publish(new EvidenceFoundEvent
            {
                EvidenceId = evidence.Id,
                CaseId = evidence.CaseId,
                Description = evidence.Description
            });

            LogInfo($"Added evidence: {evidence.Name}");
        }

        public void RemoveEvidence(string evidenceId)
        {
            if (_evidenceDatabase.Remove(evidenceId))
            {
                _evidenceByCase.Clear();
                OnEvidenceRemoved?.Invoke(evidenceId);
                LogInfo($"Removed evidence: {evidenceId}");
            }
        }

        public Evidence GetEvidence(string evidenceId)
        {
            _evidenceDatabase.TryGetValue(evidenceId, out Evidence evidence);
            return evidence;
        }

        public List<Evidence> GetAllEvidence()
        {
            return _evidenceDatabase.Values.ToList();
        }

        public List<Evidence> GetEvidenceByCase(string caseId)
        {
            if (!_evidenceByCase.TryGetValue(caseId, out var evidences))
            {
                evidences = _evidenceDatabase.Values
                    .Where(e => e.CaseId == caseId)
                    .ToList();
                _evidenceByCase[caseId] = evidences;
            }
            return evidences;
        }

        public bool HasEvidence(string evidenceId)
        {
            return _evidenceDatabase.ContainsKey(evidenceId);
        }

        public int GetEvidenceCount()
        {
            return _evidenceDatabase.Count;
        }
    }
}