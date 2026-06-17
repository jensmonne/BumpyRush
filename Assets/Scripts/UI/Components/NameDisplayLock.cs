using UnityEngine;

public class NameDisplayLock : MonoBehaviour
{
    private Transform localCameraTransform;

    private void Start()
    {
        CacheLocalCamera();
    }

    private void LateUpdate()
    {
        if (localCameraTransform == null)
        {
            CacheLocalCamera();
            if (localCameraTransform == null) {
                Debug.LogWarning("No main camera found. NameDisplayLock will not function properly.");
                return;
            };
        }

        transform.LookAt
        (
            transform.position + localCameraTransform.rotation * Vector3.forward,
            localCameraTransform.rotation * Vector3.up
        );
    }

    private void CacheLocalCamera()
    {
        if (Camera.main != null)
        {
            localCameraTransform = Camera.main.transform;
        }
    }
}