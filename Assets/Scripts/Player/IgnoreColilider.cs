using UnityEngine;

/// <summary>
/// Dit script zorgt ervoor dat de speler een specifieke collider negeert,
/// wat handig kan zijn voor situaties zoals het negeren van botsingen met bepaalde objecten of triggers.
/// </summary>
public class IgnoreCollider : MonoBehaviour
{
    [SerializeField] Collider myCollider;
    [SerializeField] Collider objectToIgnore;

    void Start()
    {
        Physics.IgnoreCollision(myCollider, objectToIgnore);
    }
}