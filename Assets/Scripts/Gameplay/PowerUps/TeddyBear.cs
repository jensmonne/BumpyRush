using Mirror;
//using UnityEngine;

public class TeddyBear : PickUpBase
{
    // protected override void OnPickUpServer(NetworkIdentity playerIdentity)
    // {
    //     base.OnPickUpServer(playerIdentity);
    //     Debug.Log($"Server: Player {playerIdentity.netId} picked up a teddy bear");
    // }

    protected override void OnPickUpClient(NetworkIdentity playerIdentity)
    {
        base.OnPickUpClient(playerIdentity);
        playerIdentity.gameObject.GetComponent<RopeWithBears>().AddBear();
    }
}