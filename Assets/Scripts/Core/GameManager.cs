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
    [SerializeField] private int pickupsToWin = 10;

    [Header("Item Spawning")]
    [SerializeField] private PickUpBase pickupPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int itemsToSpawn = 10;
    [SerializeField] private float respawnInterval = 10f;

    private List<NetworkIdentity> spawnedPickups = new();
    private float respawnTimer = 0f;

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
        respawnTimer = respawnInterval;

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

    private void Update()
    {
        if (!isServer || isMatchOver)
            return;

        respawnTimer -= Time.deltaTime;

        if (respawnTimer <= 0f)
        {
            respawnTimer = respawnInterval;
            CheckAndRespawnItems();
        }
    }

    [Server]
    private void CheckAndRespawnItems()
    {
        // Count living items (filter out null references)
        int livingItemCount = 0;
        spawnedPickups.RemoveAll(identity => identity == null || identity.gameObject == null);
        livingItemCount = spawnedPickups.Count;

        // Spawn new items if below target
        int itemsNeeded = itemsToSpawn - livingItemCount;
        if (itemsNeeded > 0)
        {
            for (int i = 0; i < itemsNeeded; i++)
            {
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                SpawnItemAt(spawnPoint.position, spawnPoint.rotation);
            }

            Debug.Log($"Respawned {itemsNeeded} items. Total on map: {spawnedPickups.Count}");
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

    [Server]

    public void ChangeScore(uint playerNetId, int scoreDelta)
    {
        if (!playerScores.TryGetValue(playerNetId, out int currentScore))
        {
            currentScore = 0;
        }
        playerScores[playerNetId] = currentScore + scoreDelta;
    }


    [Server]
    public void SpawnAllItems()
    {
        if (pickupPrefab == null)
        {
            Debug.LogError("GameManager: Pickup prefab not assigned!");
            return;
        }

        ClearSpawnedItems();
        SpawnItemsAtPoints();


        totalPickups = spawnedPickups.Count;
        if (pickupsToWin <= 0 || pickupsToWin > totalPickups)
        {
            pickupsToWin = totalPickups;
        }

        OnItemsSpawned?.Invoke(spawnedPickups.Count);
    }

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