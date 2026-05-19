using System.Collections;
using UnityEngine;

public class StopMovement_Bounce : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;
    [SerializeField] private float stoppingMovementDuration = 1f;

    public bool hasCollided = false;

    private Coroutine stopCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.GetContact(0).thisCollider != targetCollider)
            return;

        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
        }

        stopCoroutine = StartCoroutine(StopMovementTimer());

        Debug.Log("Correct collider hit!");
    }

    private IEnumerator StopMovementTimer()
    {
        hasCollided = true;

        yield return new WaitForSeconds(stoppingMovementDuration);

        hasCollided = false;
    }
}