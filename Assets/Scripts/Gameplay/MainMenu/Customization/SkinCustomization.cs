using UnityEngine;

public class SkinCustomization : MonoBehaviour
{
    [SerializeField] private Material[] skinMaterials; // Array of available skin materials
    [SerializeField] private GameObject bumpyPrefab;  // The prefab to spawn
    [SerializeField] private Transform spawnPoint;     // Where to spawn the prefab (will become the parent)

    public static SkinCustomization Instance { get; private set; }

    private int currentSkinIndex = 0; // Index of the currently selected skin
    private GameObject spawnedCharacter;   // Tracks the instantiated prefab
    private Renderer[] characterRenderers; // Cache of all renderers found in the children

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
        // Spawn de prefab bij de start van de eerste scène
        SpawnCharacterPrefab();
    }

    // Methode om de prefab te spawnen en als child in te stellen
    private void SpawnCharacterPrefab()
    {
        if (bumpyPrefab != null && spawnPoint != null)
        {
            // Fix: Door 'spawnPoint' als derde argument mee te geven, wordt het direct de parent in de hiërarchie
            spawnedCharacter = Instantiate(bumpyPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            
            // Haal automatisch ALLE renderers uit de children (de meshes)
            characterRenderers = spawnedCharacter.GetComponentsInChildren<Renderer>();

            // Pas direct de huidige geselecteerde skin toe
            ApplySkinToModels();
        }
        else
        {
            Debug.LogWarning("Bumpy prefab or spawn point is not assigned.");
        }
    }

    // Voor de Lobby: Als je van scène wisselt, geef je het nieuwe spawnpoint in de lobby door
    public void SetupInNewScene(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        SpawnCharacterPrefab();
    }

    public void ChangeSkin(int skinIndex)
    {
        if (skinMaterials.Length == 0) return;

        // Zorg dat de index altijd binnen de array-grenzen blijft (modulo)
        currentSkinIndex = (skinIndex + skinMaterials.Length) % skinMaterials.Length;

        PlayerPrefs.SetInt("PlayerSkin", currentSkinIndex);

        ApplySkinToModels();
    }

    public void ChangeSkinScrollLeft()
    {
        ChangeSkin(currentSkinIndex - 1);
    }

    public void ChangeSkinScrollRight()
    {
        ChangeSkin(currentSkinIndex + 1);
    }

    // Hulpmethode die door alle renderers in de children loopt
    private void ApplySkinToModels()
    {
        if (characterRenderers == null || skinMaterials.Length == 0) return;

        foreach (var renderer in characterRenderers)
        {
            if (renderer != null)
            {
                renderer.material = skinMaterials[currentSkinIndex];
            }
        }
    }

    public Material GetSkinMaterial(int index)
    {
        if (skinMaterials == null || skinMaterials.Length == 0) return null;
        
        int safeIndex = (index + skinMaterials.Length) % skinMaterials.Length;
        return skinMaterials[safeIndex];
    }
}