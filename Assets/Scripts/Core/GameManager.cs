using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int scoreToWin = 10;
    [SerializeField] private float matchDuration = 180f;
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private float returnToLobbyDelay = 3f;

    private float serverTimeRemaining;
    private float serverReturnTimer = -1f;

    [SyncVar(hook = nameof(HandleMatchOverChanged))] private bool isMatchOver;
    [SyncVar(hook = nameof(HandleWinnerChanged))] private uint winnerNetId;
    [SyncVar(hook = nameof(HandleMatchTiedChanged))] private bool isMatchTied;
    [SyncVar(hook = nameof(HandleTimerChanged))] private int timeRemainingSeconds;
    [SyncVar(hook = nameof(HandleReturnCountdownChanged))] private int returnCountdownSeconds;

    public class ScoreMap : SyncDictionary<uint, int> { }
    public readonly ScoreMap playerScores = new();

    public class NameMap : SyncDictionary<uint, string> { }
    public readonly NameMap playerNames = new();

    public static event Action<bool, uint, bool> OnMatchStateChanged;
    public static event Action<int> OnTimerChanged;
    public static event Action<int> OnReturnCountdownChanged;
    public static event Action OnScoresChanged;

    public bool IsMatchOver => isMatchOver;
    public uint WinnerNetId => winnerNetId;
    public bool IsMatchTied => isMatchTied;
    public int TimeRemainingSeconds => timeRemainingSeconds;
    public int ReturnCountdownSeconds => returnCountdownSeconds;

    public string GetPlayerName(uint netId) =>
        playerNames.TryGetValue(netId, out string name) && !string.IsNullOrEmpty(name)
            ? name : $"Player {netId}";

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
        playerNames.Clear();
        isMatchOver = false;
        isMatchTied = false;
        winnerNetId = 0;
        serverTimeRemaining = matchDuration;
        timeRemainingSeconds = Mathf.CeilToInt(matchDuration);
        serverReturnTimer = -1f;
        RegisterExistingPlayers();
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
        }
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
        if (isMatchOver || playerIdentity == null) return;
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
    private void CheckScoreWin(uint netId, int score)
    {
        if (isMatchOver) return;
        if (scoreToWin > 0 && score >= scoreToWin)
            EndMatch(netId, false);
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
    }

    private void HandleMatchOverChanged(bool _, bool v) => OnMatchStateChanged?.Invoke(v, winnerNetId, isMatchTied);
    private void HandleWinnerChanged(uint _, uint v) { if (isMatchOver) OnMatchStateChanged?.Invoke(isMatchOver, v, isMatchTied); }
    private void HandleMatchTiedChanged(bool _, bool v) { if (isMatchOver) OnMatchStateChanged?.Invoke(isMatchOver, winnerNetId, v); }
    private void HandleTimerChanged(int _, int v) => OnTimerChanged?.Invoke(v);
    private void HandleReturnCountdownChanged(int _, int v) => OnReturnCountdownChanged?.Invoke(v);
}