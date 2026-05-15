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
    [SerializeField] private Button startGameButton;

    private Dictionary<LobbyPlayer, LobbyPlayerCard> playerCards = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
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

    public void AddPlayerToDisplay(LobbyPlayer player)
    {
        if (playerCards.ContainsKey(player)) return;

        GameObject newCard = Instantiate(playerCardPrefab, playerCardContainer);
        LobbyPlayerCard cardScript = newCard.GetComponent<LobbyPlayerCard>();

        player.SetCard(cardScript);
        playerCards.Add(player, cardScript);
    }

    public void RemovePlayerFromDisplay(LobbyPlayer player)
    {
        if (playerCards.TryGetValue(player, out LobbyPlayerCard card))
        {
            Destroy(card.gameObject);
            playerCards.Remove(player);
        }

        UpdateStartButton();
    }

    public void UpdateStartButton()
    {
        if (!NetworkServer.active)
        {
            // Maybe show a "Waiting for host..." message here instead?
            startGameButton.gameObject.SetActive(false);
            return;
        }

        startGameButton.gameObject.SetActive(true);

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
        CustomNetworkManager.singleton.ServerChangeScene("");
    }
}