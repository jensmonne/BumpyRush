using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string displayName = "Loading...";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    private LobbyPlayerCard myCard;

    public override void OnStartServer()
    {
        displayName = $"Player {netId}";
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

    private void HandleDisplayNameChanged(string oldName, string newName)
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
        myCard.UpdateName(displayName);
        myCard.UpdateReadyStatus(isReady);
    }
}