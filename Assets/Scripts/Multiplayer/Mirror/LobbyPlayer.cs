using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandlePlayerNameChanged))]
    public string PlayerName = "Player";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    private LobbyPlayerCard myCard;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PlayerName = PlayerPrefs.GetString("PlayerName", $"Player {Random.Range(1000, 9999)}");
    }

    public override void OnStartServer()
    {
        PlayerName = $"Player {netId}";
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
        LobbyUIManager.Instance.RemovePlayerFromDisplay(this);
    }

    private void HandlePlayerNameChanged(string oldName, string newName)
    {
        if (myCard != null) myCard.UpdateName(newName);
    }

    private void HandleReadyStatusChanged(bool oldStatus, bool newStatus)
    {
        if (myCard != null) myCard.UpdateReadyStatus(newStatus);

        LobbyUIManager.Instance.UpdateStartButton();
    }

    public void SetCard(LobbyPlayerCard card)
    {
        myCard = card;
        myCard.UpdateName(PlayerName);
        myCard.UpdateReadyStatus(isReady);
    }
}