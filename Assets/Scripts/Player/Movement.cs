using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float maxSpeed = 10f;

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
        HandleMovement1();
        HandleSteering();
    }

    void HandleMovement()
    {
        if (rb3D == null) return;

        Vector3 inputWorld = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputWorld.sqrMagnitude > 1f)
        {
            inputWorld.Normalize();
        }

        Vector3 force = inputWorld * speed;
        rb3D.AddForce(force, ForceMode.Acceleration);
    }

    //Testing
    void HandleMovement1()
    {
        float input = 0f;
        if (moveInput.y < 0) input += 1f;
        if (moveInput.y > 0) input -= 1f;

        rb3D.AddForce(transform.forward * input * speed, ForceMode.Acceleration);
    }


    void HandleSteering()
    {
        float steer = 0f;
        if (moveInput.x < 0) steer -= 1f;
        if (moveInput.x > 0) steer += 1f;

        float speedFactor = rb3D.linearVelocity.magnitude / maxSpeed;

        rb3D.MoveRotation(rb3D.rotation *
            Quaternion.Euler(0f, steer * turnSpeed * speedFactor * Time.fixedDeltaTime, 0f));
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (rb3D != null)
        {
            rb3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }
}
