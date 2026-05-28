using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostMenuManager : MonoBehaviour
{
    public enum LobbyVisibility { Private, FriendsOnly, Public }

    [Header("Visibility UI References")]
    [SerializeField] private TMP_Text visibilityText;
    [SerializeField] private Button visibilityLeftBtn;
    [SerializeField] private Button visibilityRightBtn;

    [Header("Max Players UI References")]
    [SerializeField] private TMP_Text maxPlayersText;
    [SerializeField] private Button maxPlayersLeftBtn;
    [SerializeField] private Button maxPlayersRightBtn;

    [Header("Player Count Constraints")]
    [SerializeField] private int minPlayersAllowed = 2;
    [SerializeField] private int maxPlayersAllowed = 4;
    
    private LobbyVisibility currentVisibility = LobbyVisibility.Public;
    private int currentMaxPlayers = 4;
    private int totalVisibilityOptions;

    private void Start()
    {
        totalVisibilityOptions = Enum.GetValues(typeof(LobbyVisibility)).Length;
        visibilityLeftBtn.onClick.AddListener(() => CycleVisibility(-1));
        visibilityRightBtn.onClick.AddListener(() => CycleVisibility(1));

        maxPlayersLeftBtn.onClick.AddListener(() => ChangeMaxPlayers(-1));
        maxPlayersRightBtn.onClick.AddListener(() => ChangeMaxPlayers(1));

        UpdateVisibilityUI();
        UpdateMaxPlayersUI();
    }

    public void OnStartPressed()
    {
        MenuManager.Instance.OpenMenu("LoadingMenu");
        MenuManager.Instance.SetLoadingStatusText("Starting online game...");

        SteamLobbyManager.Instance.CreateLobby(currentMaxPlayers, currentVisibility, () =>
        {
            MenuManager.Instance.SetLoadingStatusText("Failed to start online game.");
            MenuManager.Instance.OpenMenu("MainMenu");
        });
    }

    public void OnBackPressed()
    {
        MenuManager.Instance.OpenMenu("MainMenu");
    }

    private void CycleVisibility(int direction)
    {
        int currentIndex = (int)currentVisibility + direction;

        if (currentIndex < 0) 
            currentIndex = totalVisibilityOptions - 1;
        else if (currentIndex >= totalVisibilityOptions) 
            currentIndex = 0;

        currentVisibility = (LobbyVisibility)currentIndex;
        UpdateVisibilityUI();
    }

    private void UpdateVisibilityUI()
    {
        switch (currentVisibility)
        {
            case LobbyVisibility.Public: visibilityText.text = "Public"; break;
            case LobbyVisibility.FriendsOnly: visibilityText.text = "Friends Only"; break;
            case LobbyVisibility.Private: visibilityText.text = "Private"; break;
        }
    }

    private void ChangeMaxPlayers(int amount)
    {
        currentMaxPlayers += amount;

        currentMaxPlayers = Mathf.Clamp(currentMaxPlayers, minPlayersAllowed, maxPlayersAllowed);
        
        UpdateMaxPlayersUI();
    }

    private void UpdateMaxPlayersUI()
    {
        maxPlayersText.text = currentMaxPlayers.ToString();
    }
}