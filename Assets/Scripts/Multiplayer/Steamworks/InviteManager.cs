using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;

public class InviteManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text inviteText;

    private Lobby pendingLobby;

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyInvite += OnReceiveInGameInvite;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyInvite -= OnReceiveInGameInvite;
    }

    private void OnReceiveInGameInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"Received an in-game invite from: {friend.Name}");
        
        pendingLobby = lobby;
        inviteText.text = $"{friend.Name} has invited you to join their game!";
        MenuManager.Instance.OpenMenu("InviteMenu");
    }

    public async void AcceptInvite()
    {
        SteamLobbyManager.Instance.OnSteamInviteReceived(pendingLobby, default);
    }

    public void DeclineInvite()
    {
        pendingLobby = default; 
        MenuManager.Instance.OpenMenu("MainMenu");
    }
}