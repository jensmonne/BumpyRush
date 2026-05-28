using System.Collections.Generic;
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

    private Stack<MenuPanel> menuStack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        InitializeMenus();
    }

    private void InitializeMenus()
    {
        foreach (var menu in menus)
        {
            menu.canvasGroup.alpha = 0;
            menu.canvasGroup.interactable = false;
            menu.canvasGroup.blocksRaycasts = false;
        }
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