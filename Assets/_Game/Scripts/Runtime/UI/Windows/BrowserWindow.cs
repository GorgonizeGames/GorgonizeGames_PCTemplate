using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Runtime.Core.Services;

namespace Game.Runtime.UI.Windows
{
    public class BrowserWindow : WindowBase
    {
        [Header("Browser Components")]
        [SerializeField] private TMP_InputField urlInput;
        [SerializeField] private Button backButton;
        [SerializeField] private Button forwardButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TextMeshProUGUI contentText;
        
        private string _currentUrl;
        
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "Web Browser";
        }
        
        protected override void SetupButtons()
        {
            base.SetupButtons();
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            
            if (forwardButton != null)
                forwardButton.onClick.AddListener(OnForwardClicked);
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);
            
            if (urlInput != null)
                urlInput.onSubmit.AddListener(OnUrlSubmit);
        }
        
        protected override void OnOpen()
        {
            base.OnOpen();
            NavigateTo("https://www.searchengine.com");
        }
        
        public void NavigateTo(string url)
        {
            _currentUrl = url;
            if (urlInput != null) urlInput.text = url;
            
            LoadWebsiteContent(url);
            _audioService?.PlayUISound(UISoundType.Click);
        }
        
        private void LoadWebsiteContent(string url)
        {
            if (contentText != null)
            {
                contentText.text = $"Loading content from: {url}\n\n[Website content will be loaded here]";
            }
        }
        
        private void OnBackClicked() => _audioService?.PlayUISound(UISoundType.Click);
        private void OnForwardClicked() => _audioService?.PlayUISound(UISoundType.Click);
        private void OnRefreshClicked()
        {
            _audioService?.PlayUISound(UISoundType.Click);
            LoadWebsiteContent(_currentUrl);
        }
        
        private void OnUrlSubmit(string url) => NavigateTo(url);
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
            if (forwardButton != null) forwardButton.onClick.RemoveListener(OnForwardClicked);
            if (refreshButton != null) refreshButton.onClick.RemoveListener(OnRefreshClicked);
            if (urlInput != null) urlInput.onSubmit.RemoveListener(OnUrlSubmit);
        }
    }
}