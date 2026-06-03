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

    [Header("References")]
    [SerializeField] private LoadingMenuManager loadingMenuManager;
    [SerializeField] private MenuPanel[] menus;

    private Dictionary<string, MenuPanel> menuCache = new();

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
            if (menu.canvasGroup == null) continue;

            if (!menuCache.ContainsKey(menu.menuName))
            {
                menuCache.Add(menu.menuName, menu);
            }

            ConfigureCanvasGroup(menu.canvasGroup, isOpen: false, isInteractable: false);
        }
    }

    public void OpenMenu(string name)
    {
        if (!menuCache.TryGetValue(name, out MenuPanel targetMenu))
        {
            Debug.LogError($"[MenuManager] Cannot find menu named: {name}");
            return;
        }

        if (menuStack.Count > 0)
        {
            MenuPanel currentTop = menuStack.Peek();
            ConfigureCanvasGroup(currentTop.canvasGroup, isOpen: true, isInteractable: false);
        }

        ConfigureCanvasGroup(targetMenu.canvasGroup, isOpen: true, isInteractable: true);
        menuStack.Push(targetMenu);
    }

    public void CloseTopMenu()
    {
        if (menuStack.Count == 0) return;

        MenuPanel closedMenu = menuStack.Pop();
        ConfigureCanvasGroup(closedMenu.canvasGroup, isOpen: false, isInteractable: false);

        if (menuStack.Count > 0)
        {
            MenuPanel underlyingMenu = menuStack.Peek();
            ConfigureCanvasGroup(underlyingMenu.canvasGroup, isOpen: true, isInteractable: true);
        }
    }

    public void SetLoadingStatusText(string text)
    {
        if (loadingMenuManager != null)
        {
            loadingMenuManager.SetLoadingStatusText(text);
        }
    }

    private void ConfigureCanvasGroup(CanvasGroup group, bool isOpen, bool isInteractable)
    {
        if (group == null) return;
        
        group.alpha = isOpen ? 1f : 0f;
        group.blocksRaycasts = isOpen; 
        group.interactable = isInteractable;
    }
}