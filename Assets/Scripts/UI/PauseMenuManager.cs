using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;

    private bool isPaused = false;

    private void Start()
    {
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

    private void SetPauseMenuVisibility(bool visible)
    {
        pauseMenuCanvasGroup.alpha = visible ? 1f : 0f;
        pauseMenuCanvasGroup.interactable = visible;
        pauseMenuCanvasGroup.blocksRaycasts = visible;
    }
}