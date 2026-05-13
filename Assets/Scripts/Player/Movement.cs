using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Steering")]
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Drift")]
    [SerializeField] private float driftFactor = 0.95f;
    [SerializeField] private float velocityRotateSpeed = 2f;

    private Vector2 moveInput = Vector2.zero;
    private Rigidbody rb3D;

    private void Awake()
    {
        rb3D = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleSteering();
        ApplyDrift();
    }

    void HandleMovement()
    {
        float input = 0f;

        if (moveInput.y < 0) input += 1f;
        if (moveInput.y > 0) input -= 1f;

        // Gas geven
        rb3D.AddForce(transform.forward * input * speed, ForceMode.Acceleration);

        // Max snelheid limiter
        if (rb3D.linearVelocity.magnitude > maxSpeed)
        {
            rb3D.linearVelocity = rb3D.linearVelocity.normalized * maxSpeed;
        }
    }

    void HandleSteering()
    {
        float steer = 0f;

        if (moveInput.x < 0) steer -= 1f;
        if (moveInput.x > 0) steer += 1f;

        // Alleen sturen als de auto beweegt
        float speedFactor = rb3D.linearVelocity.magnitude / maxSpeed;

        if (speedFactor > 0.05f)
        {
            float rotationAmount =
                steer * turnSpeed * speedFactor * Time.fixedDeltaTime;

            rb3D.MoveRotation(
                rb3D.rotation * Quaternion.Euler(0f, rotationAmount, 0f)
            );

            // Draai velocity langzaam mee met de auto
            Vector3 rotatedVelocity = Quaternion.Euler(0f, rotationAmount, 0f) * rb3D.linearVelocity;

            rb3D.linearVelocity = Vector3.Lerp(
                rb3D.linearVelocity,
                rotatedVelocity,
                velocityRotateSpeed * Time.fixedDeltaTime
            );
        }
    }

    void ApplyDrift()
    {
        // Lokale velocity
        Vector3 localVelocity = transform.InverseTransformDirection(rb3D.linearVelocity);

        // Minder grip zijwaarts = meer drift
        localVelocity.x *= driftFactor;

        // Terug naar world velocity
        rb3D.linearVelocity = transform.TransformDirection(localVelocity);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        rb3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}