using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dit script regelt de beweging van de speler, inclusief acceleratie, remmen, sturen, driften en springen.
/// Volledig gebaseerd op physics, met een focus op een arcade-achtige rijervaring.
/// </summary>
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float throttleResponse = 5f;

    [Header("Steering")]
    [SerializeField] private float turnSpeed = 120f;

    [Header("Drift")]
    [SerializeField] private float driftFactor = 0.98f;
    [SerializeField] private float velocityRotateSpeed = 2f;

    [Header("Ground Check")]
    [SerializeField] private Grounded grounded;

    [Header("StopMovements")]
    [SerializeField] private BounceFeatures stopMovement_Bounce;

    [Header("Effects")]
    [SerializeField] private ParticleSystem JumpParticles;
    [SerializeField] private Transform JumpEffectPoint;

    private Rigidbody rb3D;
    private PlayerSFX playerSFX;

    private float driveInput;
    private float steer;
    private float smoothedThrottle;

    private void Awake()
    {
        rb3D = GetComponent<Rigidbody>();
        playerSFX = GetComponent<PlayerSFX>();
    }

    // Vangt de input op voor gas geven (1) en achteruitrijden/remmen (-1)
    public void OnDrive(InputAction.CallbackContext context)
    {
        driveInput = Mathf.Clamp(context.ReadValue<float>(), -1f, 1f);
    }

    // Vangt de input op voor sturen (links = -1, rechts = 1)
    public void OnSteer(InputAction.CallbackContext context)
    {
        steer = context.ReadValue<float>();
    }

    // FixedUpdate wordt gebruikt voor physics-berekeningen (loopt synchroon met de physics engine)
    private void FixedUpdate()
    {
        // Zorgt voor het geleidelijk optrekken en afremmen van de input (geen instant 0 naar 100)
        smoothedThrottle = Mathf.Lerp(
            smoothedThrottle,
            driveInput,
            throttleResponse * Time.fixedDeltaTime
        );

        HandleMovement();
        HandleSteering();
        ApplyDrift();
    }

    // MOVEMENT (Vooruit/achteruit)
    void HandleMovement()
    {
        // Stop de code direct als de auto in de lucht hangt of net ergens tegenaan is gebonst
        if (!grounded.isGrounded) return;
        if (stopMovement_Bounce.hasCollided) return;

        float currentThrottle = smoothedThrottle;

        // Berekent hoe snel de auto momenteel vooruit (of achteruit) rijdt
        float forwardSpeed = Vector3.Dot(rb3D.linearVelocity, transform.forward);

        // Kracht toepassen op basis van de gas-input
        if (Mathf.Abs(currentThrottle) > 0.01f)
        {
            // Snelheidslimiet controleren voor achteruitrijden
            if (currentThrottle < 0f && forwardSpeed <= -maxSpeed * 0.5f)
            {
                return;
            }
            else
            {
                // Achteruitrijden gaat langzamer (30% van de kracht) dan vooruitrijden
                float reverseMultiplier = (currentThrottle < 0f) ? 0.3f : 1.0f;
                rb3D.AddForce(transform.forward * currentThrottle * speed * reverseMultiplier, ForceMode.Acceleration);
            }
        }

        // Snelheid omzetten naar lokale coördinaten om de maximale voorwaartse snelheid te begrenzen
        Vector3 localVelocity = transform.InverseTransformDirection(rb3D.linearVelocity);
        
        // BUG HIER GEVONDEN!!!! -maxspeed = vooruit en maxspeed = achteruit.
        // (Opmerking: Deze Clamp begrenst nu de Z-snelheid tussen de achteruit-limiet en vooruit-limiet)
        localVelocity.z = Mathf.Clamp(localVelocity.z, -maxSpeed * 0.5f, maxSpeed * 0.5f);
        
        // Zet de begrensde snelheid weer terug naar de wereld-coördinaten van de Rigidbody
        rb3D.linearVelocity = transform.TransformDirection(localVelocity);
    }

    // STUREN
    void HandleSteering()
    {
        // Berekent hoe snel de auto rijdt t.o.v. de topsnelheid (sturen werkt alleen als je rijdt)
        float speedFactor = rb3D.linearVelocity.magnitude / maxSpeed;

        if (speedFactor > 0.05f)
        {
            // Controleert of we vooruit of achteruit rijden, zodat het sturen omdraait bij achteruitrijden
            float forwardSpeed = Vector3.Dot(rb3D.linearVelocity, transform.forward);
            float directionSign = (forwardSpeed >= 0f) ? 1f : -1f;

            // Berekent de rotatiehoek op basis van input, draaisnelheid en hoe snel de auto gaat
            float rotationAmount =
                steer *
                turnSpeed *
                speedFactor *
                directionSign *
                Time.fixedDeltaTime;

            // Roteert het model van de auto
            rb3D.MoveRotation(
                rb3D.rotation *
                Quaternion.Euler(0f, rotationAmount, 0f)
            );

            // Roteert de *bewegingsrichting* (velocity) mee, zodat de auto niet alleen draait maar ook die kant op gaat
            Vector3 rotatedVelocity =
                Quaternion.Euler(0f, rotationAmount, 0f) *
                rb3D.linearVelocity;

            // Zorgt voor een soepele overgang tussen de oude richting en de nieuwe rijrichting
            rb3D.linearVelocity = Vector3.Lerp(
                rb3D.linearVelocity,
                rotatedVelocity,
                velocityRotateSpeed * Time.fixedDeltaTime
            );
        }
    }

    // DRIFTEN (Zwaartekracht/Grip-simulatie)
    void ApplyDrift()
    {
        // Haalt de lokale snelheid op (X = zijwaarts, Z = voorwaarts)
        Vector3 localVelocity = transform.InverseTransformDirection(rb3D.linearVelocity);

        // Hoe lager de factor, hoe sneller de auto stopt met slippen en grip krijgt.
        localVelocity.x *= driftFactor;

        // Past de aangepaste snelheid weer toe op de Rigidbody
        rb3D.linearVelocity = transform.TransformDirection(localVelocity);
    }

    // SPRINGEN
    public void OnJump(InputAction.CallbackContext context)
    {
        // checks om te voorkomen dat de spleler springt
        if (!context.performed) return;
        if (!grounded.isGrounded) return;

        // geft een directe fysieke impuls omhoog (de jump zelf)
        rb3D.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );

        // Spawnt de jump-particlessssss
        ParticleSystem JumpEffect = Instantiate(
            JumpParticles,
            JumpEffectPoint.position,
            Quaternion.identity
        );
        JumpEffect.Play();
        Destroy(JumpEffect.gameObject, JumpEffect.main.duration);

        // jump sfx
        playerSFX.PlayJumpSound();
    }
}