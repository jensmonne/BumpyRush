using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsCarController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    private InputAction forwardAction;
    private InputAction backAction;
    private InputAction leftAction;
    private InputAction rightAction;

    private bool forward;
    private bool back;
    private bool left;
    private bool right;

    [Header("Car Settings")]
    public Rigidbody rb;
    public float acceleration = 25f;
    public float turnSpeed = 120f;
    public float maxSpeed = 12f;
    public float grip = 5f;

    // 🛞 wheel grounding
    private int groundedWheels = 0;
    private bool CanDrive => groundedWheels > 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        inputActions.Enable();

        forwardAction = inputActions.FindAction("Forward");
        backAction = inputActions.FindAction("Backward");
        leftAction = inputActions.FindAction("Left");
        rightAction = inputActions.FindAction("Right");

        forwardAction.performed += ctx => forward = true;
        forwardAction.canceled += ctx => forward = false;

        backAction.performed += ctx => back = true;
        backAction.canceled += ctx => back = false;

        leftAction.performed += ctx => left = true;
        leftAction.canceled += ctx => left = false;

        rightAction.performed += ctx => right = true;
        rightAction.canceled += ctx => right = false;
    }

    void FixedUpdate()
    {
        if (CanDrive)
        {
            HandleMovement();
            HandleSteering();
        }

        HandleGrip();
        ClampSpeed();
    }

    void HandleMovement()
    {
        float input = 0f;
        if (forward) input += 1f;
        if (back) input -= 1f;

        rb.AddForce(transform.forward * input * acceleration, ForceMode.Acceleration);
    }

    void HandleSteering()
    {
        float steer = 0f;
        if (left) steer -= 1f;
        if (right) steer += 1f;

        float speedFactor = rb.linearVelocity.magnitude / maxSpeed;

        rb.MoveRotation(rb.rotation *
            Quaternion.Euler(0f, steer * turnSpeed * speedFactor * Time.fixedDeltaTime, 0f));
    }

    void HandleGrip()
    {
        Vector3 lateral = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        rb.AddForce(-lateral * grip, ForceMode.Acceleration);
    }

    void ClampSpeed()
    {
        Vector3 flat = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flat.magnitude > maxSpeed)
        {
            Vector3 limited = flat.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
    }

    // 🛞 CALLED BY WHEELS
    public void WheelTouchGround()
    {
        groundedWheels++;
    }

    public void WheelLeaveGround()
    {
        groundedWheels = Mathf.Max(0, groundedWheels - 1);
    }
}