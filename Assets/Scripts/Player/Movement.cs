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
        Move();
    }

    public void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * speed * Time.deltaTime;

        if (rb3D != null)
        {
            rb3D.AddForce(transform.position + movement, ForceMode.VelocityChange);
        }
        else
        {
            transform.Translate(movement, Space.World);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (rb3D != null)
        {
            rb3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else
        {
            transform.position += Vector3.up * (jumpForce * 0.1f);
        }
    }
}
