using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandlePlayerDataChanged))]
    public PlayerNetworkData networkData;

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    private LobbyPlayerCard myCard;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        string localName = PlayerPrefs.GetString("PlayerName", "Player");
        int localSkin = PlayerPrefs.GetInt("PlayerSkin", 0);

        PlayerNetworkData localData = new PlayerNetworkData
        {
            playerName = localName,
            skinIndex = localSkin
        };

        CmdSetPlayerData(localData);
    }

    [Command]
    public void CmdSetPlayerData(PlayerNetworkData dataToSet)
    {
        networkData = dataToSet;
    }

    [Command]
    public void CmdSetReady(bool readyState)
    {
        isReady = readyState;
    }

    public override void OnStartClient()
    {
        LobbyUIManager.Instance.AddPlayerToDisplay(this);
    }

    public override void OnStopClient()
    {
        if (LobbyUIManager.Instance != null && myCard != null)
        {
            LobbyUIManager.Instance.RemovePlayerFromDisplay(this);
        }
    }

    private void HandlePlayerDataChanged(PlayerNetworkData oldData, PlayerNetworkData newData)
    {
        if (myCard != null) myCard.UpdateName(newData.playerName);
        // need to implement a way of showing the skin stuffs.
    }

    private void HandleReadyStatusChanged(bool oldStatus, bool newStatus)
    {
        if (myCard != null) myCard.UpdateReadyStatus(newStatus);

        LobbyUIManager.Instance.UpdateStartButton();
    }

    public void SetCard(LobbyPlayerCard card)
    {
        myCard = card;
        myCard.UpdateName(networkData.playerName);
        myCard.UpdateReadyStatus(isReady);
    }
}