using UnityEngine;
using Mirror;

public class PlayerEffects : NetworkBehaviour
{
    [SerializeField] private ParticleSystem jumpParticles;

    public void PlayJumpEffect(Transform effectPoint)
    {
        if (!isLocalPlayer) return;

        if (jumpParticles != null && effectPoint != null)
        {
            // Stuur de positie door naar de server
            CmdPlayJumpEffect(effectPoint.position);
        }
    }

    [Command]
    private void CmdPlayJumpEffect(Vector3 position)
    {
        RpcPlayJumpEffect(position);
    }

    [ClientRpc]
    private void RpcPlayJumpEffect(Vector3 position)
    {
        if (jumpParticles == null) return;

        ParticleSystem jumpEffectInstance = Instantiate(
            jumpParticles,
            position,
            Quaternion.identity
        );
        
        jumpEffectInstance.Play();
        Destroy(jumpEffectInstance.gameObject, jumpEffectInstance.main.duration);
    }
}
