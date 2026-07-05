using Mirror;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemToSpawn;
    [SerializeField] private float respawnDelay = 10f;
    private GameObject spawnedItem;
    private float timer;

    private void Start()
    {
        if (NetworkServer.active)
            Spawn();
    }

    private void Update()
    {
        if (!NetworkServer.active || spawnedItem != null) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Spawn();
    }

    private void Spawn()
    {
        spawnedItem = Instantiate(itemToSpawn, transform.position, transform.rotation);
        NetworkServer.Spawn(spawnedItem);
        timer = respawnDelay;
    }
}