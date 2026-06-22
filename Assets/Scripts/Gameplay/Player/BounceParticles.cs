using UnityEngine;
using System.Collections;
using Mirror;

public class BounceParticles : NetworkBehaviour
{
    [Tooltip("The particle system Prefab for bounce")]
    [SerializeField] private ParticleSystem effect;

    [Tooltip("The bounce collider on this object")]
    [SerializeField] private Collider bounceCollider;

    [Tooltip("Minimum impact force to trigger the bounce effect")]
    [SerializeField] private float minimumImpactForce = 3f;

    private void Start()
    {
        if(effect == null)
        {
            Debug.LogError("Effect Prefab is not assigned in the inspector.");
        }
        if (bounceCollider == null)
        {
            Debug.LogError("No Collider found on the GameObject. Please add a Collider component.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!isServerOnly && !isLocalPlayer) return;

        // Ignore collisions with certain tags
        if (collision.gameObject.CompareTag("PhyObject") || collision.gameObject.CompareTag("Bridge"))
        {
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider == bounceCollider)
            {
                float impactForce = collision.relativeVelocity.magnitude;

                // Ignore soft scrapes
                if (impactForce < minimumImpactForce)
                    return;

                //Debug.Log("Bounce collision detected at point: " + contact.point);
                CmdRequestBounceEffect(contact.point, contact.normal);
            }
        }
    }

    [Command]
    private void CmdRequestBounceEffect(Vector3 point, Vector3 normal)
    {
        // De server stuurt dit door naar ALLE clients (inclusief de host zelf)
        RpcPlayBounceEffect(point, normal);
    }

    [ClientRpc]
    private void RpcPlayBounceEffect(Vector3 point, Vector3 normal)
    {
        if (effect == null) return;

        // Bereken de rotatie op basis van de normal van de muur/ondergrond
        Quaternion rotation = Quaternion.LookRotation(normal);

        ParticleSystem effectInstance = Instantiate(effect, point, rotation);
        effectInstance.Play();
        
        StartCoroutine(DestroyEffect(effectInstance));
    }

    private IEnumerator DestroyEffect(ParticleSystem effectInstance)
    {
        if (effectInstance != null)
        {
            yield return new WaitForSeconds(effectInstance.main.duration);
            Destroy(effectInstance.gameObject);
        }
    }
}
