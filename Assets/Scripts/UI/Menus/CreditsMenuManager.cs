using UnityEngine;

public class CreditsMenuManager : MonoBehaviour
{
    public void OnBackPressed()
    {
        MenuManager.Instance.CloseTopMenu();
    }
}