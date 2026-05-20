using UnityEngine;

public class FlipBounceDisabler : MonoBehaviour
{
    // Uitleg script:
    // Dit script controleert of de speler flipped is of niet grounded is,
    // en schakelt de bounce collider uit als een van beide waar is.

    //Dit zorgt ervoor dat als de auto flipped is, dat hij niet door blijft bouncen

    [SerializeField] private float checkRotation = 45f;
    [SerializeField] private BoxCollider Bounce;
    [SerializeField] private Grounded groundedScript;

    private bool Flipped;
    private bool Grounded;

    private void Update()
    {
        CheckIfFlipped();
        CheckIfGrounded();

        if (Flipped || !Grounded)
        {
            Bounce.enabled = false;
            Debug.Log("Flipped or Not Grounded");
        }
        else
        {
            Bounce.enabled = true;
            Debug.Log("Not Flipped and Grounded");
        }
    }

    private void CheckIfFlipped()
    {
        float zRotation = transform.eulerAngles.z;

        Flipped = zRotation > checkRotation && zRotation < 360f - checkRotation;
    }

    private void CheckIfGrounded()
    {
        Grounded = groundedScript.isGrounded;
    }
}