using System;
using TMPro;
using UnityEngine;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private GameObject powerUpText;

    private TextMeshProUGUI tmp;

    public static event Action<string> OnPowerupChanged;

    private void Awake()
    {
        tmp = powerUpText.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable() => OnPowerupChanged += HandlePowerupChanged;
    private void OnDisable() => OnPowerupChanged -= HandlePowerupChanged;

    private void Start() => powerUpText.SetActive(false);

    private void HandlePowerupChanged(string powerupName)
    {
        Debug.Log($"PowerUpUI: Powerup changed to {powerupName}");
        bool hasPowerup = !string.IsNullOrEmpty(powerupName);

        powerUpText.SetActive(hasPowerup);
        if (hasPowerup && tmp != null)
            tmp.text = powerupName;
    }

    public static void SetPowerup(string powerupName) => OnPowerupChanged?.Invoke(powerupName);
    public static void ClearPowerup() => OnPowerupChanged?.Invoke(null);
}