using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityToolbarExtender;

namespace Game.Editor
{
    [InitializeOnLoad]
    public static class PlayFromBootstrapToolbar
    {
        private const string BOOTSTRAP_SCENE_PATH = "Assets/_Game/Scenes/Bootstrap.unity";
        private const string RETURN_SCENE_KEY = "BootstrapToolbar.ReturnScene";


        static PlayFromBootstrapToolbar()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            // 1. GO TO BOOTSTRAP BUTTON (SOL)
            var gotoButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Normal,
                fixedHeight = 22,
                fixedWidth = 100
            };

            bool isInBootstrap = SceneManager.GetActiveScene().path == BOOTSTRAP_SCENE_PATH;
            GUI.enabled = !isInBootstrap && !EditorApplication.isPlaying;
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.9f);

            // Sahne ikonu: 🎬 (film klaperi) veya 🎭 (maske) veya 🎪 (çadır)
            if (GUILayout.Button("🎬 Bootstrap", gotoButtonStyle))
            {
                GoToBootstrap();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            // Butonlar arası boşluk
            GUILayout.Space(5);

            // 2. PLAY/STOP BUTTON (SAĞ)
            var playButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedHeight = 22,
                fixedWidth = 120
            };

            GUI.backgroundColor = EditorApplication.isPlaying ? Color.red : Color.green;

            string buttonText = EditorApplication.isPlaying ? "■ Stop (F5)" : "▶ Bootstrap (F5)";

            if (GUILayout.Button(buttonText, playButtonStyle))
            {
                PlayFromBootstrapScene();
            }

            GUI.backgroundColor = Color.white;
        }

        [MenuItem("Gorgonize/Play From Bootstrap _F5", priority = 100)]
        private static void PlayFromBootstrapScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string currentScene = SceneManager.GetActiveScene().path;

                if (currentScene != BOOTSTRAP_SCENE_PATH)
                {
                    EditorPrefs.SetString(RETURN_SCENE_KEY, currentScene);
                    Debug.Log($"<color=lime><b>🚀 Playing from Bootstrap</b></color> (Will return to: {System.IO.Path.GetFileNameWithoutExtension(currentScene)})");
                }
                else
                {
                    EditorPrefs.DeleteKey(RETURN_SCENE_KEY);
                    Debug.Log($"<color=lime><b>🚀 Playing from Bootstrap</b></color>");
                }

                EditorSceneManager.OpenScene(BOOTSTRAP_SCENE_PATH);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Gorgonize/Go to Bootstrap Scene _F6", priority = 101)]
        private static void GoToBootstrap()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Cannot change scene while playing!");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string currentScene = SceneManager.GetActiveScene().path;

                if (currentScene == BOOTSTRAP_SCENE_PATH)
                {
                    Debug.Log("<color=yellow>Already in Bootstrap scene</color>");
                    return;
                }

                EditorSceneManager.OpenScene(BOOTSTRAP_SCENE_PATH);
                Debug.Log($"<color=cyan>📁 Opened Bootstrap scene</color>");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (EditorPrefs.HasKey(RETURN_SCENE_KEY))
                {
                    string returnScene = EditorPrefs.GetString(RETURN_SCENE_KEY);
                    EditorPrefs.DeleteKey(RETURN_SCENE_KEY);

                    if (!string.IsNullOrEmpty(returnScene) && System.IO.File.Exists(returnScene))
                    {
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(returnScene);
                        Debug.Log($"<color=cyan><b>📂 Returning to:</b> {sceneName}</color>");

                        EditorSceneManager.OpenScene(returnScene);
                    }
                }
            }
        }
    }
}