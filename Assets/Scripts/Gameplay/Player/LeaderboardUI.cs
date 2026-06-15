using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> playerTMPs = new List<GameObject>();
    [SerializeField] private Color localPlayerColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private void OnEnable() => GameManager.OnScoresChanged += Rebuild;
    private void OnDisable() => GameManager.OnScoresChanged -= Rebuild;

    private void Start()
    {
        foreach (var entry in playerTMPs)
            entry.SetActive(false);

        Rebuild();
    }

    private void Rebuild()
    {
        foreach (var entry in playerTMPs)
            entry.SetActive(false);

        if (GameManager.Instance == null) return;

        uint localNetId = NetworkClient.connection?.identity?.netId ?? 0;

        List<KeyValuePair<uint, int>> sorted = new(GameManager.Instance.playerScores);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

        for (int i = 0; i < sorted.Count && i < playerTMPs.Count; i++)
        {
            uint netId = sorted[i].Key;
            int score = sorted[i].Value;
            bool isLocal = netId == localNetId;

            GameObject entry = playerTMPs[i];
            entry.SetActive(true);

            TextMeshProUGUI tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = $"{i + 1}. {(isLocal ? "You" : $"Player {netId}")} - {score}";
                tmp.color = isLocal ? localPlayerColor : defaultColor;
            }
        }
    }
}