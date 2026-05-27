using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        if (SteamAuthInitializer.IsAuthenticated) EnableOnlineButtons();
        CursorController.Instance.UnlockCursor();
    }

    private void OnEnable()
    {
        SteamAuthInitializer.OnAuthenticated += EnableOnlineButtons;
    }

    private void OnDestroy()
    {
        SteamAuthInitializer.OnAuthenticated -= EnableOnlineButtons;
    }

    private void EnableOnlineButtons()
    {
        hostButton.interactable = true;
        joinCodeInput.interactable = true;
    }

    public void OnLocalPressed()
    {
        MenuManager.Instance.OpenMenu("LoadingMenu");
        MenuManager.Instance.SetLoadingStatusText("Starting local game...");
        CustomNetworkManager.singleton.StartLocalGame();
    }

    public void OnHostPressed()
    {
        MenuManager.Instance.OpenMenu("LoadingMenu");
        MenuManager.Instance.SetLoadingStatusText("Starting online game...");

        int maxPlayers = 4; // Later maybe make this a user input

        SteamLobbyManager.Instance.CreateLobby(maxPlayers, () =>
        {
            MenuManager.Instance.SetLoadingStatusText("Failed to start online game.");
            MenuManager.Instance.OpenMenu("MainMenu");
        });
    }

    public void OnCodeInputChanged()
    {
        string codeInputText = joinCodeInput.text.Trim();
        joinCodeInput.text = codeInputText.ToUpper();
        joinButton.interactable = !string.IsNullOrEmpty(codeInputText);
    }

    public void OnJoinPressed()
    {
        string code = joinCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code)) return;

        MenuManager.Instance.OpenMenu("LoadingMenu");

        SteamLobbyManager.Instance.JoinLobbyByIdString(code, () =>
        {
            MenuManager.Instance.OpenMenu("MainMenu");
        });
    }

    public void OnBackPressed()
    {
        MenuManager.Instance.OpenMenu("MainMenu");
    }

    public void OnSettingsPressed()
    {
        MenuManager.Instance.OpenMenu("SettingsMenu");
    }
    
    public void OnCreditsPressed()
    {
        MenuManager.Instance.OpenMenu("CreditsMenu");
    }
    
    public void OnExitPressed()
    {
        Application.Quit();
        Debug.Log("Exited game.");
    }
}