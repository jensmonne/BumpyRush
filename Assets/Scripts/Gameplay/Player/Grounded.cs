using System.Collections;
using UnityEngine;

/// <summary>
/// Dit script detecteert of de speler grounded is door middel van een trigger collider.
/// </summary>

public class Grounded : MonoBehaviour
{
    public bool isGrounded = false;

    private Coroutine helpCoroutine;

    public bool HELP = false;

    // COLLISIONS
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Bridge"))
        {
            isGrounded = true;
            //StartCoroutine(WaitBeforeGrounded());
            HELP = false;

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
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Bridge"))
        {
            isGrounded = false;

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