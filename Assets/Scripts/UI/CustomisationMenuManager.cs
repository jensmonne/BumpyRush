using TMPro;
using UnityEngine;

public class CustomisationMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    private void Start()
    {
        // TODO: change this to get steam username
        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            string randomName = $"Player {Random.Range(1000, 9999)}";
            PlayerPrefs.SetString("PlayerName", randomName);
            PlayerPrefs.Save();

            Debug.Log($"No name found! Created and saved a default name: {randomName}");
        }
        
        nameInputField.text = PlayerPrefs.GetString("PlayerName");
    }

    public void OnNameInputFieldChanged()
    {
        PlayerPrefs.SetString("PlayerName", nameInputField.text.Trim());
        PlayerPrefs.Save();
    }
}