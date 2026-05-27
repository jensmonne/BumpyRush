using UnityEngine;

public class NameDisplayLock : MonoBehaviour
{
    private Camera localCamera;

    void Start()
    {
        localCamera = FindAnyObjectByType<Camera>();
    }

    void LateUpdate()
    {
        if (localCamera == null)
        {
            Debug.LogWarning("No main camera found for NameDisplayLock script.");
            return;
        }

        transform.LookAt(
            transform.position + localCamera.transform.rotation * Vector3.forward,
            localCamera.transform.rotation * Vector3.up
        );
    }
}