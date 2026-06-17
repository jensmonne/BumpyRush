using UnityEngine;

/// <summary>
/// Dit script schakelt de bounce collider uit wanneer de speler flipped is of niet grounded is,
/// en schakelt de bounce collider uit als een van beide waar is.
/// Dit zorgt ervoor dat als de auto flipped is, dat hij niet door blijft bouncen
/// </summary>

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
        }
        else
        {
            Bounce.enabled = true;
        }
    }

    private void CheckIfFlipped()
    {
        float zRotation = transform.eulerAngles.z;

        Flipped = zRotation > checkRotation && zRotation < 360f - checkRotation;
    }
}