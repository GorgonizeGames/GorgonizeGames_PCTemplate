using System;

namespace Game.Runtime.Core.Services
{
    public interface IEventService
    {
        void Subscribe<T>(Action<T> callback) where T : struct, IGameEvent;
        void Unsubscribe<T>(Action<T> callback) where T : struct, IGameEvent;
        void Publish<T>(T eventData) where T : struct, IGameEvent;
        void Clear();
    }
    
    public interface IGameEvent { }
}