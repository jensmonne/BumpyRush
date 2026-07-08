using Mirror;
using UnityEngine;

public class PlushieSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip PickupSound;

    public void PlayPickupSound()
    {
        Debug.LogWarning($"WAAAAA");
        RpcPlayPickupSound();
    }

    [ClientRpc]
    private void RpcPlayPickupSound()
    {
        if (PickupSound != null)
        {
            Debug.LogWarning($"Playing pickup sound for {gameObject.name} at position {transform.position}");
            SoundManager.Instance.Play3DSFX(PickupSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Pickup sound clip is not assigned in PlushieSFX.");
        }
    }
}
