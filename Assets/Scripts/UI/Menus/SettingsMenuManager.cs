using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    public void OnBackPressed()
    {
        MenuManager.Instance.CloseTopMenu();
    }
}