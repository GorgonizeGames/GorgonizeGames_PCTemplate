using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.UI.Windows;
using Game.Runtime.Core.Events;
using Game.Runtime.Investigation.Services;

namespace Game.Runtime.Desktop
{
    public class DesktopController : MonoBehaviour
    {
        [Header("Desktop Icons")]
        [SerializeField] private Button browserIcon;
        [SerializeField] private Button emailIcon;
        [SerializeField] private Button fileExplorerIcon;
        
        [Inject] private IWindowManager _windowManager;
        [Inject] private IAudioService _audioService;
        [Inject] private IEventService _eventService;
        [Inject] private IInputService _inputService;
        [Inject] private IEvidenceService _evidenceService;
        [Inject] private IClueService _clueService;
        
        private bool _isInitialized = false;
        
        private async void Start()
        {
            Dependencies.Inject(this);
            
            // Servislerin initialize olmasını bekle
            await WaitForServicesAsync();
            
            SetupIconButtons();
            SubscribeToEvents();
            SubscribeToInputEvents();
            
            _isInitialized = true;
            Debug.Log("[DesktopController] ✅ Initialized");
        }
        
        private async Task WaitForServicesAsync()
        {
            int maxWaitTime = 5000; // 5 saniye
            int waitedTime = 0;
            int checkInterval = 100;
            
            while (!AreServicesReady() && waitedTime < maxWaitTime)
            {
                await Task.Delay(checkInterval);
                waitedTime += checkInterval;
            }
            
            if (waitedTime >= maxWaitTime)
            {
                Debug.LogWarning("[DesktopController] Timeout waiting for services!");
            }
        }
        
        private bool AreServicesReady()
        {
            // Critical servislerin hazır olup olmadığını kontrol et
            if (_windowManager is IInitializable windowManagerInit && !windowManagerInit.IsInitialized)
                return false;
            
            if (_audioService is IInitializable audioServiceInit && !audioServiceInit.IsInitialized)
                return false;
            
            if (_eventService is IInitializable eventServiceInit && !eventServiceInit.IsInitialized)
                return false;
            
            return true;
        }
        
        private void SetupIconButtons()
        {
            if (browserIcon != null)
                browserIcon.onClick.AddListener(OnBrowserIconClicked);
            
            if (emailIcon != null)
                emailIcon.onClick.AddListener(OnEmailIconClicked);
            
            if (fileExplorerIcon != null)
                fileExplorerIcon.onClick.AddListener(OnFileExplorerIconClicked);
        }
        
        private void SubscribeToEvents()
        {
            if (_eventService == null)
            {
                Debug.LogWarning("[DesktopController] EventService not available!");
                return;
            }
            
            _eventService.Subscribe<WindowOpenedEvent>(OnWindowOpened);
            _eventService.Subscribe<WindowClosedEvent>(OnWindowClosed);
            _eventService.Subscribe<EvidenceFoundEvent>(OnEvidenceFound);
            _eventService.Subscribe<ClueDiscoveredEvent>(OnClueDiscovered);
            _eventService.Subscribe<HackingCompletedEvent>(OnHackingCompleted);
        }
        
        private void SubscribeToInputEvents()
        {
            if (_inputService != null)
            {
                _inputService.OnEscapePressed += OnEscapePressed;
            }
        }
        
        private void OnBrowserIconClicked()
        {
            if (!_isInitialized) return;
            
            _audioService?.PlayUISound(UISoundType.Click);
            _windowManager?.OpenWindow<BrowserWindow>();
        }
        
        private void OnEmailIconClicked()
        {
            if (!_isInitialized) return;
            
            _audioService?.PlayUISound(UISoundType.Click);
            _windowManager?.OpenWindow<EmailWindow>();
        }
        
        private void OnFileExplorerIconClicked()
        {
            if (!_isInitialized) return;
            
            _audioService?.PlayUISound(UISoundType.Click);
            _windowManager?.OpenWindow<FileExplorerWindow>();
        }
        
        private void OnWindowOpened(WindowOpenedEvent evt)
        {
            Debug.Log($"[Desktop] Window opened: {evt.WindowId}");
        }
        
        private void OnWindowClosed(WindowClosedEvent evt)
        {
            Debug.Log($"[Desktop] Window closed: {evt.WindowId}");
        }
        
        private void OnEvidenceFound(EvidenceFoundEvent evt)
        {
            Debug.Log($"[Desktop] 🔍 Evidence found: {evt.EvidenceId}");
            _audioService?.PlayUISound(UISoundType.Notification);
        }
        
        private void OnClueDiscovered(ClueDiscoveredEvent evt)
        {
            Debug.Log($"[Desktop] 💡 Clue discovered: {evt.ClueId}");
            _audioService?.PlayUISound(UISoundType.Success);
        }
        
        private void OnHackingCompleted(HackingCompletedEvent evt)
        {
            if (evt.Success)
            {
                Debug.Log($"[Desktop] ✅ Hacking successful");
            }
            else
            {
                Debug.Log($"[Desktop] ❌ Hacking failed");
            }
        }
        
        private void OnEscapePressed()
        {
            if (_windowManager == null) return;
            
            var openWindows = _windowManager.GetOpenWindows();
            
            if (openWindows != null && openWindows.Count > 0)
            {
                var topWindow = openWindows[openWindows.Count - 1];
                topWindow.Close();
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Debug: Add Test Evidence")]
        private void DebugAddTestEvidence()
        {
            if (_evidenceService == null)
            {
                Debug.LogError("EvidenceService not available!");
                return;
            }
            
            var evidence = new Evidence
            {
                Id = $"test_evidence_{System.Guid.NewGuid()}",
                CaseId = "case_001",
                Name = "Test Evidence",
                Description = "Test evidence for debugging",
                Type = EvidenceType.Document
            };
            
            _evidenceService.AddEvidence(evidence);
        }
        
        [ContextMenu("Debug: Add Test Clue")]
        private void DebugAddTestClue()
        {
            if (_clueService == null)
            {
                Debug.LogError("ClueService not available!");
                return;
            }
            
            var clue = new Clue
            {
                Id = $"test_clue_{System.Guid.NewGuid()}",
                CaseId = "case_001",
                Title = "Test Clue",
                Description = "Test clue for debugging"
            };
            
            _clueService.AddClue(clue);
        }
#endif
        
        private void OnDestroy()
        {
            if (browserIcon != null) browserIcon.onClick.RemoveListener(OnBrowserIconClicked);
            if (emailIcon != null) emailIcon.onClick.RemoveListener(OnEmailIconClicked);
            if (fileExplorerIcon != null) fileExplorerIcon.onClick.RemoveListener(OnFileExplorerIconClicked);
            
            if (_eventService != null)
            {
                _eventService.Unsubscribe<WindowOpenedEvent>(OnWindowOpened);
                _eventService.Unsubscribe<WindowClosedEvent>(OnWindowClosed);
                _eventService.Unsubscribe<EvidenceFoundEvent>(OnEvidenceFound);
                _eventService.Unsubscribe<ClueDiscoveredEvent>(OnClueDiscovered);
                _eventService.Unsubscribe<HackingCompletedEvent>(OnHackingCompleted);
            }
            
            if (_inputService != null)
            {
                _inputService.OnEscapePressed -= OnEscapePressed;
            }
        }
    }
}