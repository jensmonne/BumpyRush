using UnityEngine;

public class SpeedBumper : MonoBehaviour
{
    [SerializeField] private float bounceMultiplier = 1.2f;
    [SerializeField] private float minSpeed = 1.5f;

    private Rigidbody carRb;

    void Awake()
    {
        if (transform.parent != null)
        {
            carRb = transform.parent.GetComponent<Rigidbody>();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log("Colliding with: " + collision.gameObject.name);
        if (carRb == null) return;

        float speed = carRb.linearVelocity.magnitude;
        if (speed < minSpeed) return;

        Vector3 normal = collision.contacts[0].normal;

        // reflect current velocity
        Vector3 reflected = Vector3.Reflect(carRb.linearVelocity, normal);

        // force overwrite (this is the key fix)
        carRb.linearVelocity = reflected * bounceMultiplier;
    }
}