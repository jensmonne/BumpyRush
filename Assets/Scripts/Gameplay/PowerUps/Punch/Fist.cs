using Mirror;
using UnityEngine;

public class Fist : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float knockbackAmount = 1800f;

    private void Start()
    {
        if (isServer)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player: " + other.gameObject.name);
            other.gameObject.GetComponentInParent<Rigidbody>().AddForce(transform.forward * knockbackAmount, ForceMode.Impulse);
        }
        if (other.gameObject.CompareTag("Ground")) { return; }
        Destroy(gameObject);
    }
}
