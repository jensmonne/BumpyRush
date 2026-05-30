using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string FileName = "settings.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void SaveSettings(SettingsData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Settings saved successfully to {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save settings: {e.Message}");
        }
    }

    public static SettingsData LoadSettings()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"[SaveSystem] No save file found at {SavePath}. Creating default settings.");
            SettingsData defaultData = new SettingsData();
            SaveSettings(defaultData);
            return defaultData;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SettingsData data = JsonUtility.FromJson<SettingsData>(json);
            Debug.Log($"[SaveSystem] Settings loaded successfully from {SavePath}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load settings: {e.Message}. Returning default settings.");
            return new SettingsData();
        }
    }
}