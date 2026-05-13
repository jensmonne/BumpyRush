using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button localButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        if (UnityAuthInitializer.IsAuthenticated)
        {
            hostButton.interactable = true;
        }

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

    public void LocalButtonPressed()
    {
        
    }

    public void HostButtonPressed()
    {
        
    }

    public void CodeInputChanged()
    {
        joinButton.interactable = !string.IsNullOrEmpty(joinCodeInput.text);
    }

    public void JoinButtonPressed()
    {
        
    }
}