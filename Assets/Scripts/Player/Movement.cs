using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;

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
        if (rb3D == null) return;

        Vector3 inputWorld = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputWorld.sqrMagnitude > 1f)
        {
            inputWorld.Normalize();
        }

        Vector3 force = inputWorld * speed;
        rb3D.AddForce(force, ForceMode.Acceleration);
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
