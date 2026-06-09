using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity))]
public class ItemManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> availableItems = new List<GameObject>();
    [SerializeField] private List<GameObject> currentItems = new List<GameObject>();
    private float itemUseCooldown = 6f;
    private float lastItemUseTime = -999f;

    private void Start()
    {
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["UseItem"].performed += UseItem;
        }
    }
    public void AddRandomItem()
    {
        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No available items to add!");
            return;
        }
        int randomIndex = Random.Range(0, availableItems.Count);
        GameObject randomItem = availableItems[randomIndex];
        AddItem(randomItem);
    }
    public void AddItem(GameObject item)
    {
        currentItems.Add(item);
        Debug.Log("Item added: " + item.name);
    }

    public void UseItem(InputAction.CallbackContext context)
    {
        Debug.Log("UseItem called with context: " + context);
        if (!isLocalPlayer) return;

        int index = 0;
        if (Time.time - lastItemUseTime < itemUseCooldown)
        {
            Debug.Log("Item use is on cooldown!");
            return;
        }
        if (index < 0 || index >= currentItems.Count)
        {
            Debug.Log("Invalid item index: " + index);
            return;
        }

        CmdUseItem(index);
        lastItemUseTime = Time.time;
    }

    [Command]
    private void CmdUseItem(int index)
    {
        if (index < 0 || index >= currentItems.Count)
        {
            Debug.Log("Invalid item index: " + index);
            return;
        }
        GameObject item = currentItems[index];

        Vector3 spawnPos = transform.position + -transform.forward * 1.5f;
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward);

        GameObject spawnedItem = Instantiate(item, spawnPos, spawnRot, gameObject.transform);
        Debug.Log("Using item: " + item.name);

        NetworkIdentity netIdentity = spawnedItem.GetComponent<NetworkIdentity>();
        if (netIdentity != null)
        {
            NetworkServer.Spawn(spawnedItem);
        }
        currentItems.RemoveAt(index);
        RpcItemUsed(item.name);
    }

    [ClientRpc]
    private void RpcItemUsed(string itemName)
    {
        Debug.Log("Item was used: " + itemName);
    }
}