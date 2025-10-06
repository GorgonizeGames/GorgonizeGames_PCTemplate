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
    public class ClueService : ServiceBase, IClueService
    {
        [Inject(required: false)] private IAudioService _audioService;
        
        private readonly Dictionary<string, Clue> _clueDatabase = new Dictionary<string, Clue>();
        
        public event Action<Clue> OnClueDiscovered;
        
        public override int InitializationPriority => 21;
        protected override string ServiceName => "ClueService";
        
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
            
            return true;
        }
        
        public void AddClue(Clue clue)
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return;
            }
            
            if (clue == null || string.IsNullOrEmpty(clue.Id))
            {
                LogError("Invalid clue");
                return;
            }
            
            if (_clueDatabase.ContainsKey(clue.Id))
            {
                LogWarning($"Clue '{clue.Id}' already exists");
                return;
            }
            
            clue.DiscoveredAt = DateTime.Now;
            clue.IsRead = false;
            _clueDatabase[clue.Id] = clue;
            
            OnClueDiscovered?.Invoke(clue);
            _eventService?.Publish(new ClueDiscoveredEvent 
            { 
                ClueId = clue.Id, 
                Description = clue.Description 
            });
            
            _audioService?.PlayUISound(UISoundType.Notification);
            
            LogInfo($"New clue discovered: {clue.Title}");
        }
        
        public void RemoveClue(string clueId)
        {
            if (_clueDatabase.Remove(clueId))
            {
                LogInfo($"Removed clue: {clueId}");
            }
        }
        
        public Clue GetClue(string clueId)
        {
            _clueDatabase.TryGetValue(clueId, out Clue clue);
            return clue;
        }
        
        public List<Clue> GetAllClues()
        {
            return _clueDatabase.Values.ToList();
        }
        
        public List<Clue> GetCluesByCase(string caseId)
        {
            return _clueDatabase.Values.Where(c => c.CaseId == caseId).ToList();
        }
        
        public void MarkClueAsRead(string clueId)
        {
            if (_clueDatabase.TryGetValue(clueId, out Clue clue))
            {
                clue.IsRead = true;
                LogInfo($"Clue marked as read: {clueId}");
            }
        }
        
        public bool HasUnreadClues()
        {
            return _clueDatabase.Values.Any(c => !c.IsRead);
        }
    }
}