using UnityEngine;
using UnityEngine.SceneManagement; // Nodig voor SceneManager

public class DontDestroySoundtrack : MonoBehaviour
{
    private static DontDestroySoundtrack instance;

    [SerializeField] private AudioClip soundtrackMainMenu;
    [SerializeField] private AudioClip soundtrackGameplay;

    private AudioSource currentSoundtrack;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        currentSoundtrack = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Registreer ons script bij de sceneLoaded event van Unity
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Altijd netjes afmelden als het object wordt uitgeschakeld/vernietigd
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Dit vervangt de oude OnLevelWasLoaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int level = scene.buildIndex;

        if (level == 0 || level == 1) // Scène 0 en 1 (Main Menu / Intro)
        {
            // Speel de muziek alleen af als deze NIET al aan het spelen was
            if (currentSoundtrack.clip != soundtrackMainMenu)
            {
                currentSoundtrack.clip = soundtrackMainMenu;
                currentSoundtrack.loop = true;
                currentSoundtrack.Play();
            }
        }
        else if (level == 2) // Scène 2 (Gameplay / Level 3)
        {
            // Kap de oude muziek direct af en start de gameplay muziek
            if (currentSoundtrack.clip != soundtrackGameplay)
            {
                currentSoundtrack.clip = soundtrackGameplay;
                currentSoundtrack.loop = true;
                currentSoundtrack.Play();
            }
        }
    }
}