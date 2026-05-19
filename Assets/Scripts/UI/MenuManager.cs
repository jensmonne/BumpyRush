using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public struct MenuPanel
    {
        public string menuName;
        public CanvasGroup canvasGroup;
    }

    public static MenuManager Instance { get; private set; }

    [SerializeField] private LoadingMenuManager loadingMenuManager;

    [SerializeField] private MenuPanel[] menus;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    public void OpenMenu(string name)
    {
        foreach (var menu in menus)
        {
            bool shouldOpen = menu.menuName == name;
            
            menu.canvasGroup.alpha = shouldOpen ? 1 : 0;
            menu.canvasGroup.interactable = shouldOpen;
            menu.canvasGroup.blocksRaycasts = shouldOpen;
        }
    }

    public void SetLoadingStatusText(string text)
    {
        loadingMenuManager.SetLoadingStatusText(text);
    }
}