using Mirror;
using UnityEngine;
public class TeddyBear : PickUpBase
{
    protected override void OnPickUpServer(NetworkIdentity playerIdentity)
    {
        base.OnPickUpServer(playerIdentity);
        Debug.Log($"Server: Player {playerIdentity.netId} picked up a teddy bear");
        //Score stuff nees to go here, but we don't have a score system yet so idk
    }
    protected override void OnPickUpClient(NetworkIdentity playerIdentity)
    {
        base.OnPickUpClient(playerIdentity);
        string picker = playerIdentity != null
            ? $"Player {playerIdentity.netId}"
            : "Unknown player";
        Debug.Log($"Client: {picker} picked up a teddy bear yaaaay");
        // Add client effects here (e.g., sounds, particles idk)
    }

}
