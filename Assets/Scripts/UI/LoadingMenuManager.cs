using TMPro;
using UnityEngine;

public class LoadingMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void SetLoadingStatusText(string text)
    {
        statusText.text = text;
    }
}