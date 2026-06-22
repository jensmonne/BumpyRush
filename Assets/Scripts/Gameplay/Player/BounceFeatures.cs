using UnityEngine;
using Mirror;

/// <summary>
/// Dit script detecteert wanneer de bounce collider een impact heeft en geeft
/// een bounce kracht terug op basis van de impact kracht, 
/// en stopt de movement tijdelijk in he movement script via de hasCollided bool, die wordt gereset zodra de bounce bijna gestopt is.
/// </summary>
public class BounceFeatures : NetworkBehaviour
{
    [SerializeField] private Collider bounceCollider;
    [SerializeField] private Rigidbody cartRigidbody;

    [Header("Bounce Force")]
    [SerializeField] private float bounceForceMultiplier = 10f;
    [SerializeField] private float maxBounceForce = 30f;
    [SerializeField] private float minimumImpactForce = 3f;

    [Tooltip("Hoeveel kracht er op de speler wordt toegepast bij een bounce, als deze een andere speler raakt.")]
    [SerializeField] private float bounceForceOnPlayer = 100f;

    [SerializeField] private float environmentBounceMultiplier = 2.5f;

    [Header("Release Settings")]
    [SerializeField] private float maxBounceDuration = 0.35f;

    public bool hasCollided = false;
    private float bounceTimer = 0f;

    private void FixedUpdate()
    {
        if (hasCollided)
        {
            bounceTimer -= Time.fixedDeltaTime;
            if (bounceTimer <= 0f)
            {
                hasCollided = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Alleen de lokale speler die de botsing veroorzaakt berekent dit
        if (!isLocalPlayer) return; 

        if (collision.gameObject.CompareTag("PhyObject") || collision.gameObject.CompareTag("Bridge"))
            return;

        bool isPlayerCollision = collision.gameObject.CompareTag("Player");

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider == bounceCollider)
            {
                float impactForce = collision.relativeVelocity.magnitude;
                if (impactForce < minimumImpactForce) return;

                Vector3 bounceDirection = contact.normal;
                float bounceForce = Mathf.Clamp(impactForce * bounceForceMultiplier, 0f, maxBounceForce);

                float selfApplyForce = bounceForce;

                if (!isPlayerCollision)
                {
                    selfApplyForce *= environmentBounceMultiplier;
                }

                cartRigidbody.AddForce(bounceDirection * selfApplyForce, ForceMode.Impulse);

                float impactPercentage = bounceForce / maxBounceForce;
                float calculatedDuration = impactPercentage * maxBounceDuration;

                TriggerBounceState(calculatedDuration);

                // De andere speler wegbeuken via netwerk
                if (isPlayerCollision)
                {
                    //Debug.Log($"Player {collision.gameObject.name} has been hit with a force of {bounceForce} in direction {bounceDirection}");
                    // Mirror NetworkIdentity
                    NetworkIdentity targetIdentity = collision.gameObject.GetComponent<NetworkIdentity>();
                    
                    if (targetIdentity != null)
                    {
                        Vector3 pushDirection = -bounceDirection;
                        float bumpyPushForce = bounceForce * bounceForceOnPlayer;
                        Vector3 finalForce = pushDirection * bumpyPushForce;

                        // request voor de beuk
                        CmdRequestBounce(targetIdentity, finalForce, calculatedDuration);
                    }
                }
                return;
            }
        }
    }

    private void TriggerBounceState(float duration)
    {
        hasCollided = true;
        bounceTimer = duration;
    }

    // Een Command stuurt data van de Client naar de Server
    [Command]
    private void CmdRequestBounce(NetworkIdentity targetIdentity, Vector3 force, float duration)
    {
        if (targetIdentity == null) return;

        // Haal het BounceFeatures script op van de speler die we geraakt hebben
        BounceFeatures targetBounceScript = targetIdentity.GetComponent<BounceFeatures>();

        if (targetBounceScript != null)
        {
            // TargetRpc stuurt data specifiek naar de client die de 'targetIdentity' bezit
            targetBounceScript.TargetApplyBounce(targetIdentity.connectionToClient, force, duration);
        }
    }

    // Een TargetRpc voert ALTIJD uit op de PC van de speler die gekoppeld is aan de NetworkConnection
    [TargetRpc]
    private void TargetApplyBounce(NetworkConnectionToClient target, Vector3 force, float duration)
    {
        if (cartRigidbody != null)
        {
            // Pas de kracht toe op andere speler via physics
            cartRigidbody.AddForce(force, ForceMode.Impulse);
            
            // Zet hun eigen hasCollided op true
            TriggerBounceState(duration);
        }
    }
}