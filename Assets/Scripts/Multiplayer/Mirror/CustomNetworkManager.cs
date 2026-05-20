using UnityEngine;
using Mirror;
using Utp;
using UnityEditor;

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
    [SerializeField] private GameObject lobbyPlayerPrefab;

    public override void Awake()
    {
        base.Awake();
        utpTransport = GetComponent<UtpTransport>();
    }

    public void StartRelayHost(int maxPlayers, string regionId = null)
    {
        utpTransport.useRelay = true;

#if UNITY_WEBGL
    if (string.IsNullOrEmpty(regionId))
    {
        regionId = "europe-west4"; 
        Debug.Log($"WebGL detected: Defaulting to region {regionId} to bypass QoS.");
    }
#endif

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

    public void JoinRelayGame(string joinCode)
    {
        utpTransport.useRelay = true;

        utpTransport.ConfigureClientWithJoinCode(joinCode,
        () =>
        {
            Debug.Log("Relay Join Success. Connecting Mirror...");
            StartClient();
        },
        () => Debug.LogError("Failed to join Relay server."));
    }

    public void StartLocalGame()
    {
        utpTransport.useRelay = false;
        StartHost();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log("Called");

        base.OnServerSceneChanged(sceneName);

        // TODO: Change to handle changing back to lobbyplayer.
        if (sceneName == "MainMenu" || sceneName == "LobbyScene") return;

        Debug.Log("Scene changed, replacing lobby players with game players...");

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn == null || conn.identity == null) continue;

            if (conn.identity.TryGetComponent<LobbyPlayer>(out LobbyPlayer lobbyPlayer))
            {
                string retainedName = lobbyPlayer.PlayerName;

                Transform spawnPoint = GetStartPosition();
                GameObject gamePlayerInstance = spawnPoint != null 
                    ? Instantiate(gamePlayerPrefab, spawnPoint.position, spawnPoint.rotation)
                    : Instantiate(gamePlayerPrefab);

                if (gamePlayerInstance.TryGetComponent<GamePlayer>(out GamePlayer gameScript))
                {
                    gameScript.PlayerName = retainedName;
                }

                ReplacePlayerOptions options = new();

                // Swap authority and wipe old LobbyPlayer
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance, options);
            }
        }
    }
}