using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // muziek
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // 3D SFX
    public void Play3DSFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null)
        {
            // Dit maakt automatisch een tijdelijke 3D AudioSource aan op de 'position'
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}