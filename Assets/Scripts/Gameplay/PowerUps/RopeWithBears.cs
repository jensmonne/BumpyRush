using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RopeWithCubes : NetworkBehaviour
{
    [Header("Bear Settings")]
    [SerializeField] private GameObject bearPrefab;
    [SerializeField] private int initialBearCount = 5;
    [SerializeField] private float bearMass = 0.5f;
    [SerializeField] private LayerMask bearLayer;

    [Header("Chain Settings")]
    [SerializeField] private float segmentLength = 1f;
    [SerializeField] private float springStrength = 50f;
    [SerializeField] private float springDamper = 5f;
    [SerializeField] private float ropeHeight = 1f;
    [SerializeField] private Vector3 ropeOriginOffset = new Vector3(0, 0, -3f);

    private GameObject anchor;
    private Rigidbody anchorRb;
    private Collider[] carColliders;
    private readonly List<GameObject> bears = new List<GameObject>();
    private int bearLayerIndex;

    private void Start()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Car needs a Rigidbody!");
            return;
        }

        carColliders = GetComponentsInChildren<Collider>();

        bearLayerIndex = (int)Mathf.Log(bearLayer.value, 2);
        // Only bears ignore each other — NOT the ground layer
        Physics.IgnoreLayerCollision(bearLayerIndex, bearLayerIndex, true);

        anchor = new GameObject("RopeAnchor");
        anchor.transform.SetParent(transform);
        anchor.transform.localPosition = ropeOriginOffset + Vector3.up * ropeHeight;
        anchorRb = anchor.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        for (int i = 0; i < initialBearCount; i++)
            AddBear();
    }

    public void AddBear()
    {
        int index = bears.Count;

        Vector3 spawnPos = transform.position
            + ropeOriginOffset
            + -transform.forward * segmentLength * index
            + Vector3.up * ropeHeight;

        GameObject bear = Instantiate(bearPrefab, spawnPos, Quaternion.identity);

        foreach (Transform t in bear.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = bearLayerIndex;

        Rigidbody rb = bear.GetComponent<Rigidbody>();
        if (rb == null) rb = bear.AddComponent<Rigidbody>();
        rb.mass = bearMass;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        Collider col = bear.GetComponentInChildren<Collider>();
        if (col == null) col = bear.AddComponent<BoxCollider>();
        col.isTrigger = false;

        // Ignore car colliders specifically, not the whole car layer
        foreach (Collider carCol in carColliders)
            Physics.IgnoreCollision(col, carCol);

        Rigidbody connectedRb = index == 0 ? anchorRb : bears[index - 1].GetComponent<Rigidbody>();

        SpringJoint joint = bear.AddComponent<SpringJoint>();
        joint.connectedBody = connectedRb;
        joint.spring = springStrength;
        joint.damper = springDamper;
        joint.minDistance = 0f;
        joint.maxDistance = segmentLength;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        bears.Add(bear);
    }

    public void RemoveBear()
    {
        if (bears.Count == 0) return;
        int last = bears.Count - 1;
        Destroy(bears[last]);
        bears.RemoveAt(last);
    }

    public void ClearBears()
    {
        foreach (GameObject bear in bears)
            if (bear != null) Destroy(bear);
        bears.Clear();
    }

    public int BearCount => bears.Count;
}