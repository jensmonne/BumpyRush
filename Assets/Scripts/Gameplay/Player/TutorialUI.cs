using System.Collections;
using UnityEngine;
public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialText;
    [SerializeField] private int displayDurationSeconds = 10;

    private void Start()
    {
        tutorialText.SetActive(true);
        StartCoroutine(HideTutorialAfterDelay(displayDurationSeconds));
    }

    private IEnumerator HideTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tutorialText.SetActive(false);
    }
}

