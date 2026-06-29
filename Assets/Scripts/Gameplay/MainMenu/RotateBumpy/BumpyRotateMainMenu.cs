using UnityEngine;

public class BumpyRotateMainMenu : MonoBehaviour
{
    [SerializeField] private GameObject BumpyStage;
    [SerializeField] private float rotationSpeed = 300f;

    private float currentDirection = 0f;

    private void Update()
    {
        // 1. Draai het object als er een richting is meegegeven
        if (BumpyStage != null && currentDirection != 0f)
        {
            BumpyStage.transform.Rotate(Vector3.up * currentDirection * rotationSpeed * Time.deltaTime);
        }

        // 2. Reset de richting direct naar 0 voor de volgende frame.
        // Als de knop nog steeds ingedrukt is, zal de knop deze in de volgende frame weer op 1 of -1 zetten.
        currentDirection = 0f;
    }

    public void SetRotationDirection(float direction)
    {
        currentDirection = direction;
    }

    public void RotateLeft()
    {
        SetRotationDirection(10f);
    }

    public void RotateRight()
    {
        SetRotationDirection(-10f);
    }
}