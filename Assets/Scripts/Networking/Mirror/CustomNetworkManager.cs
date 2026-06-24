using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class CustomNetworkManager : NetworkManager
{
    public static new CustomNetworkManager singleton => (CustomNetworkManager)NetworkManager.singleton;

    [Header("Player Prefabs")]
    [SerializeField] private GameObject gamePlayerPrefab;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            GameObject lobbyPlayerInstance = Instantiate(playerPrefab);
            lobbyPlayerInstance.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance);
        }
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Contains("MainMenu") || currentScene.Contains("Lobby")) return;

        if (conn.identity != null && conn.identity.TryGetComponent(out LobbyPlayer lobbyPlayer))
        {
            PlayerNetworkData retainedData = lobbyPlayer.networkData;

            Transform spawnPoint = GetStartPosition();
            GameObject gamePlayerInstance = spawnPoint != null 
                ? Instantiate(gamePlayerPrefab, spawnPoint.position, spawnPoint.rotation)
                : Instantiate(gamePlayerPrefab);

            if (gamePlayerInstance.TryGetComponent(out GamePlayer gameScript))
            {
                gameScript.networkData = retainedData;
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
        if (NetworkClient.connection != null && NetworkClient.connection.isAuthenticated && !NetworkClient.ready) NetworkClient.Ready();
        if (NetworkClient.connection != null && NetworkClient.connection.isAuthenticated && NetworkClient.localPlayer == null) NetworkClient.AddPlayer();
    }

    public void LeaveGame()
    {
        SteamLobbyManager.Instance.LeaveLobby();
        if (NetworkServer.active && NetworkClient.isConnected) StopHost();
        else if (NetworkClient.isConnected) StopClient();

        foreach (var LobbyPlayer in FindObjectsByType<LobbyPlayer>())
        {
            if (LobbyPlayer != null)
            {
                Destroy(LobbyPlayer.gameObject);
            }
        }

        NetworkClient.Shutdown();

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public override void OnClientDisconnect()
    {
        SteamLobbyManager.Instance.LeaveLobby();

        NetworkClient.Shutdown();

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}