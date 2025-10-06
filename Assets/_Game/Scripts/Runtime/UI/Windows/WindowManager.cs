using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.UI.Windows
{
    public class WindowManager : ServiceBase, IWindowManager
    {
        [Header("Settings")]
        [SerializeField] private int baseZOrder = 100;
        [SerializeField] private int zOrderIncrement = 10;
        
        [Inject(required: false)] private IAudioService _audioService;
        
        private readonly Dictionary<string, IWindow> _registeredWindows = new Dictionary<string, IWindow>();
        private readonly List<IWindow> _openWindows = new List<IWindow>();
        private int _currentMaxZOrder;
        
        public override int InitializationPriority => 60;
        protected override string ServiceName => "WindowManager";
        
        protected override async Task OnInitializeAsync()
        {
            _currentMaxZOrder = baseZOrder;
            AutoRegisterWindows();
            
            await Task.CompletedTask;
            
            LogInfo($"Registered {_registeredWindows.Count} windows");
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
        
        private void AutoRegisterWindows()
        {
            WindowBase[] windows = FindObjectsOfType<WindowBase>(true);
            foreach (var window in windows)
            {
                RegisterWindow(window);
            }
            
            LogInfo($"Auto-registered {windows.Length} windows");
        }
        
        public void RegisterWindow(IWindow window)
        {
            if (window == null)
            {
                LogWarning("Cannot register null window");
                return;
            }
            
            if (!_registeredWindows.ContainsKey(window.WindowId))
            {
                _registeredWindows[window.WindowId] = window;
                LogInfo($"Registered window: {window.WindowId}");
            }
        }
        
        public void UnregisterWindow(IWindow window)
        {
            if (window != null && _registeredWindows.ContainsKey(window.WindowId))
            {
                _registeredWindows.Remove(window.WindowId);
                _openWindows.Remove(window);
                LogInfo($"Unregistered window: {window.WindowId}");
            }
        }
        
        public void OpenWindow(string windowId)
        {
            if (!IsInitialized)
            {
                LogWarning("Service not initialized!");
                return;
            }
            
            if (!_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                LogWarning($"Window '{windowId}' not found");
                return;
            }
            
            if (window.IsOpen)
            {
                BringToFront(window);
                return;
            }
            
            window.ZOrder = GetNextZOrder();
            window.Open();
            
            if (!_openWindows.Contains(window))
            {
                _openWindows.Add(window);
            }
            
            _eventService?.Publish(new WindowOpenedEvent 
            { 
                WindowId = windowId, 
                ZOrder = window.ZOrder 
            });
            
            LogInfo($"Opened window: {windowId}");
        }
        
        public void OpenWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            OpenWindow(windowId);
        }
        
        public void CloseWindow(string windowId)
        {
            if (_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                window.Close();
                _openWindows.Remove(window);
                
                _eventService?.Publish(new WindowClosedEvent { WindowId = windowId });
                
                LogInfo($"Closed window: {windowId}");
            }
        }
        
        public void CloseWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            CloseWindow(windowId);
        }
        
        public void CloseAllWindows()
        {
            var windowsToClose = _openWindows.ToList();
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
            _openWindows.Clear();
            
            LogInfo("Closed all windows");
        }
        
        public void BringToFront(IWindow window)
        {
            if (window == null || !window.IsOpen) return;
            
            window.ZOrder = GetNextZOrder();
            window.Focus();
            
            foreach (var openWindow in _openWindows)
            {
                if (openWindow != window && openWindow.IsFocused)
                {
                    openWindow.Blur();
                }
            }
            
            _openWindows.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
            
            _eventService?.Publish(new WindowFocusedEvent { WindowId = window.WindowId });
        }
        
        public void SendToBack(IWindow window)
        {
            if (window == null || !window.IsOpen) return;
            
            window.ZOrder = baseZOrder;
            window.Blur();
        }
        
        public bool IsWindowOpen(string windowId)
        {
            return _registeredWindows.TryGetValue(windowId, out IWindow window) && window.IsOpen;
        }
        
        public bool IsWindowOpen<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            return IsWindowOpen(windowId);
        }
        
        public T GetWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            if (_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                return window as T;
            }
            return null;
        }
        
        public IWindow GetWindow(string windowId)
        {
            _registeredWindows.TryGetValue(windowId, out IWindow window);
            return window;
        }
        
        public List<IWindow> GetOpenWindows()
        {
            return new List<IWindow>(_openWindows);
        }
        
        private int GetNextZOrder()
        {
            _currentMaxZOrder += zOrderIncrement;
            return _currentMaxZOrder;
        }
    }
}