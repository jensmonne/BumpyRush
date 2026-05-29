using System.Collections;
using UnityEngine;

/// <summary>
/// Dit script detecteert wanneer de speler hard landt en stabiliseert de landing door spin en bounce te verminderen,
/// en tijdelijk rotaties te locken.
/// </summary>
public class LandingStabilizer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Rigidbody van Bumpy")]
    public Rigidbody rb;

    [Tooltip("Grounded component Bumpy")]
    public Collider GroundedCollider;

    [Tooltip("Bounce collider van Bumpy")]
    public Collider BounceCollider;

    [Header("Impact Settings")]
    public float hardLandingThreshold = 6f;

    [Header("Stabilizer")]
    public float lockDuration = 0.08f;
    public float angularDamping = 0.15f;
    public float velocityDamping = 0.8f;

    private bool stabilizing = false;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        float impactSpeed = Mathf.Abs(rb.linearVelocity.y);

        //landing
        if (impactSpeed >= hardLandingThreshold && !stabilizing)
        {
            StartCoroutine(StabilizeLanding(impactSpeed));
        }
    }

    IEnumerator StabilizeLanding(float impactSpeed)
    {
        stabilizing = true;

        //Debug.Log("Hard Landing Speed: " + impactSpeed);

        //spin verminderen
        rb.angularVelocity *= angularDamping;

        //bbounce verminderen
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            rb.linearVelocity.y * velocityDamping,
            rb.linearVelocity.z
        );

        //Bounce collider tijdelijk uitschakelen
        BounceCollider.enabled = false;

        //rotaties locken
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        yield return new WaitForSeconds(lockDuration);

        //terugetten
        BounceCollider.enabled = true;

        rb.constraints = RigidbodyConstraints.None;

        stabilizing = false;
    }
}