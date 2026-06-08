using UnityEngine;
using System.Collections;

public class BounceParticles : MonoBehaviour
{
    [Tooltip("The particle system Prefab for bounce")]
    [SerializeField] private ParticleSystem effect;

    [Tooltip("The bounce collider on this object")]
    [SerializeField] private BoxCollider bounceCollider;

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
                ParticleSystem effectInstance = Instantiate(effect, contact.point, Quaternion.LookRotation(contact.normal));
                effectInstance.transform.position = contact.point;
                effectInstance.transform.rotation = Quaternion.LookRotation(contact.normal);
                effectInstance.Play();
                StartCoroutine(DestroyEffect(effectInstance));
            }
        }
    }

    private IEnumerator DestroyEffect(ParticleSystem effectInstance)
    {
        yield return new WaitForSeconds(effectInstance.main.duration);
        Destroy(effectInstance.gameObject);
    }
}
