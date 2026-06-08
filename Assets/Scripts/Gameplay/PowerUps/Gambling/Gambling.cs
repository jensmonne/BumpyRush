using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Gambling : NetworkBehaviour
{
    [SerializeField] private GameObject Coin;
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private int winAmount = 3;
    [SerializeField] private int loseAmount = -2;

    [SyncVar(hook = nameof(HandleResultChanged))]
    private bool didWin = false;

    [SyncVar]
    private bool hasActivated = false;

    private void Start()
    {
        if (!isServer) return;

        Coin.SetActive(true);
        ActivateGambling();
    }

    [Server]
    public void ActivateGambling()
    {
        if (hasActivated) return;
        hasActivated = true;

        int randomNumber = Random.Range(0, 100);
        Debug.Log("Random number generated: " + randomNumber);

        if (randomNumber < 50)
        {
            didWin = true;
            Debug.Log("You won! You get " + winAmount + " bear(s) !");
            RpcPlayAnimation("Win");
        }
        else
        {
            didWin = false;
            Debug.Log("You lost! You lose " + loseAmount + " bear(s) !");
            RpcPlayAnimation("Lose");
        }

        StartCoroutine(WaitForAnimation());
    }

    [ClientRpc]
    private void RpcPlayAnimation(string triggerName)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    private IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(animationDuration);
        NetworkServer.Destroy(gameObject);
    }

    private void HandleResultChanged(bool oldValue, bool newValue)
    {
        Debug.Log(newValue ? "Winner!" : "Better luck next time!");
    }
}