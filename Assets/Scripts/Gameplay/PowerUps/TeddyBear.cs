using Mirror;

public class TeddyBear : PickUpBase
{
    //protected override void OnPickUpServer(NetworkIdentity playerIdentity)
    //{
    //    base.OnPickUpServer(playerIdentity);
    //}

    protected override void OnPickUpClient(NetworkIdentity playerIdentity)
    {
        base.OnPickUpClient(playerIdentity);
        playerIdentity.gameObject.GetComponent<RopeWithBears>().AddBear();
    }
}