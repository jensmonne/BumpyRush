using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    // [SerializeField] private Button localButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;
    // [SerializeField] private Button backButton;

    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        if (UnityAuthInitializer.IsAuthenticated) EnableOnlineButton();
    }

    private void OnEnable()
    {
        UnityAuthInitializer.OnAuthenticated += EnableOnlineButton;
    }

    private void OnDestroy()
    {
        UnityAuthInitializer.OnAuthenticated -= EnableOnlineButton;
    }

    private void EnableOnlineButton()
    {
        hostButton.interactable = true;
        joinCodeInput.interactable = true;
        statusText.text = "Play locally or online by hosting or joining a game.";
    }

    public void OnLocalPressed()
    {
        statusText.text = "Starting local game...";

        CustomNetworkManager.singleton.StartLocalGame();
    }

    public void OnHostPressed()
    {
        statusText.text = "Starting online game...";
        
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

        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "Please enter a join code.";
            return;
        }

        CustomNetworkManager.singleton.JoinRelayGame(code);
    }

    public void OnBackPressed()
    {
        MenuManager.Instance.OpenMenu("MainMenu");
    }
}