using System.Collections;
using Mirror;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Big : NetworkBehaviour
{
    [SerializeField] private float bigDuration = 10f;
    [SerializeField] private float bigScaleMultiplier = 2f;
    private Rigidbody rb;
    private GameObject player;
    [SerializeField] private CinemachineCamera cam;

    [SyncVar(hook = nameof(HandleBigActiveChanged))]
    private bool isBigActive = false;

    private void Start()
    {
        if (!isServer) return;

        player = GetComponentInParent<Movement>().gameObject;
        rb = player.GetComponent<Rigidbody>();
        cam = FindAnyObjectByType<CinemachineCamera>();
        ActivateBig();
    }

    [Server]
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

    [Server]
    private IEnumerator BigDuration(float duration)
    {
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

    private void HandleBigActiveChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Big power-up is now active on all clients");
        }
        else
        {
            Debug.Log("Big power-up has ended on all clients");
        }
    }
}