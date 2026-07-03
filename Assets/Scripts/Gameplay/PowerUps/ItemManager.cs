using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkIdentity))]
public class ItemManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> availableItems = new();
    [SerializeField] private List<GameObject> currentItems = new();

    private float itemUseCooldown = 6f;
    private float lastItemUseTime = -999f;

    private void Start()
    {
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            playerInput.actions["UseItem"].performed += UseItem;
    }

    public void AddRandomItem()
    {
        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No available items to add!");
            return;
        }
        AddItem(availableItems[Random.Range(0, availableItems.Count)]);
    }

    public void AddItem(GameObject item)
    {
        currentItems.Add(item);
        Debug.Log("Item added: " + item.name);
        TargetItemAdded(item.name);
    }

    [TargetRpc]
    private void TargetItemAdded(string itemName)
    {
        Debug.Log("TargetItemAdded received: " + itemName);
        PowerUpUI.SetPowerup(itemName);
    }

    public void UseItem(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;

        if (Time.time - lastItemUseTime < itemUseCooldown) { Debug.Log("Item use is on cooldown!"); return; }
        if (currentItems.Count == 0) { Debug.Log("No items to use!"); return; }
        CmdUseItem(0);
        lastItemUseTime = Time.time;
        PowerUpUI.SetPowerup("");
        Debug.Log("Current items count: " + currentItems.Count);
        PowerUpUI.ClearPowerup();
    }

    [Command]
    private void CmdUseItem(int index)
    {
        if (index < 0 || index >= currentItems.Count) { Debug.Log("Invalid item index: " + index); return; }

        GameObject item = currentItems[index];
        Vector3 spawnPos = transform.position - transform.forward * 1.5f;
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward);
        GameObject spawnedItem = Instantiate(item, spawnPos, spawnRot, gameObject.transform);
        NetworkIdentity netIdentity = spawnedItem.GetComponent<NetworkIdentity>();
        if (netIdentity != null) NetworkServer.Spawn(spawnedItem);
        currentItems.RemoveAt(index);
        RpcItemUsed(item.name);
    }

    [ClientRpc]
    private void RpcItemUsed(string itemName)
    {
        Debug.Log("Item was used: " + itemName);
    }
}