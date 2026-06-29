using Steamworks;
using TMPro;
using UnityEngine;

public class CustomisationMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private void Start()
    {
        string steamName = SteamClient.Name;

        PlayerPrefs.SetString("PlayerName", steamName);
        PlayerPrefs.Save();
        
        nameText.text = steamName;
    }

    public void OnLeftButton()
    {
        SkinCustomization.Instance.ChangeSkinScrollLeft();
    }

    public void OnRightButton()
    {
        SkinCustomization.Instance.ChangeSkinScrollRight();
    }
}