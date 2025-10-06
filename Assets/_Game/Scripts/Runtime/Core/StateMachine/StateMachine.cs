using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core.StateMachine
{
    /// <summary>
    /// Generic State Machine - Her türlü state için kullanılabilir
    /// </summary>
    public class StateMachine<T>
    {
        private readonly T _owner;
        private IState<T> _currentState;
        private readonly Dictionary<Type, IState<T>> _states = new Dictionary<Type, IState<T>>();
        
        public IState<T> CurrentState => _currentState;
        public T Owner => _owner;
        
        public StateMachine(T owner)
        {
            _owner = owner;
        }
        
        public void AddState(IState<T> state)
        {
            if (state == null)
            {
                Debug.LogError("[StateMachine] Cannot add null state");
                return;
            }
            
            _states[state.GetType()] = state;
        }
        
        public void Update()
        {
            _currentState?.OnUpdate(_owner);
        }
        
        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate(_owner);
        }
        
        public bool ChangeState<TState>() where TState : IState<T>
        {
            if (!_states.ContainsKey(typeof(TState)))
            {
                Debug.LogWarning($"[StateMachine] State {typeof(TState).Name} not found");
                return false;
            }
            
            _currentState?.OnExit(_owner);
            
            var previousState = _currentState;
            _currentState = _states[typeof(TState)];
            _currentState.OnEnter(_owner);
            
            Debug.Log($"[StateMachine] State changed: {previousState?.GetType().Name} -> {_currentState.GetType().Name}");
            
            return true;
        }
        
        public bool IsInState<TState>() where TState : IState<T>
        {
            return _currentState != null && _currentState.GetType() == typeof(TState);
        }
        
        public TState GetState<TState>() where TState : IState<T>
        {
            if (_states.TryGetValue(typeof(TState), out IState<T> state))
            {
                return (TState)state;
            }
            return default(TState);
        }
        
        public void Cleanup()
        {
            _currentState?.OnExit(_owner);
            _currentState = null;
            _states.Clear();
        }
    }
    
    /// <summary>
    /// State interface - Tüm state'lerin implement etmesi gereken
    /// </summary>
    public interface IState<T>
    {
        void OnEnter(T owner);
        void OnUpdate(T owner);
        void OnFixedUpdate(T owner);
        void OnExit(T owner);
    }
    
    /// <summary>
    /// Base state class - İsteğe bağlı override için
    /// </summary>
    public abstract class BaseState<T> : IState<T>
    {
        public virtual void OnEnter(T owner) { }
        public virtual void OnUpdate(T owner) { }
        public virtual void OnFixedUpdate(T owner) { }
        public virtual void OnExit(T owner) { }
    }
}