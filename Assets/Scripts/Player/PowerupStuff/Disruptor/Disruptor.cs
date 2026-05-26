using System.Collections;
using UnityEngine;
public class Disruptor : MonoBehaviour
{
    private GameObject player;
    private bool isActive = false;
    private Rigidbody rb;
    [SerializeField] private float disruptionForce = 100f;
    [SerializeField] private float disruptionDuration = 3f;

    private void Start()
    {
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
        if (isActive)
        {
            rb.AddForce(new Vector3(0f, 0f, -disruptionForce));
        }
    }

    public void Activate()
    {
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

    public IEnumerator Disrupt()
    {
        isActive = true;
        yield return new WaitForSeconds(disruptionDuration);
        isActive = false;
    }

}
