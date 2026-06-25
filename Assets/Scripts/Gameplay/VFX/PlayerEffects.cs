using UnityEngine;
using Mirror;

public class PlayerEffects : NetworkBehaviour
{
    //type van effecten
    public enum EffectType
    {
        Jump,
        Drift
    }

    [Header("Prefabs")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem driftParticles;

    //roep deze functie aan!
    public void PlayEffect(EffectType type, Transform effectPoint)
    {
        if (!isLocalPlayer) return;

        if (effectPoint != null)
        {
            CmdPlayEffect(type, effectPoint.position);
        }
    }

    [Command]
    private void CmdPlayEffect(EffectType type, Vector3 position)
    {
        RpcPlayEffect(type, position);
    }

    [ClientRpc]
    private void RpcPlayEffect(EffectType type, Vector3 position)
    {
        ParticleSystem prefabToSpawn = null;

        switch (type)
        {
            case EffectType.Jump:
                prefabToSpawn = jumpParticles;
                break;
            case EffectType.Drift:
                prefabToSpawn = driftParticles;
                break;
        }

        if (prefabToSpawn == null) return;

        ParticleSystem effectInstance = Instantiate(
            prefabToSpawn,
            position,
            Quaternion.identity
        );
        
        effectInstance.Play();
        Destroy(effectInstance.gameObject, effectInstance.main.duration);
    }
}