using System.Collections;
using UnityEngine;

public class StopMovement_Bounce : MonoBehaviour
{
    [SerializeField] private Collider BounceCollider;
    [SerializeField] private Rigidbody CartRigidbody;

    [Header("Collision Duration")]
    [SerializeField] private float minimumDuration = 0.2f;
    [SerializeField] private float maximumDuration = 3f;
    [SerializeField] private float impactMultiplier = 0.2f;

    [Header("Bounce Force")]
    [SerializeField] private float bounceForceMultiplier = 10f;
    [SerializeField] private float maxBounceForce = 30f;

    public bool hasCollided = false;

    private Coroutine stopCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider == BounceCollider)
            {
                float impactForce = collision.relativeVelocity.magnitude;

                // Duration
                float duration = impactForce * impactMultiplier;
                duration = Mathf.Clamp(duration, minimumDuration, maximumDuration);

                // Bounce
                Vector3 bounceDirection = contact.normal;

                float bounceForce = impactForce * bounceForceMultiplier;
                bounceForce = Mathf.Clamp(bounceForce, 0f, maxBounceForce);

                CartRigidbody.AddForce(
                    bounceDirection * bounceForce,
                    ForceMode.Impulse
                );

                if (stopCoroutine != null)
                {
                    StopCoroutine(stopCoroutine);
                }

                stopCoroutine = StartCoroutine(StopMovementTimer(duration));

                Debug.Log("Impact: " + impactForce);
                Debug.Log("Bounce Force: " + bounceForce);

                return;
            }
        }
    }

    private IEnumerator StopMovementTimer(float duration)
    {
        hasCollided = true;

        yield return new WaitForSeconds(duration);

        hasCollided = false;
    }
}