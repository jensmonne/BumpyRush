using UnityEngine;
/// <summary>
/// Dit script detecteert wanneer een speler de bump obstacle raakt en geeft een bump kracht terug op basis van de impact kracht,
/// </summary>
public class Bump_Obstacle : MonoBehaviour
{
    [SerializeField] private float bumpForce_ = 20f;
    [SerializeField] private ParticleSystem bumpEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the bump obstacle!");
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 bumpDirection = collision.contacts[0].normal;
                playerRb.AddForce(bumpDirection * bumpForce_, ForceMode.Impulse);
            }
            if (bumpEffect != null)
            {
                ParticleSystem instantiatedEffect = Instantiate(bumpEffect, collision.contacts[0].point, Quaternion.identity);
                instantiatedEffect.Play();
                Destroy(instantiatedEffect.gameObject, instantiatedEffect.main.duration);
            }
        }
    }
}
