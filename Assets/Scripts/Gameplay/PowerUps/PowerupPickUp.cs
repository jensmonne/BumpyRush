using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PowerupPickUp : NetworkBehaviour
{
    [Header("Powerup Settings")]
    [Tooltip("Prefabs of the powerups to be given to the player.")]
    [SerializeField] private List<GameObject> itemsPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> itemPickUpPrefabs = new List<GameObject>();

    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 90f;
    [SerializeField] private Transform modelRoot;

    [SyncVar] private int selectedIndex;
    private GameObject spawnedItem;

    public override void OnStartServer()
    {
        base.OnStartServer();
        selectedIndex = Random.Range(0, itemsPrefabs.Count);
        spawnedItem = Instantiate(itemPickUpPrefabs[selectedIndex], transform.position, Quaternion.identity, transform);
    }

    private void Update()
    {
        if (modelRoot != null)
            spawnedItem.transform.Rotate(Vector3.down, spinSpeed * Time.deltaTime, Space.World);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ItemManager itemManager = other.GetComponentInParent<ItemManager>();
        if (itemManager == null) return;

        itemManager.AddItem(itemsPrefabs[selectedIndex]);
        NetworkServer.Destroy(gameObject);
    }
}
