using UnityEngine;
using Mirror;

public class PlushieSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip PickupSound;

    public void PlayPickupSound()
    {
        CmdPlayPickupSound();
    }

    [Command]
    private void CmdPlayPickupSound()
    {
        RpcPlayPickupSound();
    }

    [ClientRpc]
    private void RpcPlayPickupSound()
    {
        if (PickupSound != null)
        {
            SoundManager.Instance.Play3DSFX(PickupSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Pickup sound clip is not assigned in PlushieSFX.");
        }
    }
}
