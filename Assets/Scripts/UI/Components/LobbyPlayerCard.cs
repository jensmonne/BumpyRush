using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image readyStatusImage;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite unreadySprite;

    public void UpdateName(string newName)
    {
        if (playerNameText != null)
        {
            playerNameText.text = newName;
        }
    }

    public void UpdateReadyStatus(bool isReady)
    {
        if (readyStatusImage == null) return;

        Sprite targetSprite = isReady ? readySprite : unreadySprite;

        if (targetSprite != null)
        {
            readyStatusImage.sprite = targetSprite;
        }
    }
}