using UnityEngine;
using System;

public class SkinCustomization : MonoBehaviour
{
    [SerializeField] private Material[] skinMaterials;

    public static SkinCustomization Instance { get; private set; }

    public int CurrentSkinIndex { get; private set; } = 0;

    public event Action<int> OnSkinChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentSkinIndex = PlayerPrefs.GetInt("PlayerSkin", 0);
    }

    public void ChangeSkin(int skinIndex)
    {
        if (skinMaterials.Length == 0) return;

        // Zorg dat de index altijd binnen de array-grenzen blijft (modulo)
        CurrentSkinIndex = (skinIndex + skinMaterials.Length) % skinMaterials.Length;

        PlayerPrefs.SetInt("PlayerSkin", CurrentSkinIndex);

        OnSkinChanged?.Invoke(CurrentSkinIndex);
    }

    public void ChangeSkinScrollLeft()
    {
        ChangeSkin(CurrentSkinIndex - 1);
    }

    public void ChangeSkinScrollRight()
    {
        ChangeSkin(CurrentSkinIndex + 1);
    }

    public Material GetSkinMaterial(int index)
    {
        if (skinMaterials == null || skinMaterials.Length == 0) return null;
        
        int safeIndex = (index + skinMaterials.Length) % skinMaterials.Length;
        return skinMaterials[safeIndex];
    }
}