using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance { get; private set; }

    public string LobbyJoinCode { get; private set; }
    public Lobby? CurrentLobby { get; private set; }

    private const string GameFilterKey = "BumpyRushV1";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SteamFriends.OnGameLobbyJoinRequested += OnSteamInviteReceived;
    }

    private void OnDestroy()
    {
        SteamFriends.OnGameLobbyJoinRequested -= OnSteamInviteReceived;
    }

    public async void CreateLobby(int maxPlayers, HostMenuManager.LobbyVisibility visibility, Action onFailure)
    {
        try
        {
            Lobby? lobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if (!lobbyOutput.HasValue)
            {
                onFailure?.Invoke();
                return;
            }

            Lobby lobby = lobbyOutput.Value;

            switch (visibility)
            {
                case HostMenuManager.LobbyVisibility.Public: lobby.SetPublic(); break;
                case HostMenuManager.LobbyVisibility.FriendsOnly: lobby.SetFriendsOnly(); break;
                case HostMenuManager.LobbyVisibility.Private: lobby.SetPrivate(); break;
            }

            lobby.SetJoinable(true);
            lobby.SetData("HostSteamID", SteamClient.SteamId.ToString());
            lobby.SetData("GameFilterKey", GameFilterKey);

            CurrentLobby = lobby;
            LobbyJoinCode = lobby.Id.ToString();

            GUIUtility.systemCopyBuffer = LobbyJoinCode;
            Debug.Log($"[Steam Lobbies] Lobby Created: {LobbyJoinCode} (Copied to Clipboard)");

            if (Mirror.NetworkServer.active || Mirror.NetworkClient.active)
            {
                Debug.LogWarning("[Steam Lobbies] Clean up active ghost networks before starting a new one...");
                CustomNetworkManager.singleton.LeaveGame();
            }

            CustomNetworkManager.singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Steam Lobbies] Error creating lobby: {e.Message}");
            onFailure?.Invoke();
        }
    }

    public void JoinLobbyByIdString(string lobbyIdText, Action onFailure)
    {
        if (!ulong.TryParse(lobbyIdText, out ulong parsedLobbyId))
        {
            onFailure?.Invoke();
            return;
        }

        JoinLobbyDirectly(new Lobby(parsedLobbyId), onFailure);
    }

    public async void JoinLobbyDirectly(Lobby targetLobby, Action onFailure)
    {
        RoomEnter result = await targetLobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogError($"[Steam Lobbies] Failed to join lobby data structure: {result}");
            onFailure?.Invoke();
            return;
        }

        string hostSteamID = targetLobby.GetData("HostSteamID");
        if (string.IsNullOrEmpty(hostSteamID))
        {
            Debug.LogError("[Steam Lobbies] Lobby found, but missing HostSteamID metadata!");
            onFailure?.Invoke();
            return;
        }

        CurrentLobby = targetLobby;
        
        CustomNetworkManager.singleton.networkAddress = hostSteamID;
        CustomNetworkManager.singleton.StartClient();
    }

    public void OnSteamInviteReceived(Lobby lobby, SteamId friendId)
    {
        MenuManager.Instance.OpenMenu("LoadingMenu");
        MenuManager.Instance.SetLoadingStatusText("Joining friend via Steam...");
        JoinLobbyDirectly(lobby, () => MenuManager.Instance.OpenMenu("MainMenu"));
    }

    public void LeaveLobby()
    {
        if (CurrentLobby.HasValue)
        {
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
            LobbyJoinCode = string.Empty;
        }
    }
}