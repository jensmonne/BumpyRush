using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button localButton;
    [SerializeField] private Button onlineButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        if (localButton == null || onlineButton == null || settingsButton == null || exitButton == null)
        {
            Debug.LogError("One or more UI buttons are not assigned in the inspector.");
            return;
        }
        
        if (UnityAuthInitializer.IsAuthenticated)
        {
            onlineButton.interactable = true;
        }

        UnityAuthInitializer.OnAuthenticated += EnableOnlineButton;
    }

    private void OnDestroy()
    {
        UnityAuthInitializer.OnAuthenticated -= EnableOnlineButton;
    }

    private void EnableOnlineButton()
    {
        onlineButton.interactable = true;
    }

    public void LocalButtonPressed()
    {
        
    }
}
