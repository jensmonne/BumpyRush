using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;
    [SerializeField] private Transform carTransform;

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

    public void OnUnstuckButton()
    {
        if (carTransform != null)
        {
            carTransform.position = new Vector3(13f, 10f, -100f);
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