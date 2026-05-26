using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandleNameChanged))]
    public string PlayerName = "Player";

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

        string localName = PlayerPrefs.GetString("PlayerName");
        CmdSetPlayerName(localName);
    }

    [Command]
    public void CmdSetPlayerName(string nameToSet)
    {
        PlayerName = nameToSet;
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

    private void HandleNameChanged(string oldName, string newName)
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