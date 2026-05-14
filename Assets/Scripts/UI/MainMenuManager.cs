using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // [Header("UI Elements")]
    // [SerializeField] private Button startButton;
    // [SerializeField] private Button settingsButton;
    // [SerializeField] private Button creditsButton;
    // [SerializeField] private Button exitButton;

    public void OnStartPressed()
    {
        MenuManager.Instance.OpenMenu("StartMenu");
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