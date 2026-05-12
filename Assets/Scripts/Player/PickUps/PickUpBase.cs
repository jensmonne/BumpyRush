using UnityEngine;

public abstract class PickUpBase : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickUp(other.gameObject);
            Destroy(gameObject);
        }
    }

    protected virtual void OnPickUp(GameObject player)
    {
        Debug.Log("Picked up: " + gameObject.name);
    }

}
