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
        if (UnityAuthInitializer.IsAuthenticated) EnableOnlineButtons();
    }

    private void OnEnable()
    {
        UnityAuthInitializer.OnAuthenticated += EnableOnlineButtons;
    }

    private void OnDestroy()
    {
        UnityAuthInitializer.OnAuthenticated -= EnableOnlineButtons;
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

        CustomNetworkManager.singleton.StartRelayHost(maxPlayers);
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

        CustomNetworkManager.singleton.JoinRelayGame(code, () =>
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