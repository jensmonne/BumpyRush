using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button FriendsButton;
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
        FriendsButton.interactable = true;
    }

    public void OnHostPressed()
    {
        MenuManager.Instance.OpenMenu("HostMenu");
    }

    public void OnFriendsPressed()
    {
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