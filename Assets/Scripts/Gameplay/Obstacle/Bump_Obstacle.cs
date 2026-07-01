using UnityEngine;
using System.Collections;
/// <summary>
/// Dit script detecteert wanneer een speler de bump obstacle raakt en geeft een bump kracht terug op basis van de impact kracht,
/// </summary>
public class Bump_Obstacle : MonoBehaviour
{
    [SerializeField] private float bumpForce_ = 20f;
    [SerializeField] private ParticleSystem bumpEffect;
    [SerializeField] private AudioClip BumpSound;
    [SerializeField] private GameObject Bumpeffect;

    private bool canBump = true;


    void Start()
    {
        Bumpeffect.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!canBump) return;

            Debug.Log("Player hit the bump obstacle!");
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 bumpDirection = collision.contacts[0].normal;
                playerRb.AddForce(bumpDirection * bumpForce_, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning("Player does not have a Rigidbody component.");
            }
            if (bumpEffect != null)
            {
                ParticleSystem instantiatedEffect = Instantiate(bumpEffect, collision.contacts[0].point, Quaternion.identity);
                instantiatedEffect.Play();
                Destroy(instantiatedEffect.gameObject, instantiatedEffect.main.duration);
            }
            StartCoroutine(SoundAlarm());
        }
    }

    private IEnumerator SoundAlarm()
    {
        canBump = false;

        SoundManager.Instance.Play3DSFX(BumpSound, transform.position);
        Bumpeffect.SetActive(true);

        float duration = 4f;
        float rotateSpeed = 720f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Bumpeffect.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
        canBump = true;

        Bumpeffect.SetActive(false);
    }
}
