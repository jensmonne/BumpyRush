using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public SettingsData CurrentSettings { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CurrentSettings = SaveSystem.LoadSettings();
    }

    private void Start()
    {
        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        UpdateFrameRate(CurrentSettings.FPS);
        UpdateVSync(CurrentSettings.VSync);
    }

    public void UpdateFrameRate(int fps)
    {
        CurrentSettings.FPS = fps;
        Application.targetFrameRate = fps;
        SaveSystem.SaveSettings(CurrentSettings);
    }

    public void UpdateVSync(bool isEnabled)
    {
        CurrentSettings.VSync = isEnabled;
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        SaveSystem.SaveSettings(CurrentSettings);
    }
}