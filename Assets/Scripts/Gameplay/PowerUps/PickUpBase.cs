using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Collider))]
public abstract class PickUpBase : NetworkBehaviour
{
    [SerializeField] private string playerTag = "Player";

    [SyncVar] private bool isCollected;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (!other.CompareTag(playerTag)) return;

        NetworkIdentity playerIdentity = other.GetComponent<NetworkIdentity>();
        if (playerIdentity == null)
        {
            playerIdentity = other.GetComponentInParent<NetworkIdentity>();
        }

        if (playerIdentity == null) return;

        Collect(playerIdentity);
    }

    [Server]
    private void Collect(NetworkIdentity playerIdentity)
    {
        if (isCollected) return;

        isCollected = true;
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.RegisterPickup(playerIdentity);
        }

        OnPickUpServer(playerIdentity);
        RpcOnPickUpClient(playerIdentity.netId);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    protected virtual void OnPickUpServer(NetworkIdentity playerIdentity)
    {
        Debug.Log($"Picked up: {gameObject.name} by player netId {playerIdentity.netId}");
    }

    [ClientRpc]
    private void RpcOnPickUpClient(uint playerNetId)
    {
        NetworkIdentity playerIdentity = null;
        if (NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity spawnedIdentity))
        {
            playerIdentity = spawnedIdentity;
        }

        OnPickUpClient(playerIdentity);
    }

    protected virtual void OnPickUpClient(NetworkIdentity playerIdentity)
    {
        // Space for client-side effects sounds, particles, idk
    }

}
