using UnityEngine;
using Mirror;
using Utp;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class CustomNetworkManager : NetworkManager
{
    public static new CustomNetworkManager singleton => (CustomNetworkManager)NetworkManager.singleton;

    private UtpTransport utpTransport;

    public string relayJoinCode { get; private set; }

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
        () => Debug.LogError("Failed to start Relay server."));
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
}