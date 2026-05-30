using UnityEngine;

public class WheelGroundCheck : MonoBehaviour
{
    private PhysicsCarController car;

    void Awake()
    {
        car = GetComponentInParent<PhysicsCarController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGround(collision, true);
    }

    void OnCollisionExit(Collision collision)
    {
        CheckGround(collision, false);
    }

    void CheckGround(Collision collision, bool entering)
    {
        Debug.Log((entering ? "Entering" : "Exiting") + " collision with: " + collision.gameObject.name);
        if (collision.collider.CompareTag("Ground"))
        {
            if (entering)
                car.WheelTouchGround();
            else
                car.WheelLeaveGround();
        }
    }
}