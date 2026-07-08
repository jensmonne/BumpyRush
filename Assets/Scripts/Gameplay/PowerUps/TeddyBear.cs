using System.Collections;
using Mirror;
using UnityEngine;

public class TeddyBear : PickUpBase
{
    [SerializeField] private float destroyDelay = 2f;
    protected override void OnPickUpServer(NetworkIdentity playerIdentity)
    {
        Debug.LogWarning($"TeddyBear picked up by player netId {playerIdentity.netId}");
        GetComponent<PlushieSFX>()?.PlayPickupSound();
        base.OnPickUpServer(playerIdentity);
        StartCoroutine(DestroyAfterDelay());

    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        NetworkServer.Destroy(gameObject);
    }
}

