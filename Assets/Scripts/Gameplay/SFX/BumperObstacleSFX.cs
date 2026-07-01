using UnityEngine;
using Mirror;

public class BumperObstacleSFX : NetworkBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip BumpSound;

    public void PlayBumpSound()
    {
        CmdPlayBumpSound();
    }

    [Command]
    private void CmdPlayBumpSound()
    {
        RpcPlayBumpSound();
    }

    [ClientRpc]
    private void RpcPlayBumpSound()
    {
        if (BumpSound != null)
        {
            SoundManager.Instance.Play3DSFX(BumpSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Bump sound clip is not assigned in BumperObstacleSFX.");
        }
    }
}
