using UnityEngine;

public class OilSpill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the oil spill!" + other.gameObject.name);
            Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
            rb.AddExplosionForce(5000f, transform.position, 5f, 5f, ForceMode.Impulse);
        }
    }
}
