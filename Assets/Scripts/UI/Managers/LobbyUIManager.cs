using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    private LobbyPlayer localPlayer;

    private Dictionary<LobbyPlayer, LobbyPlayerCard> playerCards = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddPlayerToDisplay(LobbyPlayer player)
    {
        if (playerCards.ContainsKey(player)) return;

        GameObject newCard = Instantiate(playerCardPrefab, playerCardContainer);
        LobbyPlayerCard cardScript = newCard.GetComponent<LobbyPlayerCard>();

        player.SetCard(cardScript);
        playerCards.Add(player, cardScript);

        if (player.isLocalPlayer)
        {
            localPlayer = player;
            
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(OnReadyClicked);

            UpdateStartButton();
        }
    }

    public void OnReadyClicked()
    {
        if (localPlayer == null) return;

        bool newReadyState = !localPlayer.isReady;
        
        localPlayer.CmdSetReady(newReadyState);
    }

    public void RemovePlayerFromDisplay(LobbyPlayer player)
    {
        if (player == null) return;

        if (playerCards.TryGetValue(player, out LobbyPlayerCard card))
        {
            playerCards.Remove(player);

            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }

        if (player == localPlayer)
        {
            localPlayer = null;
        }

        UpdateStartButton();
    }

    public void UpdateStartButton()
    {
        bool allReady = true;

        foreach (var player in playerCards.Keys)
        {
            if (!player.isReady)
            {
                allReady = false;
                break;
            }
        }

        if (NetworkServer.active)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = allReady && playerCardContainer.childCount > 0;
        }
        else startGameButton.gameObject.SetActive(false);
    }

    public void OnInviteButton()
    {
        SteamLobbyManager.Instance.OpenSteamInviteOverlay();
    }

    public void OnStartGameButton()
    {
        CustomNetworkManager.singleton.ServerChangeScene("MainGame");
    }
}