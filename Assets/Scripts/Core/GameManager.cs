using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [Tooltip("If 0 or less, the match only ends when the timer runs out.")]
    [SerializeField] private int pickupsToWin = 0;

    [Header("Match Timer")]
    [SerializeField] private float matchDuration = 180f;

    [Header("Post Match")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private float returnToLobbyDelay = 3f;

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
    private float respawnTimer;
    private float powerupRespawnTimer;
    private float serverTimeRemaining;
    private float serverReturnTimer = -1f;

    [SyncVar(hook = nameof(HandleTotalPickupsChanged))]
    private int totalPickups;

    [SyncVar(hook = nameof(HandleCollectedPickupsChanged))]
    private int collectedPickups;

    [SyncVar(hook = nameof(HandleMatchOverChanged))]
    private bool isMatchOver;

    [SyncVar(hook = nameof(HandleWinnerChanged))]
    private uint winnerNetId;

    [SyncVar(hook = nameof(HandleMatchTiedChanged))]
    private bool isMatchTied;

    [SyncVar(hook = nameof(HandleTimerChanged))]
    private int timeRemainingSeconds;

    [SyncVar(hook = nameof(HandleReturnCountdownChanged))]
    private int returnCountdownSeconds;

    public class ScoreMap : SyncDictionary<uint, int> { }
    public readonly ScoreMap playerScores = new();

    public static event Action<int, int> OnPickupProgressChanged;
    public static event Action<bool, uint, bool> OnMatchStateChanged;
    public static event Action<int> OnTimerChanged;
    public static event Action<int> OnReturnCountdownChanged;
    public static event Action OnScoresChanged;
    public int TotalPickups => totalPickups;
    public int CollectedPickups => collectedPickups;
    public bool IsMatchOver => isMatchOver;
    public uint WinnerNetId => winnerNetId;
    public bool IsMatchTied => isMatchTied;
    public int TimeRemainingSeconds => timeRemainingSeconds;
    public int ReturnCountdownSeconds => returnCountdownSeconds;
    public int SpawnedItemCount => spawnedPickups.Count;
    public int SpawnedPowerupCount => spawnedPowerups.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        playerScores.Clear();
        spawnedPickups.Clear();
        spawnedPowerups.Clear();

        collectedPickups = 0;
        isMatchOver = false;
        isMatchTied = false;
        winnerNetId = 0;
        serverTimeRemaining = matchDuration;
        timeRemainingSeconds = Mathf.CeilToInt(matchDuration);
        serverReturnTimer = -1f;
        respawnTimer = respawnInterval;
        powerupRespawnTimer = powerupRespawnInterval;

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

    private void Update()
    {
        if (!isServer) return;

        if (isMatchOver)
        {
            if (serverReturnTimer > 0f)
            {
                serverReturnTimer -= Time.deltaTime;

                int newSeconds = Mathf.CeilToInt(serverReturnTimer);
                if (newSeconds != returnCountdownSeconds)
                    returnCountdownSeconds = newSeconds;

                if (serverReturnTimer <= 0f)
                    NetworkManager.singleton.ServerChangeScene(lobbySceneName);
            }
            return;
        }

        serverTimeRemaining -= Time.deltaTime;

        int remaining = Mathf.CeilToInt(serverTimeRemaining);
        if (remaining != timeRemainingSeconds)
            timeRemainingSeconds = remaining;

        if (serverTimeRemaining <= 0f)
        {
            bool tied = IsTied();
            EndMatch(tied ? 0 : GetHighestScoringPlayer(), tied);
            return;
        }

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
    public void SpawnAllItems()
    {
        if (bearPrefab == null) { Debug.LogError("GameManager: Bear prefab not assigned!"); return; }
        ClearSpawnedItems();
        for (int i = 0; i < bearsToSpawn; i++)
            SpawnItemAt(bearSpawnpoints[i % bearSpawnpoints.Length].position,
                        bearSpawnpoints[i % bearSpawnpoints.Length].rotation);
        totalPickups = spawnedPickups.Count;
        if (pickupsToWin <= 0 || pickupsToWin > totalPickups)
            pickupsToWin = totalPickups;
    }

    [Server]
    public void SpawnAllPowerups()
    {
        if (powerupPrefab == null) { Debug.LogError("GameManager: Powerup prefab not assigned!"); return; }
        ClearSpawnedPowerups();
        for (int i = 0; i < powerupsToSpawn; i++)
            SpawnPowerupAt(powerupSpawnpoints[i % powerupSpawnpoints.Length].position,
                           powerupSpawnpoints[i % powerupSpawnpoints.Length].rotation);
    }

    [Server]
    private void CheckAndRespawnItems()
    {
        spawnedPickups.RemoveAll(id => id == null || id.gameObject == null);
        int needed = bearsToSpawn - spawnedPickups.Count;
        for (int i = 0; i < needed; i++)
        {
            Transform point = bearSpawnpoints[UnityEngine.Random.Range(0, bearSpawnpoints.Length)];
            SpawnItemAt(point.position, point.rotation);
        }
    }

    [Server]
    private void CheckAndRespawnPowerups()
    {
        spawnedPowerups.RemoveAll(id => id == null || id.gameObject == null);
        int needed = powerupsToSpawn - spawnedPowerups.Count;
        for (int i = 0; i < needed; i++)
        {
            Transform point = powerupSpawnpoints[UnityEngine.Random.Range(0, powerupSpawnpoints.Length)];
            SpawnPowerupAt(point.position, point.rotation);
        }
    }

    [Server]
    private void SpawnItemAt(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(bearPrefab.gameObject, position, rotation);
        if (obj.TryGetComponent<NetworkIdentity>(out var id)) { spawnedPickups.Add(id); NetworkServer.Spawn(obj); }
        else Destroy(obj);
    }

    [Server]
    private void SpawnPowerupAt(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(powerupPrefab.gameObject, position, rotation);
        if (obj.TryGetComponent<NetworkIdentity>(out var id)) { spawnedPowerups.Add(id); NetworkServer.Spawn(obj); }
        else Destroy(obj);
    }

    [Server]
    public void ClearSpawnedItems()
    {
        foreach (var id in spawnedPickups)
            if (id != null && id.gameObject != null) NetworkServer.Destroy(id.gameObject);
        spawnedPickups.Clear();
    }

    [Server]
    public void ClearSpawnedPowerups()
    {
        foreach (var id in spawnedPowerups)
            if (id != null && id.gameObject != null) NetworkServer.Destroy(id.gameObject);
        spawnedPowerups.Clear();
    }

    [Server] public void UnregisterPickup(NetworkIdentity id) => spawnedPickups.Remove(id);
    [Server] public void UnregisterPowerup(NetworkIdentity id) => spawnedPowerups.Remove(id);


    [Server]
    public void RegisterPickup(NetworkIdentity playerIdentity)
    {
        if (isMatchOver || totalPickups <= 0) return;

        collectedPickups = Mathf.Min(collectedPickups + 1, totalPickups);

        if (playerIdentity != null)
        {
            uint netId = playerIdentity.netId;
            playerScores[netId] = (playerScores.TryGetValue(netId, out int score) ? score : 0) + 1;
            OnScoresChanged?.Invoke();
        }

        if (pickupsToWin > 0 && collectedPickups >= pickupsToWin)
            EndMatch(GetHighestScoringPlayer(), false);
    }

    [Server]
    public void ChangeScore(uint playerNetId, int scoreDelta)
    {
        playerScores[playerNetId] = (playerScores.TryGetValue(playerNetId, out int current) ? current : 0) + scoreDelta;
        OnScoresChanged?.Invoke();
    }


    [Server]
    public int GetScoreDifference(NetworkIdentity playerIdentity)
    {
        if (playerIdentity == null) return 0;
        uint playerNetId = playerIdentity.netId;
        int playerScore = playerScores.TryGetValue(playerNetId, out int score) ? score : 0;
        int highestOpponent = int.MinValue;
        foreach (var entry in playerScores)
            if (entry.Key != playerNetId && entry.Value > highestOpponent)
                highestOpponent = entry.Value;
        return playerScore - highestOpponent;
    }


    [Server]
    private bool IsTied()
    {
        if (playerScores.Count < 2) return false;
        int highest = int.MinValue;
        foreach (var s in playerScores.Values)
            if (s > highest) highest = s;
        int count = 0;
        foreach (var s in playerScores.Values)
            if (s == highest) count++;
        return count > 1;
    }

    [Server]
    private uint GetHighestScoringPlayer()
    {
        uint best = 0;
        int bestScore = int.MinValue;
        foreach (var entry in playerScores)
            if (entry.Value > bestScore) { bestScore = entry.Value; best = entry.Key; }
        return best;
    }

    [Server]
    private void EndMatch(uint winnerPlayerNetId, bool tied)
    {
        isMatchOver = true;
        isMatchTied = tied;
        winnerNetId = winnerPlayerNetId;
        serverReturnTimer = returnToLobbyDelay;
        returnCountdownSeconds = Mathf.CeilToInt(returnToLobbyDelay);
        RpcAnnounceMatchResult(winnerPlayerNetId, tied);
    }

    [ClientRpc]
    private void RpcAnnounceMatchResult(uint winnerPlayerNetId, bool tied)
    {
        Debug.Log(tied ? "Match ended in a tie!" : $"Match finished. Winner is player netId {winnerPlayerNetId}.");
    }


    private void HandleTotalPickupsChanged(int _, int newValue) =>
        OnPickupProgressChanged?.Invoke(collectedPickups, newValue);

    private void HandleCollectedPickupsChanged(int _, int newValue) =>
        OnPickupProgressChanged?.Invoke(newValue, totalPickups);

    private void HandleMatchOverChanged(bool _, bool newValue) =>
        OnMatchStateChanged?.Invoke(newValue, winnerNetId, isMatchTied);

    private void HandleWinnerChanged(uint _, uint newValue)
    {
        if (!isMatchOver) return;
        OnMatchStateChanged?.Invoke(isMatchOver, newValue, isMatchTied);
    }

    private void HandleMatchTiedChanged(bool _, bool newValue)
    {
        if (!isMatchOver) return;
        OnMatchStateChanged?.Invoke(isMatchOver, winnerNetId, newValue);
    }

    private void HandleTimerChanged(int _, int newValue) =>
        OnTimerChanged?.Invoke(newValue);

    private void HandleReturnCountdownChanged(int _, int newValue) =>
        OnReturnCountdownChanged?.Invoke(newValue);
}