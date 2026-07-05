using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private List<PowerupIcon> icons = new();

    public static event Action<string> OnPowerupChanged;
    private static string current;
    private Dictionary<string, Sprite> iconLookup = new();

    [Serializable]
    public struct PowerupIcon
    {
        public string name;
        public Sprite sprite;
    }

    private void Awake()
    {
        foreach (var icon in icons)
            iconLookup[icon.name] = icon.sprite;
        iconImage.enabled = false;
    }

    private void OnEnable()
    {
        OnPowerupChanged += HandlePowerupChanged;
        HandlePowerupChanged(current);
    }

    private void OnDisable() => OnPowerupChanged -= HandlePowerupChanged;

    private void HandlePowerupChanged(string powerupName)
    {
        if (!string.IsNullOrEmpty(powerupName) && iconLookup.TryGetValue(powerupName, out Sprite sprite))
        {
            iconImage.sprite = sprite;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
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