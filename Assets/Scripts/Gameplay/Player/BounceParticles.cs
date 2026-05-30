using UnityEngine;

public class BounceParticles : MonoBehaviour
{
    [Tooltip("The particle system Prefab for bounce")]
    [SerializeField] private ParticleSystem effect;

    [Tooltip("The bounce collider on this object")]
    [SerializeField] private BoxCollider bounceCollider;

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
        if (collision.gameObject.CompareTag("PhyObject"))
        {
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider == bounceCollider)
            {
                Debug.Log("Bounce collision detected at point: " + contact.point);
                ParticleSystem effectInstance = Instantiate(effect, contact.point, Quaternion.LookRotation(contact.normal));
                effectInstance.transform.position = contact.point;
                effectInstance.transform.rotation = Quaternion.LookRotation(contact.normal);
                effectInstance.Play();
            }
        }
    }
}
