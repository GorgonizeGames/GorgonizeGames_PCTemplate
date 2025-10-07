using System;
using System.Collections;
using UnityEngine;

namespace Game.Runtime.Utilities
{
    /// <summary>
    /// Static coroutine runner - no MonoBehaviour needed
    /// Usage: Coroutines.Start(MyCoroutine());
    /// </summary>
    public static class Coroutines
    {
        private class CoroutineRunner : MonoBehaviour { }
        private static CoroutineRunner _runner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_runner == null)
            {
                GameObject obj = new GameObject("[CoroutineRunner]");
                _runner = obj.AddComponent<CoroutineRunner>();
                UnityEngine.Object.DontDestroyOnLoad(obj);
            }
        }

        public static Coroutine Start(IEnumerator routine)
        {
            Initialize();
            return _runner.StartCoroutine(routine);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null && _runner != null)
                _runner.StopCoroutine(coroutine);
        }

        public static void StopAll()
        {
            if (_runner != null)
                _runner.StopAllCoroutines();
        }

        /// <summary>
        /// Execute action after delay
        /// </summary>
        public static Coroutine DoAfter(float seconds, Action action)
        {
            return Start(DoAfterRoutine(seconds, action));
        }

        private static IEnumerator DoAfterRoutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        /// <summary>
        /// Execute action every frame until condition is true
        /// </summary>
        public static Coroutine DoUntil(Func<bool> condition, Action action)
        {
            return Start(DoUntilRoutine(condition, action));
        }

        private static IEnumerator DoUntilRoutine(Func<bool> condition, Action action)
        {
            while (!condition())
            {
                action?.Invoke();
                yield return null;
            }
        }

        /// <summary>
        /// Lerp value over time with easing
        /// </summary>
        public static Coroutine Lerp(float duration, Action<float> onUpdate, Action onComplete = null, AnimationCurve curve = null)
        {
            return Start(LerpRoutine(duration, onUpdate, onComplete, curve));
        }

        private static IEnumerator LerpRoutine(float duration, Action<float> onUpdate, Action onComplete, AnimationCurve curve)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                if (curve != null)
                    t = curve.Evaluate(t);
                
                onUpdate?.Invoke(t);
                yield return null;
            }
            onUpdate?.Invoke(1f);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Execute action every frame for duration
        /// </summary>
        public static Coroutine DoFor(float duration, Action<float> action, Action onComplete = null)
        {
            return Start(DoForRoutine(duration, action, onComplete));
        }

        private static IEnumerator DoForRoutine(float duration, Action<float> action, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                action?.Invoke(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            onComplete?.Invoke();
        }
    }
}