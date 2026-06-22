using UnityEngine;
using Mirror;

/// <summary>
/// Dit script beheert alle geluidseffecten specifiek voor de speler/het object
/// en communiceert met de centrale SoundManager.
/// </summary>
public class PlayerSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip jumpSound;

    public void PlayJumpSound()
    {
        if(isLocalPlayer)
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
}