using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Player Data")]
    public string PlayerName = "Player";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    private LobbyPlayerCard myCard;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PlayerName = PlayerPrefs.GetString("PlayerName");
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