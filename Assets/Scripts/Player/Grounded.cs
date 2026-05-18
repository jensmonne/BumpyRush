using System.Collections;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    public bool isGrounded = false;

    private Coroutine helpCoroutine;

    public bool HELP = false;

    // COLLISIONS
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            HELP = false;
            Debug.Log("Grounded");

            // Reset timer
            if (helpCoroutine != null)
            {
                StopCoroutine(helpCoroutine);
                helpCoroutine = null;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log("Not Grounded");

            // Start timer
            if (helpCoroutine == null)
            {
                helpCoroutine = StartCoroutine(Helpcheck());
            }
        }
    }

    IEnumerator Helpcheck()
    {
        yield return new WaitForSeconds(2f);

        // Alleen uitvoeren als nog steeds niet grounded
        if (!isGrounded)
        {
            HELP = true;
        }

        helpCoroutine = null;
    }
}