using Mirror;
using UnityEngine;

public class TeddyBear : PickUpBase
{

    protected override void OnPickUpServer(NetworkIdentity playerIdentity)
    {
        Debug.LogWarning($"TeddyBear picked up by player netId {playerIdentity.netId}");
        GetComponent<PlushieSFX>()?.PlayPickupSound();

        base.OnPickUpServer(playerIdentity);
    }
}

