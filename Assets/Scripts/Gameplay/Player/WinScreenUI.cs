using Mirror;
using TMPro;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI countdownText;

    private void OnEnable()
    {
        GameManager.OnMatchStateChanged += HandleMatchStateChanged;
        GameManager.OnReturnCountdownChanged += HandleCountdownChanged;
    }

    private void OnDisable()
    {
        GameManager.OnMatchStateChanged -= HandleMatchStateChanged;
        GameManager.OnReturnCountdownChanged -= HandleCountdownChanged;
    }

    private void Start() => panel.SetActive(false);

    private void HandleMatchStateChanged(bool isOver, uint winnerNetId, bool isTied)
    {
        if (!isOver) return;

        panel.SetActive(true);

        if (isTied)
        {
            resultText.text = "IT'S A TIE!";
            subtitleText.text = "Equal bears collected!";
            return;
        }

        uint localNetId = NetworkClient.connection?.identity?.netId ?? 0;
        bool localWon = localNetId != 0 && localNetId == winnerNetId;

        string winnerName = GameManager.Instance != null
            ? GameManager.Instance.GetPlayerName(winnerNetId)
            : $"Player {winnerNetId}";

        resultText.text = localWon ? "YOU WIN!" : "YOU LOSE!";
        subtitleText.text = localWon ? "Great driving!" : $"{winnerName} wins!";
    }

    private void HandleCountdownChanged(int seconds)
    {
        if (countdownText != null)
            countdownText.text = $"Returning to lobby in {seconds}...";
    }
}