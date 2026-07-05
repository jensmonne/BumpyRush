using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PowerupPickUp : NetworkBehaviour
{
    [SerializeField] private List<GameObject> itemsPrefabs = new List<GameObject>();

    private bool collected;

    private static readonly float[,] weights = new float[,]
    {
        { 0.217f, 0.220f, 0.214f, 0.208f, 0.201f, 0.193f, 0.184f, 0.175f, 0.165f, 0.153f },
        { 0.217f, 0.220f, 0.214f, 0.208f, 0.201f, 0.193f, 0.184f, 0.175f, 0.165f, 0.153f },
        { 0.217f, 0.220f, 0.214f, 0.208f, 0.201f, 0.193f, 0.184f, 0.175f, 0.165f, 0.153f },
        { 0.145f, 0.146f, 0.143f, 0.139f, 0.135f, 0.129f, 0.124f, 0.117f, 0.109f, 0.102f },
        { 0.101f, 0.097f, 0.107f, 0.118f, 0.131f, 0.146f, 0.162f, 0.179f, 0.199f, 0.220f },
        { 0.101f, 0.097f, 0.107f, 0.118f, 0.131f, 0.146f, 0.162f, 0.179f, 0.199f, 0.220f },
    };

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;

        ItemManager itemManager = other.GetComponentInParent<ItemManager>();
        if (itemManager == null) return;

        collected = true;

        int scoreDiff = GameManager.Instance.GetScoreDifference(other.GetComponentInParent<NetworkIdentity>());
        int diff = Mathf.Clamp(scoreDiff, 0, 9);
        int index = GetWeightedRandom(diff);

        itemManager.AddItem(itemsPrefabs[index]);
        NetworkServer.Destroy(gameObject);
    }

    private int GetWeightedRandom(int diff)
    {
        float roll = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < itemsPrefabs.Count; i++)
        {
            cumulative += weights[i, diff];
            if (roll < cumulative)
                return i;
        }
        return itemsPrefabs.Count - 1;
    }
}