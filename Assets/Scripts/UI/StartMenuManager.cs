using UnityEngine;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;


    private void Start()
    {
        if (startButton == null || settingsButton == null || creditsButton == null || exitButton == null)
        {
            Debug.LogError("One or more UI buttons are not assigned in the inspector.");
            return;
        }
    }

    public void OnStartPressed()
    {
        
    }
    
    public void OnSettingsPressed()
    {
        
    }
    
    public void OnCreditsPressed()
    {
        
    }
    
    public void OnExitPressed()
    {
        
    }
}