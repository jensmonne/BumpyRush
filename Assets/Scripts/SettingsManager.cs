using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadAndApplyAllSettings();
    }

    public void LoadAndApplyAllSettings()
    {
        SetFrameRate(PlayerPrefs.GetInt("FPS", 60));
        SetVSync(PlayerPrefs.GetInt("VSync", 1) == 1);
    }

    public void SetFrameRate(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("FPS", fps);
    }

    public void SetVSync(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isEnabled ? 1 : 0);
    }
}