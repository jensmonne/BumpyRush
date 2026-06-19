using UnityEngine;

public class FlippedHelper : MonoBehaviour
{
    [SerializeField] private float checkRotation = 45f;
    [SerializeField] private Collider Bounce;
    [SerializeField] private float waitTimeBeforeHelp = 2f; // De 2 seconden instelling

    [Header("Ground Check")]
    [SerializeField] private Grounded grounded;

    private bool Flipped;
    private float flipTimer;
    private bool helpBumpyUp;

    private void Update()
    {
        // 1. De oude vertrouwde rotatie check
        CheckIfFlipped();

        // 2. Directe if/else voor de collider (zoals je oude script)
        if (Flipped)
        {
            Bounce.enabled = false;
        }
        else
        {
            Bounce.enabled = true;
        }

        // 3. Timer check gebaseerd op de Flipped status
        if (Flipped)
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= waitTimeBeforeHelp)
            {
                helpBumpyUp = true;
            }
        }
        else
        {
            // Reset zodra je weer recht staat
            flipTimer = 0f;
            helpBumpyUp = false;
        }

        // 4. Help bumpy uitvoeren
        if (helpBumpyUp)
        {
            HelpBumpy();
        }
    }

    private void CheckIfFlipped()
    {
        float zRotation = transform.eulerAngles.z;

        Flipped = zRotation > checkRotation && zRotation < 360f - checkRotation;
    }

    private void HelpBumpy()
    {
        // Jouw originele HelpBumpy logica
        Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        
        // Zodra hij weer redelijk recht staat, stoppen met draaien
        if (transform.eulerAngles.z < checkRotation || transform.eulerAngles.z > 360f - checkRotation)
        {
            helpBumpyUp = false;
            flipTimer = 0f;
        }
    }
}