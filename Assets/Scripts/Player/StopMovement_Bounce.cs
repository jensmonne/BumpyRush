using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// Dit script detecteert wanneer de bounce collider een impact heeft en geeft
/// een bounce kracht terug op basis van de impact kracht, 
/// en stopt de movement tijdelijk in he movement script via de hasCollided bool, die wordt gereset zodra de bounce bijna gestopt is.
/// </summary>
public class StopMovement_Bounce : MonoBehaviour
{
    [SerializeField] private Collider bounceCollider;
    [SerializeField] private Rigidbody cartRigidbody;

    [Header("Bounce Force")]
    [SerializeField] private float bounceForceMultiplier = 10f;
    [SerializeField] private float maxBounceForce = 30f;
    [SerializeField] private float minimumImpactForce = 3f;

    [Header("Release Settings")]
    [SerializeField] private float releaseVelocityThreshold = 1f;

    public bool hasCollided = false;

    private void FixedUpdate()
    {
        if (hasCollided)
        {
            // Zodra de bounce bijna gestopt is
            if (cartRigidbody.linearVelocity.magnitude < releaseVelocityThreshold)
            {
                hasCollided = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider == bounceCollider)
            {
                float impactForce = collision.relativeVelocity.magnitude;

                // Ignore soft scrapes
                if (impactForce < minimumImpactForce)
                    return;

                Vector3 bounceDirection = contact.normal;

                float bounceForce = impactForce * bounceForceMultiplier;
                bounceForce = Mathf.Clamp(bounceForce, 0f, maxBounceForce);

                cartRigidbody.AddForce(
                    bounceDirection * bounceForce,
                    ForceMode.Impulse
                );

                hasCollided = true;

                return;
            }
        }
    }
}