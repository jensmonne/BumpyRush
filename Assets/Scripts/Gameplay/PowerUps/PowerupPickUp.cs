using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PowerupPickUp : NetworkBehaviour
{
    [Header("Powerup Settings")]
    [Tooltip("Prefabs of the powerups to be given to the player. Must be ordered in Inspector: Punch, Hammer, Disruptor, Oil Spill, Big, Coin Flip, Vacuum, Thief")]
    [SerializeField] private List<GameObject> itemsPrefabs = new List<GameObject>();
    private ItemManager itemManager;

    private static readonly float[,] weights = new float[,]
    {
        // diff:  0      1      2      3      4      5      6      7      8      9
        /* Punch     */ { 0.150f, 0.167f, 0.162f, 0.157f, 0.151f, 0.144f, 0.137f, 0.129f, 0.121f, 0.111f },
        /* Hammer    */ { 0.150f, 0.167f, 0.162f, 0.157f, 0.151f, 0.144f, 0.137f, 0.129f, 0.121f, 0.111f },
        /* Disruptor */ { 0.150f, 0.167f, 0.162f, 0.157f, 0.151f, 0.144f, 0.137f, 0.129f, 0.121f, 0.111f },
        /* Oil Spill */ { 0.150f, 0.167f, 0.162f, 0.157f, 0.151f, 0.144f, 0.137f, 0.129f, 0.121f, 0.111f },
        /* Big       */ { 0.100f, 0.111f, 0.108f, 0.105f, 0.101f, 0.096f, 0.092f, 0.086f, 0.080f, 0.074f },
        /* Coin Flip */ { 0.070f, 0.074f, 0.081f, 0.089f, 0.098f, 0.109f, 0.120f, 0.132f, 0.146f, 0.160f },
        /* Vacuum    */ { 0.070f, 0.074f, 0.081f, 0.089f, 0.098f, 0.109f, 0.120f, 0.132f, 0.146f, 0.160f },
        /* Thief     */ { 0.070f, 0.074f, 0.081f, 0.089f, 0.098f, 0.109f, 0.120f, 0.132f, 0.146f, 0.160f },
    };

    private int number;


    public void GetItem(GameObject player)
    {
        int scoreDifference = GameManager.Instance.GetScoreDifference(player.GetComponent<NetworkIdentity>());
        int diff = Mathf.Clamp(scoreDifference, 0, 9);
        number = GetWeightedRandom(diff);
        Debug.Log($"Score Difference: {scoreDifference}, Diff: {diff}, Item Index: {number}");
        itemManager.AddItem(itemsPrefabs[number]);

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

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            itemManager = other.GetComponentInParent<ItemManager>();
            if (itemManager != null)
            {
                GetItem(other.gameObject);
                PowerUpUI.SetPowerup(itemsPrefabs[number].name);
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}