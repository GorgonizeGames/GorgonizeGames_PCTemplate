using System.Collections.Generic;

namespace Game.Runtime.UI.Windows
{
    public interface IWindowManager
    {
        void RegisterWindow(IWindow window);
        void UnregisterWindow(IWindow window);
        void OpenWindow(string windowId);
        void OpenWindow<T>() where T : WindowBase;
        void CloseWindow(string windowId);
        void CloseWindow<T>() where T : WindowBase;
        void CloseAllWindows();
        void BringToFront(IWindow window);
        void SendToBack(IWindow window);
        bool IsWindowOpen(string windowId);
        bool IsWindowOpen<T>() where T : WindowBase;
        T GetWindow<T>() where T : WindowBase;
        IWindow GetWindow(string windowId);
        List<IWindow> GetOpenWindows();
    }
}