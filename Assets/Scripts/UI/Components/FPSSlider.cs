using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSSlider : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private TextMeshProUGUI fpsText;

    [SerializeField] private int[] commonFPSValues = { 60, 90, 120, 144, 240 };

    private int currentFPS;
    private bool isInitializing = true;

    private void Start()
    {
        fpsSlider.minValue = commonFPSValues[0];
        fpsSlider.maxValue = commonFPSValues[commonFPSValues.Length - 1];
        fpsSlider.wholeNumbers = true;

        int initialFPS = SettingsManager.Instance.CurrentSettings.FPS;
        
        int snappedInitial = GetClosestFPS(initialFPS);
        fpsSlider.value = snappedInitial;
        UpdateFPSDisplay(snappedInitial);
        currentFPS = snappedInitial;

        isInitializing = false;
    }

    public void OnSliderValueChanged()
    {
        if (isInitializing) return;

        int closestFPS = GetClosestFPS((int)fpsSlider.value);

        fpsSlider.value = closestFPS;

        if (closestFPS != currentFPS)
        {
            currentFPS = closestFPS;
            UpdateFPSDisplay(closestFPS);
            SettingsManager.Instance.UpdateFrameRate(closestFPS);
        }
    }

    private int GetClosestFPS(int target)
    {
        int closest = commonFPSValues[0];
        int minDifference = Mathf.Abs(target - closest);

        foreach (int fps in commonFPSValues)
        {
            int difference = Mathf.Abs(target - fps);
            if (difference < minDifference)
            {
                minDifference = difference;
                closest = fps;
            }
        }
        return closest;
    }

    private void UpdateFPSDisplay(int value)
    {
        if (fpsText != null)
        {
            fpsText.text = value.ToString();
        }
    }
}