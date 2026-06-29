using UnityEngine;

public class OilSpill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the oil spill!" + other.gameObject.name);
            Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
            rb.AddExplosionForce(1500, transform.position, 5f, 20f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
