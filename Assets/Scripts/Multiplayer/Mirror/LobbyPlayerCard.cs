using TMPro;
using UnityEngine;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text readyStatusText;

    public void UpdateName(string newName)
    {
        playerNameText.text = newName;
    }

    public void UpdateReadyStatus(bool isReady)
    {
        readyStatusText.text = isReady ? "Ready" : "Not Ready";
        readyStatusText.color = isReady ? Color.green : Color.red;
    }
}