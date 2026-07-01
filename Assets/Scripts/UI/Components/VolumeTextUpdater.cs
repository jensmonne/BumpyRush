using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VolumeTextUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (SettingsManager.Instance != null && SettingsManager.Instance.CurrentSettings != null)
        {
            float savedVolume = SettingsManager.Instance.CurrentSettings.MainVolume;
            
            if (volumeSlider != null) volumeSlider.value = savedVolume;
            UpdateText(savedVolume);
        }
    }

    public void UpdateText(float volumeValue)
    {
        int percentage = Mathf.RoundToInt(volumeValue * 100f);
        volumeText.text = percentage + "%";
    }
}