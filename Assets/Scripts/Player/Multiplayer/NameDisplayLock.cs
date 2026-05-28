using UnityEngine;

public class NameDisplayLock : MonoBehaviour
{
    private Transform localCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            localCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (localCameraTransform == null)
        {
            if (Camera.main != null)
            {
                localCameraTransform = Camera.main.transform;
            }
            return;
        }

        transform.LookAt(
            transform.position + localCameraTransform.rotation * Vector3.forward,
            localCameraTransform.rotation * Vector3.up
        );
    }
}