using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color urgentColor = Color.red;
    [SerializeField] private int urgentThreshold = 60;

    private void OnEnable() => GameManager.OnTimerChanged += HandleTimerChanged;
    private void OnDisable() => GameManager.OnTimerChanged -= HandleTimerChanged;

    private void Start()
    {
        if (GameManager.Instance != null)
            HandleTimerChanged(GameManager.Instance.TimeRemainingSeconds);
    }

    private void HandleTimerChanged(int seconds)
    {
        int mins = seconds / 60;
        int secs = seconds % 60;
        timerText.text = $"{mins}:{secs:00}";
        timerText.color = seconds <= urgentThreshold ? urgentColor : normalColor;
    }
}