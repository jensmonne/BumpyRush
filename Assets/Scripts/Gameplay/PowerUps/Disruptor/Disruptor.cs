using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Disruptor : NetworkBehaviour
{
    private GameObject player;

    [SyncVar]
    private bool isActive = false;

    private Rigidbody rb;
    private readonly float disruptionForce = 1800f;
    [SerializeField] private float disruptionDuration = 3f;

    private void Start()
    {
        if (!isServer) return;

        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene! Make sure it has the 'Player' tag.");
            return;
        }
        Activate();
        Debug.Log("Disruptor script started, player found: " + (player != null) + player.name);
    }

    private void Update()
    {
        if (!isServer || !isActive) return;

        if (rb == null && player != null)
        {
            rb = player.GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.AddForce(new Vector3(0f, 0f, -disruptionForce));
        }
    }

    [Server]
    public void Activate()
    {
        if (player == null) return;

        rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = player.GetComponentInParent<Rigidbody>();
        }

        if (isActive)
        {
            Debug.Log("Disruptor is already active.");
            return;
        }

        StartCoroutine(Disrupt());
    }

    [Server]
    private IEnumerator Disrupt()
    {
        isActive = true;
        yield return new WaitForSeconds(disruptionDuration);
        isActive = false;
    }
}