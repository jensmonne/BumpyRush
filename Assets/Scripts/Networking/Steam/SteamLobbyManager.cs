using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance { get; private set; }

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
            if (Mirror.NetworkServer.active || Mirror.NetworkClient.active)
            {
                Debug.LogWarning("[Steam Lobby] Clean up active ghost networks before starting a new host...");
                CustomNetworkManager.singleton.LeaveGame();

                await System.Threading.Tasks.Task.Delay(500);
            }

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

            Debug.Log($"[Steam Lobby] Lobby Created Successfully with Steam ID: {lobby.Id}");

            CustomNetworkManager.singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Steam Lobby] Error creating lobby: {e.Message}");
            onFailure?.Invoke();
        }
    }

    public async void JoinLobbyDirectly(Lobby targetLobby, Action onFailure)
    {
        if (Mirror.NetworkClient.active)
        {
            CustomNetworkManager.singleton.LeaveGame();
            await System.Threading.Tasks.Task.Delay(500);
        }
        
        RoomEnter result = await targetLobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogError($"[Steam Lobby] Failed to join lobby data structure: {result}");
            onFailure?.Invoke();
            return;
        }

        string hostSteamID = targetLobby.GetData("HostSteamID");
        if (string.IsNullOrEmpty(hostSteamID))
        {
            Debug.LogError("[Steam Lobby] Lobby found, but missing HostSteamID metadata!");
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

    public void OpenSteamInviteOverlay()
    {
        SteamFriends.OpenOverlay("LobbyInvite");
    }

    public void LeaveLobby()
    {
        if (CurrentLobby.HasValue)
        {
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
            Debug.Log("[Steam Lobbies] Left Steam Lobby.");
        }
    }
}