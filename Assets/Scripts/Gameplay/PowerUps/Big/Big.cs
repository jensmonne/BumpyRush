using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
public class Big : MonoBehaviour
{
    [SerializeField] private float bigDuration = 10f;
    [SerializeField] private float bigScaleMultiplier = 4f;
    private Rigidbody rb;
    private GameObject player;
    [SerializeField] private CinemachineCamera cam;
    private bool isBigActive = false;

    private void Start()
    {
        player = GetComponentInParent<Movement>().gameObject;
        rb = player.GetComponent<Rigidbody>();
        cam = FindAnyObjectByType<CinemachineCamera>();
        ActivateBig();
    }
    public void ActivateBig()
    {
        if (isBigActive)
        {
            Debug.Log("Big power-up is already active!");
            return;
        }
        isBigActive = true;
        StartCoroutine(BigDuration(bigDuration));
        Debug.Log("Big power-up activated! Player is now bigger.");
    }
    IEnumerator BigDuration(float duration)
    {
        ActivateBig();
        player.transform.localScale *= bigScaleMultiplier;
        rb.mass *= bigScaleMultiplier;
        cam.Lens.FieldOfView += 40f;
        player.transform.position += Vector3.up * 4f;
        yield return new WaitForSeconds(duration);
        player.transform.localScale /= bigScaleMultiplier;
        rb.mass /= bigScaleMultiplier;
        cam.Lens.FieldOfView -= 40f;
        isBigActive = false;
    }
}
