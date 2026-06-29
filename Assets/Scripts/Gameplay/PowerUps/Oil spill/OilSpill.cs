using UnityEngine;

public class OilSpill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the oil spill!" + collision.gameObject.name);
            Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();
            rb.AddExplosionForce(1500, transform.position, 5f, 20f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
