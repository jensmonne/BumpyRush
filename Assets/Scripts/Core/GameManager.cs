using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [Tooltip("If 0 or less, all pickups in scene are required to finish.")]
    [SerializeField] private int pickupsToWin;

    [Header("Item Spawning")]
    [SerializeField] private PickUpBase pickupPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int itemsToSpawn = 10;
    [SerializeField] private bool spawnRandomly = false;
    //[SerializeField] private float spawnDelay = 0.1f;

    private List<NetworkIdentity> spawnedPickups = new();

    [SyncVar(hook = nameof(HandleTotalPickupsChanged))]
    private int totalPickups;

    [SyncVar(hook = nameof(HandleCollectedPickupsChanged))]
    private int collectedPickups;

    [SyncVar(hook = nameof(HandleMatchOverChanged))]
    private bool isMatchOver;

    [SyncVar(hook = nameof(HandleWinnerChanged))]
    private uint winnerNetId;

    public class ScoreMap : SyncDictionary<uint, int> { }
    public readonly ScoreMap playerScores = new();

    public static event Action<int, int> OnPickupProgressChanged;
    public static event Action<bool, uint> OnMatchStateChanged;
    public static event Action<int> OnItemsSpawned;

    public int TotalPickups => totalPickups;
    public int CollectedPickups => collectedPickups;
    public bool IsMatchOver => isMatchOver;
    public uint WinnerNetId => winnerNetId;
    public int SpawnedItemCount => spawnedPickups.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        playerScores.Clear();
        collectedPickups = 0;
        isMatchOver = false;
        winnerNetId = 0;
        spawnedPickups.Clear();

        RecalculatePickupTargets();
        SpawnAllItems();
    }

    public override void OnStopServer()
    {
        ClearSpawnedItems();
        playerScores.Clear();
        base.OnStopServer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [Server]
    private void RecalculatePickupTargets()
    {
        PickUpBase[] pickups = FindObjectsByType<PickUpBase>();
        totalPickups = pickups.Length;

        if (pickupsToWin <= 0 || pickupsToWin > totalPickups)
        {
            pickupsToWin = totalPickups;
        }
    }

    /// <summary>
    /// Spawns all items across the designated spawn points.
    /// Call this on the server to create and network all pickups.
    /// </summary>
    [Server]
    public void SpawnAllItems()
    {
        if (pickupPrefab == null)
        {
            Debug.LogError("GameManager: Pickup prefab not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("GameManager: No spawn points assigned!");
            return;
        }

        ClearSpawnedItems();

        if (spawnRandomly)
        {
            SpawnRandomItems();
        }
        else
        {
            SpawnItemsAtPoints();
        }

        totalPickups = spawnedPickups.Count;
        if (pickupsToWin <= 0 || pickupsToWin > totalPickups)
        {
            pickupsToWin = totalPickups;
        }

        OnItemsSpawned?.Invoke(spawnedPickups.Count);
    }

    /// <summary>
    /// Spawns items at specific spawn points in order.
    /// </summary>
    [Server]
    private void SpawnItemsAtPoints()
    {
        int itemsLeft = itemsToSpawn;
        int spawnPointIndex = 0;

        while (itemsLeft > 0)
        {
            Transform spawnPoint = spawnPoints[spawnPointIndex % spawnPoints.Length];
            SpawnItemAt(spawnPoint.position, spawnPoint.rotation);

            itemsLeft--;
            spawnPointIndex++;
        }
    }


    [Server]
    private void SpawnRandomItems()
    {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            Transform randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            SpawnItemAt(randomSpawnPoint.position, randomSpawnPoint.rotation);
        }
    }


    [Server]
    private void SpawnItemAt(Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(pickupPrefab.gameObject, position, rotation);

        if (spawnedObject.TryGetComponent<NetworkIdentity>(out var netIdentity))
        {
            spawnedPickups.Add(netIdentity);
            NetworkServer.Spawn(spawnedObject);
        }
        else
        {
            Debug.LogError("GameManager: Pickup prefab does not have a NetworkIdentity component!");
            Destroy(spawnedObject);
        }
    }


    [Server]
    public void ClearSpawnedItems()
    {
        foreach (var pickupIdentity in spawnedPickups)
        {
            if (pickupIdentity != null && pickupIdentity.gameObject != null)
            {
                NetworkServer.Destroy(pickupIdentity.gameObject);
            }
        }

        spawnedPickups.Clear();
    }

    /// <summary>
    /// Removes a specific pickup from the spawned items list.
    /// Called when a pickup is collected.
    /// </summary>
    [Server]
    public void UnregisterPickup(NetworkIdentity pickupIdentity)
    {
        spawnedPickups.Remove(pickupIdentity);
    }

    [Server]
    public void RegisterPickup(NetworkIdentity playerIdentity)
    {
        if (isMatchOver) return;
        if (totalPickups <= 0) return;

        collectedPickups = Mathf.Min(collectedPickups + 1, totalPickups);

        if (playerIdentity != null)
        {
            uint netId = playerIdentity.netId;
            if (!playerScores.TryGetValue(netId, out int score))
            {
                score = 0;
            }

            playerScores[netId] = score + 1;
        }

        if (ShouldFinishMatch())
        {
            EndMatch(GetHighestScoringPlayer());
        }
    }

    [Server]
    private bool ShouldFinishMatch()
    {
        if (pickupsToWin <= 0)
        {
            return collectedPickups >= totalPickups;
        }

        return collectedPickups >= pickupsToWin;
    }

    [Server]
    private uint GetHighestScoringPlayer()
    {
        uint bestPlayer = 0;
        int bestScore = int.MinValue;

        foreach (var scoreEntry in playerScores)
        {
            if (scoreEntry.Value > bestScore)
            {
                bestScore = scoreEntry.Value;
                bestPlayer = scoreEntry.Key;
            }
        }

        return bestPlayer;
    }

    [Server]
    private void EndMatch(uint winnerPlayerNetId)
    {
        isMatchOver = true;
        winnerNetId = winnerPlayerNetId;
        RpcAnnounceMatchResult(winnerPlayerNetId);
    }

    [ClientRpc]
    private void RpcAnnounceMatchResult(uint winnerPlayerNetId)
    {
        if (winnerPlayerNetId == 0)
        {
            Debug.Log("Match finished. No winner could be determined.");
            return;
        }

        Debug.Log($"Match finished. Winner is player netId {winnerPlayerNetId}.");
    }

    private void HandleTotalPickupsChanged(int oldValue, int newValue)
    {
        OnPickupProgressChanged?.Invoke(collectedPickups, newValue);
    }

    private void HandleCollectedPickupsChanged(int oldValue, int newValue)
    {
        OnPickupProgressChanged?.Invoke(newValue, totalPickups);
    }

    private void HandleMatchOverChanged(bool oldValue, bool newValue)
    {
        OnMatchStateChanged?.Invoke(newValue, winnerNetId);
    }

    private void HandleWinnerChanged(uint oldValue, uint newValue)
    {
        if (!isMatchOver) return;
        OnMatchStateChanged?.Invoke(isMatchOver, newValue);
    }
}