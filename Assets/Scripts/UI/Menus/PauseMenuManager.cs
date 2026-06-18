using UnityEngine;
using Mirror;

public class PauseMenuManager : MonoBehaviour
{
    private CanvasGroup pauseMenuCanvasGroup;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuCanvasGroup == null)
        {
            pauseMenuCanvasGroup = GetComponent<CanvasGroup>();
        }

        SetPauseMenuVisibility(false);
    }

    public void HandleEscapePress()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;
        SetPauseMenuVisibility(true);
        CursorController.Instance.UnlockCursor();
    }

    private void ResumeGame()
    {
        isPaused = false;
        SetPauseMenuVisibility(false);
        CursorController.Instance.LockCursor();
    }

    public void OnLeaveButton()
    {
        CursorController.Instance.UnlockCursor();
        SteamLobbyManager.Instance.LeaveLobby();
        CustomNetworkManager.singleton.LeaveGame();
    }

    public void OnUnstuckButton()
    {
        if (NetworkClient.localPlayer.TryGetComponent(out GamePlayer localPlayer))
        {
            localPlayer.UnstuckPlayer();
        }
        
        isPaused = false;
        SetPauseMenuVisibility(false);
        CursorController.Instance.LockCursor();
    }

    private void SetPauseMenuVisibility(bool visible)
    {
        pauseMenuCanvasGroup.alpha = visible ? 1f : 0f;
        pauseMenuCanvasGroup.interactable = visible;
        pauseMenuCanvasGroup.blocksRaycasts = visible;
    }
}