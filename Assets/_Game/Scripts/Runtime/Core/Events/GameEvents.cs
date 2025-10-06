namespace Game.Runtime.Core.Events
{
    using Services;
    
    // ==================== CORE GAME EVENTS ====================
    public struct GameStartedEvent : IGameEvent { }
    public struct GamePausedEvent : IGameEvent { }
    public struct GameResumedEvent : IGameEvent { }
    public struct GameQuitEvent : IGameEvent { }
    
    // ==================== SERVICE EVENTS ====================
    public struct ServiceInitializedEvent : IGameEvent
    {
        public string ServiceName;
        public float InitializationTime;
    }
    
    public struct ServiceFailedEvent : IGameEvent
    {
        public string ServiceName;
        public string ErrorMessage;
    }
    
    // ==================== SCENE EVENTS ====================
    public struct SceneLoadStartedEvent : IGameEvent
    {
        public string SceneName;
    }
    
    public struct SceneLoadedEvent : IGameEvent
    {
        public string SceneName;
    }
    
    // ==================== UI EVENTS ====================
    public struct WindowOpenedEvent : IGameEvent
    {
        public string WindowId;
        public int ZOrder;
    }
    
    public struct WindowClosedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowFocusedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowMinimizedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowMaximizedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowRestoredEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowDragStartedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowDragEndedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    // ==================== AUDIO EVENTS ====================
    public struct MusicStartedEvent : IGameEvent
    {
        public string MusicId;
    }
    
    public struct MusicStoppedEvent : IGameEvent
    {
        public string MusicId;
    }
    
    public struct SoundPlayedEvent : IGameEvent
    {
        public string SoundId;
    }
    
    // ==================== SAVE EVENTS ====================
    // FIX: Success property eklendi
    public struct GameSavedEvent : IGameEvent
    {
        public string SaveSlot;
        public bool Success;
    }
    
    // FIX: Success property eklendi
    public struct GameLoadedEvent : IGameEvent
    {
        public string SaveSlot;
        public bool Success;
    }
    
    public struct CloudSyncStartedEvent : IGameEvent { }
    
    public struct CloudSyncCompletedEvent : IGameEvent
    {
        public bool Success;
    }
}