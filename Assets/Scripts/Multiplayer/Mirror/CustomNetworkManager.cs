using UnityEngine;
using Mirror;
using Utp;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class CustomNetworkManager : NetworkManager
{
    public static new CustomNetworkManager singleton => (CustomNetworkManager)NetworkManager.singleton;

    private UtpTransport utpTransport;

    public string relayJoinCode { get; private set; }

    [Header("Player Prefabs")]
    [SerializeField] private GameObject gamePlayerPrefab;

    public override void Awake()
    {
        base.Awake();
        utpTransport = GetComponent<UtpTransport>();
    }

    public void StartRelayHost(int maxPlayers, string regionId = null)
    {
        utpTransport.useRelay = true;

        utpTransport.AllocateRelayServer(maxPlayers, regionId,
        (joinCode) =>
        {
            relayJoinCode = joinCode;
            Debug.LogError($"Relay JoinCode: {joinCode}");

            StartHost();
        },
        () => 
        {
            Debug.LogError("Failed to start Relay server.");
            MenuManager.Instance.SetLoadingStatusText("Failed to start Relay server. Please try again.");
            MenuManager.Instance.OpenMenu("MainMenu");
        });
    }

    public void JoinRelayGame(string joinCode, Action onFailure = null)
    {
        utpTransport.useRelay = true;

        utpTransport.ConfigureClientWithJoinCode(joinCode,
        () =>
        {
            Debug.Log("Relay Join Success. Connecting Mirror...");
            StartClient();
        },
        () => 
        {
            Debug.LogError("Failed to join Relay server.");
            onFailure?.Invoke();
        });
    }

    public void StartLocalGame()
    {
        utpTransport.useRelay = false;
        StartHost();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == "Lobby")
        {
            GameObject lobbyPlayerInstance = Instantiate(playerPrefab);
            lobbyPlayerInstance.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance);
        }
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene.Contains("MainMenu") || currentScene.Contains("Assets/Scenes/Lobby.unity")) return;

        if (conn.identity != null && conn.identity.TryGetComponent(out LobbyPlayer lobbyPlayer))
        {
            Debug.Log($"Client {conn.connectionId} is ready. Swapping {lobbyPlayer.PlayerName} to gameplay prefab...");
            string retainedName = lobbyPlayer.PlayerName;

            Transform spawnPoint = GetStartPosition();
            GameObject gamePlayerInstance = spawnPoint != null 
                ? Instantiate(gamePlayerPrefab, spawnPoint.position, spawnPoint.rotation)
                : Instantiate(gamePlayerPrefab);

            if (gamePlayerInstance.TryGetComponent(out GamePlayer gameScript))
            {
                gameScript.PlayerName = retainedName;
            }
            
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance, ReplacePlayerOptions.Destroy);
        }
    }

    public override void OnClientConnect()
    {
        if (!clientLoadedScene)
        {
            if (!NetworkClient.ready) NetworkClient.Ready();

            NetworkClient.AddPlayer();
        }
    }

    public override void OnClientSceneChanged()
    {
        if (NetworkClient.connection.isAuthenticated && !NetworkClient.ready) NetworkClient.Ready();

        if (NetworkClient.connection.isAuthenticated && NetworkClient.localPlayer == null)
        {
            NetworkClient.AddPlayer();
        }
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            Debug.Log("Stopping Host and disconnecting all clients...");
            StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            Debug.Log("Stopping Client and disconnecting from server...");
            StopClient();
        }
    }
}