using UnityEngine;

namespace Game.Runtime.UI.Windows
{
    public interface IWindow
    {
        string WindowId { get; }
        string WindowTitle { get; }
        bool IsOpen { get; }
        bool IsMinimized { get; }
        bool IsMaximized { get; }
        bool IsFocused { get; }
        int ZOrder { get; set; }
        RectTransform RectTransform { get; }
        
        void Open();
        void Close();
        void Minimize();
        void Maximize();
        void Restore();
        void Focus();
        void Blur();
    }
}