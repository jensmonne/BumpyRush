using UnityEngine;

public class OilSpill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the oil spill!" + collision.gameObject.name);
            Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();
            Vector3 com = rb.worldCenterOfMass;
            Vector3 explosionPos = new Vector3(com.x + 0.1f, transform.position.y, com.z);
            rb.AddExplosionForce(5000, explosionPos, 5f, 20f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
