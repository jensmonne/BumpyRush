using TMPro;
using UnityEngine;

public class FlipBounceDisabler : MonoBehaviour
{
    [SerializeField] private float checkRotation = 45f;
    [SerializeField] private BoxCollider Bounce;

    private bool Flipped;

    private void Update()
    {
        CheckIfFlipped();

        if (Flipped)
        {
            Bounce.enabled = false;
            Debug.Log("Flipped");
        }
        else
        {
            Bounce.enabled = true;
            Debug.Log("Not Flipped");
        }
    }

    private void CheckIfFlipped()
    {
        float zRotation = transform.eulerAngles.z;

        Flipped = zRotation > checkRotation && zRotation < 360f - checkRotation;
    }
}