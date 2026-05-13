using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;

    public void OnStartPressed()
    {
        MenuManager.Instance.OpenMenu("StartMenu");
    }
    
    public void OnSettingsPressed()
    {
        
    }
    
    public void OnCreditsPressed()
    {
        
    }
    
    public void OnExitPressed()
    {
        Application.Quit();
        Debug.Log("Exited game.");
    }
}