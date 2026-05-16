#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeBootstrapper
{
    private const string MenuPath = "Tools/Multiplayer/Always Start From Main Menu";
    private const string PrefKey = "UseMultiplayerBootstrapper";

    static PlayModeBootstrapper()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(MenuPath)]
    public static void ToggleBootstrapper()
    {
        bool currentState = EditorPrefs.GetBool(PrefKey, false);
        EditorPrefs.SetBool(PrefKey, !currentState);
        
        if (currentState)
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }

    [MenuItem(MenuPath, true)]
    public static bool ToggleBootstrapperValidate()
    {
        MenuUtility.CheckMenuItem(MenuPath, EditorPrefs.GetBool(PrefKey, false));
        return true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;

        bool isEnabled = EditorPrefs.GetBool(PrefKey, false);

        if (isEnabled)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene MainMenu");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                
                EditorSceneManager.playModeStartScene = sceneAsset;
            }
            else
            {
                Debug.LogWarning("[Bootstrapper] Could not find a scene named 'MainMenu'.");
            }
        }
        else EditorSceneManager.playModeStartScene = null;
    }
}

public static class MenuUtility
{
    public static void CheckMenuItem(string menuPath, bool isChecked)
    {
        Menu.SetChecked(menuPath, isChecked);
    }
}
#endif