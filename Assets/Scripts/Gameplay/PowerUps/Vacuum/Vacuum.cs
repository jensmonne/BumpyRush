using System.Collections;
using Mirror;
using UnityEngine;

public class Vacuum : NetworkBehaviour
{
    private readonly int vacuumRange = 20
        ;
    private readonly int vacuumDuration = 5;

    private void Start()
    {
        if (!isServer) return;
        UseVacuum();
    }

    [Server]
    private void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Bear"))
        {
            Rigidbody bearRigidbody = collision.GetComponent<Rigidbody>();
            if (bearRigidbody != null)
            {
                Vector3 directionToVacuum = (transform.position - collision.transform.position).normalized;
                float distance = Vector3.Distance(transform.position, collision.transform.position);
                float forceMagnitude = Mathf.Lerp(50f, 0f, distance / vacuumRange);
                bearRigidbody.AddForce(directionToVacuum * forceMagnitude, ForceMode.Acceleration);
            }
        }
    }

    [Server]
    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Bear"))
        {
            Rigidbody bearRigidbody = collision.GetComponent<Rigidbody>();
            if (bearRigidbody != null)
            {
                bearRigidbody.linearVelocity = Vector3.zero;
            }
        }
    }

    [Server]
    public void UseVacuum()
    {
        StartCoroutine(startDelay(vacuumDuration));
        RpcShowVacuumEffect();
    }

    private IEnumerator startDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }


    [ClientRpc]
    private void RpcShowVacuumEffect()
    {

        Debug.Log("Vacuum effect triggered!");
    }

}