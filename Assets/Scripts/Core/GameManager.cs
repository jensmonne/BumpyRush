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

    [Header("Bear Spawning")]
    [SerializeField] private PickUpBase bearPrefab;
    [SerializeField] private Transform[] bearSpawnpoints;
    [SerializeField] private int bearsToSpawn = 10;
    [SerializeField] private float respawnInterval = 10f;

    [Header("Powerup Spawning")]
    [SerializeField] private PowerupPickUp powerupPrefab;
    [SerializeField] private Transform[] powerupSpawnpoints;
    [SerializeField] private int powerupsToSpawn = 5;
    [SerializeField] private float powerupRespawnInterval = 15f;

    private List<NetworkIdentity> spawnedPickups = new();
    private List<NetworkIdentity> spawnedPowerups = new();
    private float respawnTimer = 0f;
    private float powerupRespawnTimer = 0f;

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
    public static event Action<int> OnPowerupsSpawned;

    public int TotalPickups => totalPickups;
    public int CollectedPickups => collectedPickups;
    public bool IsMatchOver => isMatchOver;
    public uint WinnerNetId => winnerNetId;
    public int SpawnedItemCount => spawnedPickups.Count;
    public int SpawnedPowerupCount => spawnedPowerups.Count;

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
        spawnedPowerups.Clear();
        respawnTimer = respawnInterval;
        powerupRespawnTimer = powerupRespawnInterval;

        RecalculatePickupTargets();
        SpawnAllItems();
        SpawnAllPowerups();
    }

    public override void OnStopServer()
    {
        ClearSpawnedItems();
        ClearSpawnedPowerups();
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

        powerupRespawnTimer -= Time.deltaTime;

        if (powerupRespawnTimer <= 0f)
        {
            powerupRespawnTimer = powerupRespawnInterval;
            CheckAndRespawnPowerups();
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
        int itemsNeeded = bearsToSpawn - livingItemCount;
        if (itemsNeeded > 0)
        {
            for (int i = 0; i < itemsNeeded; i++)
            {
                Transform spawnPoint = bearSpawnpoints[UnityEngine.Random.Range(0, bearSpawnpoints.Length)];
                SpawnItemAt(spawnPoint.position, spawnPoint.rotation);
            }

            Debug.Log($"Respawned {itemsNeeded} items. Total on map: {spawnedPickups.Count}");
        }
    }

    [Server]
    private void CheckAndRespawnPowerups()
    {
        // Count living powerups (filter out null references)
        spawnedPowerups.RemoveAll(identity => identity == null || identity.gameObject == null);
        int livingPowerupCount = spawnedPowerups.Count;

        // Spawn new powerups if below target
        int powerupsNeeded = powerupsToSpawn - livingPowerupCount;
        if (powerupsNeeded > 0)
        {
            for (int i = 0; i < powerupsNeeded; i++)
            {
                Transform spawnPoint = powerupSpawnpoints[UnityEngine.Random.Range(0, powerupSpawnpoints.Length)];
                SpawnPowerupAt(spawnPoint.position, spawnPoint.rotation);
            }

            Debug.Log($"Respawned {powerupsNeeded} powerups. Total on map: {spawnedPowerups.Count}");
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
        if (bearPrefab == null)
        {
            Debug.LogError("GameManager: Bear prefab not assigned!");
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
    public void SpawnAllPowerups()
    {
        if (powerupPrefab == null)
        {
            Debug.LogError("GameManager: Powerup prefab not assigned!");
            return;
        }

        ClearSpawnedPowerups();
        SpawnPowerupsAtPoints();

        OnPowerupsSpawned?.Invoke(spawnedPowerups.Count);
    }

    [Server]
    private void SpawnItemsAtPoints()
    {
        int itemsLeft = bearsToSpawn;
        int spawnPointIndex = 0;

        while (itemsLeft > 0)
        {
            Transform spawnPoint = bearSpawnpoints[spawnPointIndex % bearSpawnpoints.Length];
            SpawnItemAt(spawnPoint.position, spawnPoint.rotation);

            itemsLeft--;
            spawnPointIndex++;
        }
    }

    [Server]
    private void SpawnPowerupsAtPoints()
    {
        int powerupsLeft = powerupsToSpawn;
        int spawnPointIndex = 0;

        while (powerupsLeft > 0)
        {
            Transform spawnPoint = powerupSpawnpoints[spawnPointIndex % powerupSpawnpoints.Length];
            SpawnPowerupAt(spawnPoint.position, spawnPoint.rotation);

            powerupsLeft--;
            spawnPointIndex++;
        }
    }

    [Server]
    private void SpawnItemAt(Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(bearPrefab.gameObject, position, rotation);

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
    private void SpawnPowerupAt(Vector3 position, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(powerupPrefab.gameObject, position, rotation);

        if (spawnedObject.TryGetComponent<NetworkIdentity>(out var netIdentity))
        {
            spawnedPowerups.Add(netIdentity);
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
    public void ClearSpawnedPowerups()
    {
        foreach (var powerupIdentity in spawnedPowerups)
        {
            if (powerupIdentity != null && powerupIdentity.gameObject != null)
            {
                NetworkServer.Destroy(powerupIdentity.gameObject);
            }
        }

        spawnedPowerups.Clear();
    }

    [Server]
    public void UnregisterPickup(NetworkIdentity pickupIdentity)
    {
        spawnedPickups.Remove(pickupIdentity);
    }

    [Server]
    public void UnregisterPowerup(NetworkIdentity powerupIdentity)
    {
        spawnedPowerups.Remove(powerupIdentity);
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

    public int GetScoreDifference(NetworkIdentity playerIdentity)
    {
        if (playerIdentity == null) return 0;
        uint playerNetId = playerIdentity.netId;
        int playerScore = playerScores.TryGetValue(playerNetId, out int score) ? score : 0;
        int highestScore = int.MinValue;
        foreach (var scoreEntry in playerScores)
        {
            if (scoreEntry.Key != playerNetId && scoreEntry.Value > highestScore)
            {
                highestScore = scoreEntry.Value;
            }
        }
        int scoreDifference = playerScore - highestScore;
        Debug.Log($"Player {playerNetId} score: {playerScore}, highest opponent score: {highestScore}, difference: {scoreDifference}");
        return scoreDifference;
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