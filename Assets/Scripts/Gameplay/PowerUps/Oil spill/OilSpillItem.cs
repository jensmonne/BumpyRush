using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class OilSpillItem : NetworkBehaviour
{
    [SerializeField] private GameObject oilSpillPrefab;
    [SerializeField] private float distanceBehind = 3f;
    [SerializeField]
    private float heightOffset = -0.7f;

    private void Start()
    {
        if (!isServer) return;
        ActivateOilSpill();
    }

    [Server]
    public void ActivateOilSpill()
    {
        GameObject parent = GetComponentInParent<ItemManager>().gameObject;
        Quaternion rotation = parent.transform.rotation;
        Vector3 spawnPos = parent.transform.position - (-parent.transform.forward * distanceBehind);
        spawnPos.y += heightOffset;
        GameObject oilSpill = Instantiate(oilSpillPrefab, spawnPos, rotation);
        NetworkIdentity netIdentity = oilSpill.GetComponent<NetworkIdentity>();
        if (netIdentity != null)
        {
            NetworkServer.Spawn(oilSpill);
        }

        NetworkServer.Destroy(gameObject);
    }
}