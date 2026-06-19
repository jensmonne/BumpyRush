using UnityEngine;

public class Grounded : MonoBehaviour
{
    public bool isGrounded { get; private set; }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Bridge"))
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Bridge"))
        {
            isGrounded = false;
        }
    }
}