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
    [SerializeField] private StopMovement_Bounce stopMovement_Bounce;

    [Header("Effects")]
    [SerializeField] private ParticleSystem JumpParticles;
    [SerializeField] private Transform JumpEffectPoint;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;

    private Rigidbody rb3D;

    // Input values
    private float driveInput;
    private float steer;
    private float smoothedThrottle;

    private void Awake()
    {
        rb3D = GetComponent<Rigidbody>();
    }

    // INPUT SYSTEM (via PlayerInput component)
    public void OnDrive(InputAction.CallbackContext context)
    {
        // VEILIGHEIDSMAATREGEL: We klemmen de input HARD tussen -1 en 1.
        // Mocht je controller een rare waarde doorgeven, dan filteren we dat hier eruit.
        driveInput = Mathf.Clamp(context.ReadValue<float>(), -1f, 1f);
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        steer = context.ReadValue<float>();
    }

    // UPDATE
    private void Update()
    {
        //Help bumpy
        if (grounded.HELP)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }

    // FIXED UPDATE
    private void FixedUpdate()
    {
        smoothedThrottle = Mathf.Lerp(
            smoothedThrottle,
            driveInput,
            throttleResponse * Time.fixedDeltaTime
        );

        HandleMovement();
        HandleSteering();
        ApplyDrift();
    }

    // MOVEMENT
    void HandleMovement()
    {
        if (!grounded.isGrounded) return;
        if (stopMovement_Bounce.hasCollided) return;

        float currentThrottle = smoothedThrottle;

        float forwardSpeed = Vector3.Dot(rb3D.linearVelocity, transform.forward);

        // Pas de kracht toe
        if (Mathf.Abs(currentThrottle) > 0.01f)
        {
            if (currentThrottle < 0f && forwardSpeed <= -maxSpeed * 0.5f)
            {
                // Als we achteruit rijden en de snelheid is al op de limiet, doe niets
                return;
            }
            else
            {
                float reverseMultiplier = (currentThrottle < 0f) ? 0.3f : 1.0f;
                rb3D.AddForce(transform.forward * currentThrottle * speed * reverseMultiplier, ForceMode.Acceleration);
            }
        }

        Vector3 localVelocity = transform.InverseTransformDirection(rb3D.linearVelocity);
        
        // BUG HIER GEVONDEN!!!! -maxspeed = vooruit en maxspeed = achteruit.
        localVelocity.z = Mathf.Clamp(localVelocity.z, -maxSpeed * 0.5f, maxSpeed * 0.5f);
        
        rb3D.linearVelocity = transform.TransformDirection(localVelocity);
    }

    // STEERING
    void HandleSteering()
    {
        float speedFactor = rb3D.linearVelocity.magnitude / maxSpeed;

        if (speedFactor > 0.05f)
        {
            float forwardSpeed = Vector3.Dot(rb3D.linearVelocity, transform.forward);
            float directionSign = (forwardSpeed >= 0f) ? 1f : -1f;

            float rotationAmount =
                steer *
                turnSpeed *
                speedFactor *
                directionSign *
                Time.fixedDeltaTime;

            rb3D.MoveRotation(
                rb3D.rotation *
                Quaternion.Euler(0f, rotationAmount, 0f)
            );

            Vector3 rotatedVelocity =
                Quaternion.Euler(0f, rotationAmount, 0f) *
                rb3D.linearVelocity;

            rb3D.linearVelocity = Vector3.Lerp(
                rb3D.linearVelocity,
                rotatedVelocity,
                velocityRotateSpeed * Time.fixedDeltaTime
            );
        }
    }

    // DRIFT
    void ApplyDrift()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb3D.linearVelocity);

        localVelocity.x *= driftFactor;

        rb3D.linearVelocity = transform.TransformDirection(localVelocity);
    }

    // JUMP
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!grounded.isGrounded) return;

        rb3D.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );

        // Speel jump effect
        ParticleSystem JumpEffect = Instantiate(
            JumpParticles,
            JumpEffectPoint.position,
            Quaternion.identity
        );
        JumpEffect.Play();
        Destroy(JumpEffect.gameObject, JumpEffect.main.duration);

        //sfx
        SoundManager.Instance.Play3DSFX(jumpSound, transform.position);
    }
}