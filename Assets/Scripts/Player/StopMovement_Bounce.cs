using System.Collections;
using UnityEngine;

public class StopMovement_Bounce : MonoBehaviour
{
    [SerializeField] private float stoppingMovementDuration = 1f;

    public bool hasCollided = false;

    private Coroutine stopCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
        }

        stopCoroutine = StartCoroutine(StopMovementTimer());

        Debug.Log("Collision detected, stopping movement.");
    }

    private IEnumerator StopMovementTimer()
    {
        hasCollided = true;

        yield return new WaitForSeconds(stoppingMovementDuration);

        hasCollided = false;
    }
}