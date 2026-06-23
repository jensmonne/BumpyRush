using UnityEngine;

public class SkinCustomization : MonoBehaviour
{
    [SerializeField] private Material[] skinMaterials; // Array of available skin materials
    [SerializeField] private GameObject bumpyPrefab;  // The prefab to spawn
    [SerializeField] private Transform spawnPoint;     // Where to spawn the prefab

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

    // Methode om de prefab te spawnen en zijn renderers te registreren
    private void SpawnCharacterPrefab()
    {
        if (bumpyPrefab != null && spawnPoint != null)
        {
            // Instantiëren en opslaan in de variabele
            spawnedCharacter = Instantiate(bumpyPrefab, spawnPoint.position, spawnPoint.rotation);
            
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

    // Fix voor de Lobby: Als je van scène wisselt, kun je hier een nieuw spawnpoint doorgeven 
    // en het personage opnieuw spawnen in de lobby.
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

    // Vernieuwde hulpmethode die door alle renderers in de children loopt
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
}