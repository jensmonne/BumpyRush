using Mirror;
using UnityEngine;

public class Thief : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player: " + other.gameObject.name);
            GameManager.Instance.ChangeScore(other.gameObject.GetComponent<NetworkIdentity>().netId, -1);
            GameManager.Instance.ChangeScore(GetComponentInParent<NetworkIdentity>().netId, 1);
        }
        if (other.gameObject.CompareTag("Ground")) { return; }
        NetworkServer.Destroy(gameObject);
    }

}
