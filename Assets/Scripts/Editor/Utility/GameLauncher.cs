//------------------------------------------------------------
// File : GameLauncher.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class GameLauncher
{
    static string PREVIOUS_SCENE_KEY = "UnityEditorInitializerPreviousSceneKey";
    static HashSet<string> excludedScenes = new HashSet<string>();

    static GameLauncher()
    {
        LoadSettings();
        EditorApplication.playModeStateChanged += onPlaymodeStateChanged;
    }

    private static void LoadSettings()
    {
        string path = "Assets/Game/Fonts/TMP Settings.asset";
        TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(path);
        var settingsType = settings.GetType();
        var settingsInstanceInfo = settingsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
        settingsInstanceInfo.SetValue(null, settings);
    }

    private static void onPlaymodeStateChanged(PlayModeStateChange mode)
    {
        if (mode == PlayModeStateChange.ExitingEditMode)
        {
            //play pressed to start playing
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                PlayerPrefs.SetString(PREVIOUS_SCENE_KEY, EditorSceneManager.GetActiveScene().path);
                PlayerPrefs.Save();

                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                if (!excludedScenes.Contains(EditorSceneManager.GetActiveScene().path))
                {
                    EditorSceneManager.OpenScene("Assets/Resources/Scenes/KBScene.unity");
                }
            }
        }
        else if (mode == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene(PlayerPrefs.GetString(PREVIOUS_SCENE_KEY));
        }

    }
}
#endif