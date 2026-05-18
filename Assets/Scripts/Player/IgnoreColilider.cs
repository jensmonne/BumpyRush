using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    [SerializeField] Collider myCollider;
    [SerializeField] Collider objectToIgnore;

    void Start()
    {
        Physics.IgnoreCollision(myCollider, objectToIgnore);
    }
}