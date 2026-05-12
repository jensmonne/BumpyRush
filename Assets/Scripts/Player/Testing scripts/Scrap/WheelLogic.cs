using UnityEngine;

public class SimpleWheelStable : MonoBehaviour
{
    public Rigidbody carRb;
    public Transform wheelMesh;

    public float suspensionLength = 0.5f;
    public float springStrength = 8000f;
    public float damping = 1500f;

    public float maxForce = 12000f;

    private float previousCompression;

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.up);

        if (Physics.Raycast(ray, out RaycastHit hit, suspensionLength * 2f))
        {
            float compression = suspensionLength - hit.distance;

            // spring force (smooth)
            float springForce = compression * springStrength;

            // damping (stabilizes explosion issues)
            float velocity = (compression - previousCompression) / Time.fixedDeltaTime;
            float dampingForce = velocity * damping;

            float totalForce = springForce - dampingForce;

            // CLAMP (THIS fixes your flying bug)
            totalForce = Mathf.Clamp(totalForce, 0f, maxForce);

            carRb.AddForceAtPosition(transform.up * totalForce, transform.position);

            previousCompression = compression;
        }
        else
        {
            previousCompression = 0f;
        }
    }
}