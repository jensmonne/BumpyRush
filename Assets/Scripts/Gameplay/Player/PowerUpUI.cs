using System;
using TMPro;
using UnityEngine;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private GameObject powerUpText;
    private TextMeshProUGUI tmp;

    public static event Action<string> OnPowerupChanged;
    private static string current;

    private void Awake()
    {
        tmp = powerUpText.GetComponentInChildren<TextMeshProUGUI>(true);
        OnPowerupChanged += HandlePowerupChanged;
        Debug.Log("PowerUpUI subscribed");
    }

    private void OnEnable()
    {
        OnPowerupChanged += HandlePowerupChanged;
        HandlePowerupChanged(current);
    }

    private void OnDisable() => OnPowerupChanged -= HandlePowerupChanged;

    private void HandlePowerupChanged(string powerupName)
    {
        bool hasPowerup = !string.IsNullOrEmpty(powerupName);
        powerUpText.SetActive(hasPowerup);
        if (hasPowerup && tmp != null)
            tmp.text = powerupName;
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