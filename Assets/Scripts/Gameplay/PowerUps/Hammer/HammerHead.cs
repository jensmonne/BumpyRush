using UnityEngine;

public class HammerHead : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;
    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasHit && collision.gameObject.CompareTag("Ground"))
        {
            OnGroundHit(collision.contacts[0].point, collision.contacts[0].normal);
        }
        if (!hasHit && collision.gameObject.CompareTag("Player"))
        {
            PlayerFlattenHandler handler = collision.gameObject.GetComponent<PlayerFlattenHandler>();
            if (handler == null)
            {
                handler = collision.gameObject.AddComponent<PlayerFlattenHandler>();
            }
            handler.StartFlatten();
        }

        Debug.Log($"HammerHead collided with {collision.gameObject.name}");
    }

    private void OnGroundHit(Vector3 point, Vector3 normal)
    {
        if (hasHit) return;
        hasHit = true;

        if (hitEffectPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            Instantiate(hitEffectPrefab, point, rotation);
        }
    }
}