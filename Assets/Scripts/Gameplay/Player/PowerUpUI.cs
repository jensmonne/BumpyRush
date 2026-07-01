using System;
using TMPro;
using UnityEngine;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private GameObject powerUpText;
    private TextMeshProUGUI text;

    public static event Action<string> OnPowerupChanged;
    private static string current;

    private void Awake()
    {
        text = powerUpText.GetComponentInChildren<TextMeshProUGUI>(true);
        text.text = string.Empty;
    }

    private void OnEnable()
    {
        OnPowerupChanged += HandlePowerupChanged;
        HandlePowerupChanged(current);
    }

    private void OnDisable() => OnPowerupChanged -= HandlePowerupChanged;

    private void HandlePowerupChanged(string powerupName)
    {
        text.text = powerupName;
    }

    public static void SetPowerup(string powerupName)
    {
        current = powerupName;
        OnPowerupChanged?.Invoke(powerupName);
    }

    public static void ClearPowerup()
    {
        current = null;
        OnPowerupChanged?.Invoke(null);
    }
}