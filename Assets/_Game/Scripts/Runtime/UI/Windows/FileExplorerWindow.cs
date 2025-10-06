using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Runtime.Core.Services;

namespace Game.Runtime.UI.Windows
{
    public class FileExplorerWindow : WindowBase
    {
        [Header("File Explorer Components")]
        [SerializeField] private TMP_InputField pathInput;
        [SerializeField] private Button backButton;
        [SerializeField] private Button upButton;
        [SerializeField] private Transform fileListContainer;
        
        private string _currentPath = "C:/Users/Player/";
        private List<FileItem> _currentFiles = new List<FileItem>();
        
        protected override void Awake()
        {
            base.Awake();
            windowTitle = "File Explorer";
        }
        
        protected override void SetupButtons()
        {
            base.SetupButtons();
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            
            if (upButton != null)
                upButton.onClick.AddListener(OnUpClicked);
            
            if (pathInput != null)
                pathInput.onSubmit.AddListener(OnPathSubmit);
        }
        
        protected override void OnOpen()
        {
            base.OnOpen();
            NavigateToPath(_currentPath);
        }
        
        public void NavigateToPath(string path)
        {
            _currentPath = path;
            if (pathInput != null) pathInput.text = path;
            
            LoadDirectoryContents(path);
            _audioService?.PlayUISound(UISoundType.Click);
        }
        
        private void LoadDirectoryContents(string path)
        {
            _currentFiles.Clear();
            
            _currentFiles.Add(new FileItem { Name = "Documents", IsFolder = true });
            _currentFiles.Add(new FileItem { Name = "Downloads", IsFolder = true });
            _currentFiles.Add(new FileItem { Name = "secret.txt", IsFolder = false, Size = 1024 });
        }
        
        private void OnBackClicked() => _audioService?.PlayUISound(UISoundType.Click);
        private void OnUpClicked()
        {
            if (_currentPath.Length > 3)
            {
                int lastSlash = _currentPath.LastIndexOf('/', _currentPath.Length - 2);
                if (lastSlash > 0)
                {
                    string parentPath = _currentPath.Substring(0, lastSlash + 1);
                    NavigateToPath(parentPath);
                }
            }
        }
        
        private void OnPathSubmit(string path) => NavigateToPath(path);
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
            if (upButton != null) upButton.onClick.RemoveListener(OnUpClicked);
            if (pathInput != null) pathInput.onSubmit.RemoveListener(OnPathSubmit);
        }
    }
    
    [System.Serializable]
    public class FileItem
    {
        public string Name;
        public bool IsFolder;
        public long Size;
        public string Extension;
    }
}