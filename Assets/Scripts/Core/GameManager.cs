using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [Tooltip("First player to reach this score wins. If 0 or less, only the timer ends the match.")]
    [SerializeField] private int scoreToWin = 10;

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

    [Header("Spawn Tuning")]
    [Tooltip("A spawn point counts as occupied if a pickup is within this distance.")]
    [SerializeField] private float minSpawnDistance = 2f;

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

    public class NameMap : SyncDictionary<uint, string> { }
    public readonly NameMap playerNames = new();

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

    public string GetPlayerName(uint netId) =>
        playerNames.TryGetValue(netId, out string name) && !string.IsNullOrEmpty(name)
            ? name
            : $"Player {netId}";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [Server]
    private void RegisterExistingPlayers()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn?.identity == null) continue;
            if (conn.identity.TryGetComponent(out PlayerNameSync sync))
                RegisterPlayerName(conn.identity.netId, sync.steamName);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        RegisterExistingPlayers();


        playerScores.Clear();
        playerNames.Clear();
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

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerScores.OnChange += HandleScoreOp;
        playerNames.OnChange += HandleNameOp;
        OnScoresChanged?.Invoke();
    }

    public override void OnStopClient()
    {
        playerScores.OnChange -= HandleScoreOp;
        playerNames.OnChange -= HandleNameOp;
        base.OnStopClient();
    }

    public override void OnStopServer()
    {
        ClearSpawnedItems();
        ClearSpawnedPowerups();
        playerScores.Clear();
        playerNames.Clear();
        base.OnStopServer();
    }

    private void HandleScoreOp(ScoreMap.Operation op, uint key, int value) => OnScoresChanged?.Invoke();
    private void HandleNameOp(NameMap.Operation op, uint key, string value) => OnScoresChanged?.Invoke();

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
            RespawnMissing(bearPrefab != null ? bearPrefab.gameObject : null, bearSpawnpoints, bearsToSpawn, spawnedPickups);
        }

        powerupRespawnTimer -= Time.deltaTime;
        if (powerupRespawnTimer <= 0f)
        {
            powerupRespawnTimer = powerupRespawnInterval;
            RespawnMissing(powerupPrefab != null ? powerupPrefab.gameObject : null, powerupSpawnpoints, powerupsToSpawn, spawnedPowerups);
        }
    }

    [Server]
    public void SpawnAllItems()
    {
        if (bearPrefab == null) { Debug.LogError("GameManager: Bear prefab not assigned!"); return; }
        if (bearSpawnpoints == null || bearSpawnpoints.Length == 0) { Debug.LogError("GameManager: No bear spawnpoints assigned!"); return; }

        ClearSpawnedItems();
        SpawnWave(bearPrefab.gameObject, bearSpawnpoints, bearsToSpawn, spawnedPickups);
        totalPickups = spawnedPickups.Count;
    }

    [Server]
    public void SpawnAllPowerups()
    {
        if (powerupPrefab == null) { Debug.LogError("GameManager: Powerup prefab not assigned!"); return; }
        if (powerupSpawnpoints == null || powerupSpawnpoints.Length == 0) { Debug.LogError("GameManager: No powerup spawnpoints assigned!"); return; }

        ClearSpawnedPowerups();
        SpawnWave(powerupPrefab.gameObject, powerupSpawnpoints, powerupsToSpawn, spawnedPowerups);
    }

    [Server]
    private void SpawnWave(GameObject prefab, Transform[] points, int count, List<NetworkIdentity> tracker)
    {
        if (prefab == null || points == null || points.Length == 0) return;
        if (count > points.Length)
            Debug.LogWarning($"GameManager: spawning {count} into {points.Length} points; some will share.");

        List<int> order = ShuffledIndices(points.Length);
        for (int i = 0; i < count; i++)
        {
            Transform p = points[order[i % order.Count]];
            SpawnOne(prefab, p.position, p.rotation, tracker);
        }
    }

    [Server]
    private void RespawnMissing(GameObject prefab, Transform[] points, int target, List<NetworkIdentity> tracker)
    {
        if (prefab == null || points == null || points.Length == 0) return;

        tracker.RemoveAll(id => id == null || id.gameObject == null);
        int needed = target - tracker.Count;
        for (int i = 0; i < needed; i++)
        {
            Transform p = PickFreePoint(points, tracker);
            if (p == null) break;
            SpawnOne(prefab, p.position, p.rotation, tracker);
        }
    }

    [Server]
    private Transform PickFreePoint(Transform[] points, List<NetworkIdentity> tracker)
    {
        List<int> order = ShuffledIndices(points.Length);
        float sqr = minSpawnDistance * minSpawnDistance;

        foreach (int idx in order)
        {
            Transform p = points[idx];
            bool occupied = false;
            foreach (var id in tracker)
            {
                if (id == null) continue;
                if ((id.transform.position - p.position).sqrMagnitude < sqr) { occupied = true; break; }
            }
            if (!occupied) return p;
        }
        return points[order[0]];
    }

    [Server]
    private void SpawnOne(GameObject prefab, Vector3 position, Quaternion rotation, List<NetworkIdentity> tracker)
    {
        GameObject obj = Instantiate(prefab, position, rotation);
        if (obj.TryGetComponent(out NetworkIdentity id))
        {
            tracker.Add(id);
            NetworkServer.Spawn(obj);
        }
        else Destroy(obj);
    }

    private List<int> ShuffledIndices(int n)
    {
        List<int> list = new(n);
        for (int i = 0; i < n; i++) list.Add(i);
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
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
    public void RegisterPlayerName(uint netId, string name)
    {
        playerNames[netId] = string.IsNullOrWhiteSpace(name) ? $"Player {netId}" : name;
        if (!playerScores.ContainsKey(netId))
            playerScores[netId] = 0;
    }

    [Server]
    public void RegisterPickup(NetworkIdentity playerIdentity)
    {
        if (isMatchOver) return;

        collectedPickups++;
        if (playerIdentity == null) return;

        uint netId = playerIdentity.netId;
        int newScore = (playerScores.TryGetValue(netId, out int score) ? score : 0) + 1;
        playerScores[netId] = newScore;

        CheckScoreWin(netId, newScore);
    }

    [Server]
    public void ChangeScore(uint playerNetId, int scoreDelta)
    {
        int newScore = (playerScores.TryGetValue(playerNetId, out int current) ? current : 0) + scoreDelta;
        playerScores[playerNetId] = newScore;
        CheckScoreWin(playerNetId, newScore);
    }

    [Server]
    private void CheckScoreWin(uint netId, int score)
    {
        if (isMatchOver) return;
        if (scoreToWin > 0 && score >= scoreToWin)
            EndMatch(netId, false);
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
        return highestOpponent == int.MinValue ? playerScore : playerScore - highestOpponent;
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
        Debug.Log(tied ? "Match ended in a tie!" : $"Match finished. Winner: {GetPlayerName(winnerPlayerNetId)}.");
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