using UnityEngine;
using UnityEngine.InputSystem;

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

    private Rigidbody rb3D;

    // Input values
    private float throttle;
    private float brake;
    private float steer;
    private float smoothedThrottle;
    private bool isGrounded;

    private void Awake()
    {
        rb3D = GetComponent<Rigidbody>();
    }

    // INPUT SYSTEM
    public void OnAccelerate(InputAction.CallbackContext context)
    {
        throttle = context.ReadValue<float>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        brake = context.ReadValue<float>();
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        steer = context.ReadValue<float>();
    }

    // FIXED UPDATE
    private void FixedUpdate()
    {
        smoothedThrottle = Mathf.Lerp(
        smoothedThrottle,
        throttle - brake,
        throttleResponse * Time.fixedDeltaTime);

        HandleMovement();
        HandleSteering();
        ApplyDrift();
    }

    // MOVEMENT
    void HandleMovement()
    {
        if (!isGrounded) return;

        float driveInput = smoothedThrottle;

        float targetMaxSpeed = maxSpeed * Mathf.Abs(driveInput);

        float forwardSpeed =
            Vector3.Dot(rb3D.linearVelocity, transform.forward);

        if (Mathf.Abs(forwardSpeed) < targetMaxSpeed)
        {
            rb3D.AddForce(
                transform.forward * driveInput * speed,
                ForceMode.Acceleration
            );
        }
    }

    // STEERING
    void HandleSteering()
    {
        float speedFactor =
            rb3D.linearVelocity.magnitude / maxSpeed;

        if (speedFactor > 0.05f)
        {
            float rotationAmount =
                steer *
                turnSpeed *
                speedFactor *
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
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb3D.linearVelocity);

        // Zijwaartse grip
        localVelocity.x *= driftFactor;

        rb3D.linearVelocity =
            transform.TransformDirection(localVelocity);
    }

    // JUMP
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!isGrounded) return;
        
        rb3D.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    //COLLISIONS
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}