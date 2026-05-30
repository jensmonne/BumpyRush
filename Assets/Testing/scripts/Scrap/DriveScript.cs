using UnityEngine;
using UnityEngine.InputSystem;

public class DriveScript : MonoBehaviour
{
    private InputActionAsset inputActionAsset;
    private InputAction leftAction, rightAction, forwardAction, backwardsAction;

    [SerializeField] float speed = 5f;
    [SerializeField] InputActionAsset inputActions;

    private bool movingLeft = false, movingRight = false, movingForward = false, movingBackwards = false;

    private bool isGrounded = true;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        inputActionAsset = inputActions;

        if (inputActionAsset != null)
        {
            inputActionAsset.Enable();

            leftAction = inputActionAsset.FindAction("Left");
            rightAction = inputActionAsset.FindAction("Right");
            forwardAction = inputActionAsset.FindAction("Forward");
            backwardsAction = inputActionAsset.FindAction("Backward");

            if (leftAction != null)
            {
                leftAction.performed += OnLeftPerformed;
                leftAction.canceled += OnLeftCanceled;
            }

            if (rightAction != null)
            {
                rightAction.performed += OnRightPerformed;
                rightAction.canceled += OnRightCanceled;
            }

            if (forwardAction != null)
            {
                forwardAction.performed += OnForwardPerformed;
                forwardAction.canceled += OnForwardCanceled;
            }

            if (backwardsAction != null)
            {
                backwardsAction.performed += OnBackwardsPerformed;
                backwardsAction.canceled += OnBackwardsCanceled;
            }
        }
    }

    void OnDisable()
    {
        if (leftAction != null)
        {
            leftAction.performed -= OnLeftPerformed;
            leftAction.canceled -= OnLeftCanceled;
        }

        if (rightAction != null)
        {
            rightAction.performed -= OnRightPerformed;
            rightAction.canceled -= OnRightCanceled;
        }

        if (forwardAction != null)
        {
            forwardAction.performed -= OnForwardPerformed;
            forwardAction.canceled -= OnForwardCanceled;
        }

        if (backwardsAction != null)
        {
            backwardsAction.performed -= OnBackwardsPerformed;
            backwardsAction.canceled -= OnBackwardsCanceled;
        }

        if (inputActionAsset != null)
            inputActionAsset.Disable();
    }

    void FixedUpdate()
    {
        if (!isGrounded) return;

        float input = 0f;

        if (movingForward) input += 1f;
        if (movingBackwards) input -= 1f;

        float steer = 0f;
        if (movingLeft) steer -= 1f;
        if (movingRight) steer += 1f;

        float acceleration = 25f;
        float maxSpeed = speed;

        Vector3 velocity = rb.linearVelocity;

        // huidige snelheid in forward richting
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

        // INPUT = versnellen
        if (input != 0)
        {
            float targetSpeed = input * maxSpeed;

            float speedDiff = targetSpeed - forwardSpeed;
            float accel = speedDiff * acceleration;

            rb.AddForce(transform.forward * accel);
        }
        else
        {
            // COASTING / UITROLLEN
            float coastDrag = 0.98f;

            Vector3 flatVel = new Vector3(velocity.x, 0, velocity.z);
            flatVel *= coastDrag;

            rb.linearVelocity = new Vector3(flatVel.x, velocity.y, flatVel.z);
        }

        // steering (werkt ook tijdens coasting)
        if (Mathf.Abs(forwardSpeed) > 0.1f)
        {
            float turnStrength = 120f;
            float direction = Mathf.Sign(forwardSpeed); // auto draait realistischer

            float turn = steer * turnStrength * Time.fixedDeltaTime * direction;

            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    // Input callbacks
    private void OnLeftPerformed(InputAction.CallbackContext ctx) => movingLeft = true;
    private void OnLeftCanceled(InputAction.CallbackContext ctx) => movingLeft = false;

    private void OnRightPerformed(InputAction.CallbackContext ctx) => movingRight = true;
    private void OnRightCanceled(InputAction.CallbackContext ctx) => movingRight = false;

    private void OnForwardPerformed(InputAction.CallbackContext ctx) => movingForward = true;
    private void OnForwardCanceled(InputAction.CallbackContext ctx) => movingForward = false;

    private void OnBackwardsPerformed(InputAction.CallbackContext ctx) => movingBackwards = true;
    private void OnBackwardsCanceled(InputAction.CallbackContext ctx) => movingBackwards = false;


    public void ApplyBounce(Vector3 hitNormal, float force)
    {
        // knockback
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-hitNormal * force, ForceMode.Impulse);

        // optional: tijdelijk minder controle (feel)
        StartCoroutine(ControlLock(0.25f));
    }

    private System.Collections.IEnumerator ControlLock(float time)
    {   
        isGrounded = false;

        yield return new WaitForSeconds(time);

        isGrounded = true;
    }
}