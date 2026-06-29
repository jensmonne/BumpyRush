using UnityEngine;

public class BumpySpawner : MonoBehaviour
{
    [SerializeField] private GameObject bumpyPrefab;

    private GameObject spawnedCharacter;
    private Renderer[] characterRenderers;

    private void Start()
    {
        SpawnCharacterPrefab();

        if (SkinCustomization.Instance != null)
        {
            SkinCustomization.Instance.OnSkinChanged += ApplySkinToModels;
        }
    }

    private void OnDestroy()
    {
        if (SkinCustomization.Instance != null)
        {
            SkinCustomization.Instance.OnSkinChanged -= ApplySkinToModels;
        }
    }

    private void SpawnCharacterPrefab()
    {
        if (bumpyPrefab == null) return;

        spawnedCharacter = Instantiate(bumpyPrefab, transform.position, transform.rotation, transform);
        
        characterRenderers = spawnedCharacter.GetComponentsInChildren<Renderer>();

        if (SkinCustomization.Instance != null)
        {
            ApplySkinToModels(SkinCustomization.Instance.CurrentSkinIndex);
        }
    }

    private void ApplySkinToModels(int skinIndex)
    {
        if (characterRenderers == null || SkinCustomization.Instance == null) return;

        Material materialToApply = SkinCustomization.Instance.GetSkinMaterial(skinIndex);
        if (materialToApply == null) return;

        foreach (var renderer in characterRenderers)
        {
            if (renderer != null)
            {
                renderer.material = materialToApply;
            }
        }
    }
}