using System;
using UnityEngine;

namespace Game.Runtime.Utilities
{
    /// <summary>
    /// Simple countdown timer (no MonoBehaviour needed)
    /// Usage: var timer = new Timer(5f).OnComplete(() => Debug.Log("Done!"));
    ///        timer.Update(Time.deltaTime);
    /// </summary>
    public class Timer
    {
        public float Duration { get; private set; }
        public float Elapsed { get; private set; }
        public float Remaining => Mathf.Max(0, Duration - Elapsed);
        public float Progress => Mathf.Clamp01(Elapsed / Duration);
        public bool IsRunning { get; private set; }
        public bool IsFinished => Elapsed >= Duration;

        private Action _onComplete;
        private Action<float> _onTick;
        private bool _loop;
        private bool _autoStart;

        public Timer(float duration, bool autoStart = true, bool loop = false)
        {
            Duration = duration;
            _loop = loop;
            _autoStart = autoStart;
            IsRunning = autoStart;
        }

        /// <summary>
        /// Update timer (call in MonoBehaviour Update)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsRunning) return;

            Elapsed += deltaTime;
            _onTick?.Invoke(Progress);

            if (Elapsed >= Duration)
            {
                _onComplete?.Invoke();

                if (_loop)
                {
                    Elapsed = 0f;
                }
                else
                {
                    IsRunning = false;
                }
            }
        }

        /// <summary>
        /// Set completion callback (chainable)
        /// </summary>
        public Timer OnComplete(Action callback)
        {
            _onComplete = callback;
            return this;
        }

        /// <summary>
        /// Set tick callback (chainable)
        /// </summary>
        public Timer OnTick(Action<float> callback)
        {
            _onTick = callback;
            return this;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            Elapsed = 0f;
        }

        public void Reset()
        {
            Elapsed = 0f;
            IsRunning = _autoStart;
        }

        public void Restart()
        {
            Elapsed = 0f;
            IsRunning = true;
        }

        public void SetDuration(float duration)
        {
            Duration = duration;
        }
    }
}