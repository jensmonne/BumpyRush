using System.Collections;
using UnityEngine;

public class Gambling : MonoBehaviour
{
    [SerializeField] private GameObject Coin;
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private int winAmount = 3;
    [SerializeField] private int loseAmount = -2;

    public void Start()
    {
        Coin.SetActive(true);
        ActivateGambling();
    }
    public void ActivateGambling()
    {
        int randomNumber = Random.Range(0, 100);
        Debug.Log("Random number generated: " + randomNumber);
        if (randomNumber < 50)
        {
            GetComponent<Animator>().SetTrigger("Win");
            Debug.Log("You won! You get " + winAmount + " bear(s) !");

        }
        else
        {
            GetComponent<Animator>().SetTrigger("Lose");
            Debug.Log("You lost! You lose " + loseAmount + " bear(s) !");

        }
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(animationDuration);
        Destroy(gameObject);
    }
}
