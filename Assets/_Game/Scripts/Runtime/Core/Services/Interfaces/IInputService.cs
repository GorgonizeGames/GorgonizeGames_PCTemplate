using System;
using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public interface IInputService
    {
        Vector2 MousePosition { get; }
        bool GetMouseButtonDown(int button);
        bool GetMouseButton(int button);
        bool GetMouseButtonUp(int button);
        
        bool GetKeyDown(KeyCode key);
        bool GetKey(KeyCode key);
        bool GetKeyUp(KeyCode key);
        
        bool IsCtrlPressed { get; }
        bool IsShiftPressed { get; }
        bool IsAltPressed { get; }
        
        event Action<KeyCode> OnKeyPressed;
        event Action<int> OnMouseButtonPressed;
        event Action OnEscapePressed;
        
        void EnableInput();
        void DisableInput();
        bool IsInputEnabled { get; }
    }
}