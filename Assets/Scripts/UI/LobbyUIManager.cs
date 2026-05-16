using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text joinCodeText;
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

    private void Start()
    {
        if (CustomNetworkManager.singleton != null)
        {
            string code = CustomNetworkManager.singleton.relayJoinCode;

            if (!string.IsNullOrEmpty(code))
            {
                joinCodeText.text = $"Code: {code}";
            }
            else
            {
                joinCodeText.text = "LOCAL LOBBY";
            }
        }
        else
        {
            Debug.LogWarning("Lobby is probably not initialized yet. NetworkManager could not be found or relayJoinCode is null.");
        }
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
        if (playerCards.TryGetValue(player, out LobbyPlayerCard card))
        {
            Destroy(card.gameObject);
            playerCards.Remove(player);
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

        startGameButton.interactable = allReady && playerCardContainer.childCount > 0;
    }

    public void OnStartGameButton()
    {
        CustomNetworkManager.singleton.ServerChangeScene("TestScene");
    }
}