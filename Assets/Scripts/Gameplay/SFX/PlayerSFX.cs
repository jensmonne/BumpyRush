using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Dit script beheert alle geluidseffecten specifiek voor de speler/het object
/// en beheert eigen lokale AudioSources voor aanhoudende 3D effecten.
/// </summary>
public class PlayerSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip driftSound;
    [SerializeField] private AudioClip driveSound;

    [Header("Drift Loop Settings")]
    [SerializeField] private float driftLoopStartTime = 0.2f;
    [SerializeField] private float driftLoopEndTime = 0.8f;
    [SerializeField] private float driftFadeOutDuration = 0.05f;

    [Header("Drive Loop Settings")]
    [SerializeField] private float driveLoopStartTime = 0.1f;
    [SerializeField] private float driveLoopEndTime = 1.5f;
    [Tooltip("Het minimale volume als de auto stilstaat (Idle).")]
    [Range(0f, 1f)] [SerializeField] private float minEngineVolume = 0.15f;
    [Tooltip("Het maximale volume op topsnelheid.")]
    [Range(0f, 1f)] [SerializeField] private float maxEngineVolume = 0.85f;
    [Tooltip("De pitch (toonhoogte) van de motor bij stilstand.")]
    [SerializeField] private float minEnginePitch = 0.75f;
    [Tooltip("De pitch van de motor op topsnelheid.")]
    [SerializeField] private float maxEnginePitch = 1.5f;

    private AudioSource driftAudioSource;
    private AudioSource driveAudioSource; // Tweede AudioSource specifiek voor de motor

    private bool isDrifting = false;
    private float originalDriftVolume;

    private void Awake()
    {
        // Maak dynamisch twee AudioSources aan zodat we niet handmatig componenten hoeven te stapelen in de inspector
        AudioSource[] sources = GetComponents<AudioSource>();
        
        // Als er nog geen twee AudioSources zijn, maken we ze aan
        driftAudioSource = gameObject.AddComponent<AudioSource>();
        driveAudioSource = gameObject.AddComponent<AudioSource>();

        // Configureer Drift AudioSource
        driftAudioSource.playOnAwake = false;
        driftAudioSource.spatialBlend = 1.0f; // 3D Sound
        originalDriftVolume = driftAudioSource.volume > 0 ? driftAudioSource.volume : 1f;

        // Configureer Drive AudioSource
        driveAudioSource.playOnAwake = false;
        driveAudioSource.spatialBlend = 1.0f; // 3D Sound
        driveAudioSource.loop = false; // Handmatige loop in Update
    }

    private void Start()
    {
        // De motor start direct zodra de auto in de wereld spawnt
        if (driveSound != null)
        {
            driveAudioSource.clip = driveSound;
            driveAudioSource.Play();
        }
    }

    private void Update()
    {
        // 1. Handmatige loop voor het driftgeluid
        if (isDrifting && driftAudioSource.isPlaying)
        {
            if (driftAudioSource.time >= driftLoopEndTime)
            {
                driftAudioSource.time = driftLoopStartTime;
            }
        }

        // 2. Handmatige loop voor het motorgeluid
        if (driveAudioSource.isPlaying)
        {
            if (driveAudioSource.time >= driveLoopEndTime)
            {
                driveAudioSource.time = driveLoopStartTime;
            }
        }
    }

    /// <summary>
    /// Past de volume en pitch van de motor aan op basis van de huidige snelheid.
    /// Wordt aangeroepen vanaf de Movement.cs (hoeft niet via Mirror sync, omdat elke client de auto ziet bewegen!)
    /// </summary>
    public void UpdateEngineSound(float currentSpeedFactor)
    {
        if (driveAudioSource == null) return;

        // Vloeiend volume en toonhoogte berekenen t.o.v. de snelheid (0.0 tot 1.0)
        driveAudioSource.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, currentSpeedFactor);
        driveAudioSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, currentSpeedFactor);
    }

    #region Jump SFX
    public void PlayJumpSound()
    {
        if (isLocalPlayer) { CmdPlayJumpSound(); }
    }

    [Command] private void CmdPlayJumpSound() { RpcPlayJumpSound(); }
    [ClientRpc]
    private void RpcPlayJumpSound()
    {
        if (jumpSound != null) SoundManager.Instance.Play3DSFX(jumpSound, transform.position);
    }
    #endregion

    #region Drift SFX
    public void StartDrift()
    {
        if (isLocalPlayer && !isDrifting) { CmdStartDrift(); }
    }

    public void StopDrift()
    {
        if (isLocalPlayer && isDrifting) { CmdStopDrift(); }
    }

    [Command] private void CmdStartDrift() { RpcStartDrift(); }
    [ClientRpc]
    private void RpcStartDrift()
    {
        if (driftSound != null && driftAudioSource != null)
        {
            StopAllCoroutines();
            isDrifting = true;
            driftAudioSource.volume = originalDriftVolume;
            driftAudioSource.clip = driftSound;
            driftAudioSource.time = 0f;
            driftAudioSource.loop = false;
            driftAudioSource.Play();
        }
    }

    [Command] private void CmdStopDrift() { RpcStopDrift(); }
    [ClientRpc]
    private void RpcStopDrift()
    {
        isDrifting = false;
        if (driftAudioSource != null && driftAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutDrift(driftFadeOutDuration));
        }
    }

    private IEnumerator FadeOutDrift(float duration)
    {
        float startVolume = driftAudioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            driftAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        driftAudioSource.Stop();
        driftAudioSource.volume = originalDriftVolume;
    }
    #endregion
}