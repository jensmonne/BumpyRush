using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Dit script beheert alle geluidseffecten specifiek voor de speler/het object
/// en communiceert met de centrale SoundManager of beheert eigen lokale AudioSources.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip driftSound;

    [Header("Drift Loop Settings")]
    [Tooltip("De tijd in seconden waarop het geluid moet gaan loopen (het begin van het middenstuk).")]
    [SerializeField] private float loopStartTime = 0.2f;
    [Tooltip("De tijd in seconden waar de loop naar terugspringt als hij het einde van het middenstuk bereikt.")]
    [SerializeField] private float loopEndTime = 0.8f;
    [Tooltip("De tijd in seconden die het geluid nodig heeft om uit te faden als je stopt met driften.")]
    [SerializeField] private float fadeOutDuration = 0.05f;

    private AudioSource driftAudioSource;
    private bool isDrifting = false;
    private float originalVolume;

    private void Awake()
    {
        // We halen de AudioSource op die verplicht op dit GameObject zit
        driftAudioSource = GetComponent<AudioSource>();
        
        // Zorg ervoor dat de AudioSource correct staat ingesteld voor 3D geluid vanaf de auto
        driftAudioSource.playOnAwake = false;
        driftAudioSource.spatialBlend = 1.0f; // 1.0 betekent volledig 3D geluid
        originalVolume = driftAudioSource.volume > 0 ? driftAudioSource.volume : 1f;
    }

    private void Update()
    {
        // Iedere client controleert de timing van zijn eigen actieve drift-geluiden
        if (isDrifting && driftAudioSource.isPlaying)
        {
            // Als de audio voorbij het eindpunt van de loop is, springen we terug naar het startpunt van de loop
            if (driftAudioSource.time >= loopEndTime)
            {
                driftAudioSource.time = loopStartTime;
            }
        }
    }

    #region Jump SFX
    public void PlayJumpSound()
    {
        if (isLocalPlayer)
        {
            CmdPlayJumpSound();
        }
    }

    [Command]
    private void CmdPlayJumpSound()
    {
        RpcPlayJumpSound();
    }

    [ClientRpc]
    private void RpcPlayJumpSound()
    {
        if (jumpSound != null)
        {
            SoundManager.Instance.Play3DSFX(jumpSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Jump sound clip is not assigned in PlayerSFX.");
        }
    }
    #endregion

    #region Drift SFX

    // Roep deze functie aan vanuit je Car/Input controller zodra de speler begint te driften
    public void StartDrift()
    {
        if (isLocalPlayer && !isDrifting)
        {
            Debug.Log("StartDrift called on local player.2");
            CmdStartDrift();
        }
    }

    // Roep deze functie aan vanuit je Car/Input controller zodra de speler stopt met driften
    public void StopDrift()
    {
        if (isLocalPlayer && isDrifting)
        {
            Debug.Log("StopDrift called on local player.4");
            CmdStopDrift();
        }
    }

    [Command]
    private void CmdStartDrift()
    {
        RpcStartDrift();
    }

    [ClientRpc]
    private void RpcStartDrift()
    {
        if (driftSound != null && driftAudioSource != null)
        {
            Debug.Log("RpcStartDrift called on client.3");
            StopAllCoroutines(); // Stop eventuele actieve fade-outs van een vorige drift
            
            isDrifting = true;
            driftAudioSource.volume = originalVolume;
            driftAudioSource.clip = driftSound;
            driftAudioSource.time = 0f; // Begin netjes bij de start (de intro-screech)
            driftAudioSource.loop = false; // We handelen de loop handmatig af in de Update()
            driftAudioSource.Play();
        }
    }

    [Command]
    private void CmdStopDrift()
    {
        RpcStopDrift();
    }

    [ClientRpc]
    private void RpcStopDrift()
    {
        isDrifting = false;
        if (driftAudioSource != null && driftAudioSource.isPlaying)
        {
            // We faden het geluid snel uit zodat het niet abrupt of raar afkapt
            StartCoroutine(FadeOutDrift(fadeOutDuration));
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
        driftAudioSource.volume = originalVolume; // Reset het volume voor de volgende drift-beurt
    }
    #endregion
}