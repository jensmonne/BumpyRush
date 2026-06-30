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
        gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y + 180f, gameObject.transform.rotation.eulerAngles.z);
    }

    private void Update()
    {
        if (isServer)
        {
            transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Fist hit: " + other.gameObject.name);

        if (!isServer) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player: " + other.gameObject.name);
            Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * knockbackAmount, ForceMode.Impulse);
            }
        }

        if (other.gameObject.CompareTag("Ground")) { return; }
        NetworkServer.Destroy(gameObject);
    }
}