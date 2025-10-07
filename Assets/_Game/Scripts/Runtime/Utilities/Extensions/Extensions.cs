using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Utilities
{
    public static class Extensions
    {
        // ==================== TRANSFORM ====================
        
        /// <summary>
        /// Reset local transform to default
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Reset world transform to default
        /// </summary>
        public static void ResetWorld(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Destroy all children
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroy all children immediately (Editor safe)
        /// </summary>
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
#else
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
#endif
            }
        }

        /// <summary>
        /// Find child by name recursively
        /// </summary>
        public static Transform FindDeep(this Transform transform, string name)
        {
            Transform result = transform.Find(name);
            if (result != null) return result;

            foreach (Transform child in transform)
            {
                result = child.FindDeep(name);
                if (result != null) return result;
            }

            return null;
        }

        // ==================== GAMEOBJECT ====================
        
        /// <summary>
        /// Get or add component (safe)
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// Try get component in children (includes self)
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T component) where T : Component
        {
            component = obj.GetComponentInChildren<T>();
            return component != null;
        }

        /// <summary>
        /// Set layer recursively
        /// </summary>
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Set active with null check
        /// </summary>
        public static void SetActiveSafe(this GameObject obj, bool active)
        {
            if (obj != null)
                obj.SetActive(active);
        }

        // ==================== VECTOR2 ====================
        
        /// <summary>
        /// With X component changed
        /// </summary>
        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }

        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }

        /// <summary>
        /// Convert to Vector3 (XY plane)
        /// </summary>
        public static Vector3 ToVector3XY(this Vector2 v, float z = 0f)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Convert to Vector3 (XZ plane)
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector2 v, float y = 0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        // ==================== VECTOR3 ====================
        
        /// <summary>
        /// Get Vector2 from Vector3 (XY plane)
        /// </summary>
        public static Vector2 ToVector2XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Get Vector2 from Vector3 (XZ plane)
        /// </summary>
        public static Vector2 ToVector2XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        /// <summary>
        /// With X component changed
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Flat (Y=0)
        /// </summary>
        public static Vector3 Flat(this Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }

        // ==================== COLOR ====================
        
        /// <summary>
        /// With alpha changed
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Convert to hex string
        /// </summary>
        public static string ToHex(this Color color, bool includeAlpha = true)
        {
            if (includeAlpha)
                return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
            else
                return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        /// <summary>
        /// Lighten color
        /// </summary>
        public static Color Lighten(this Color color, float amount)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        /// <summary>
        /// Darken color
        /// </summary>
        public static Color Darken(this Color color, float amount)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        // ==================== COLLECTIONS ====================
        
        /// <summary>
        /// Get random element from list
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Get random element from array
        /// </summary>
        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return default(T);
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Shuffle list (Fisher-Yates)
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null) return;
            
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Check if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// Add range with null check
        /// </summary>
        public static void AddRangeSafe<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null) return;

            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Remove all that match predicate
        /// </summary>
        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            if (list == null || match == null) return 0;

            int count = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    count++;
                }
            }
            return count;
        }

        // ==================== STRING ====================
        
        /// <summary>
        /// Truncate string with ellipsis
        /// </summary>
        public static string Truncate(this string str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;
            return str.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Check if string is null or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Title case
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Contains ignore case
        /// </summary>
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            return source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ==================== FLOAT/INT ====================
        
        /// <summary>
        /// Remap value from one range to another
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// Clamp between 0 and 1
        /// </summary>
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Check if approximately equal
        /// </summary>
        public static bool Approximately(this float value, float other, float threshold = 0.0001f)
        {
            return Mathf.Abs(value - other) < threshold;
        }
    }
}