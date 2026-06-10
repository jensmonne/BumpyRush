using Mirror;
using UnityEngine;
public class RopeWithCubes : NetworkBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int cubeCount = 5;
    [SerializeField] private float segmentLength = 1f;
    [SerializeField] private float cubeMass = 0.5f;
    [SerializeField] private float jointStiffness = 50f;
    [SerializeField] private float jointDamper = 10f;
    [SerializeField] private float ropeHeight = 1f;
    [SerializeField] private Vector3 ropeOriginOffset = new Vector3(0, 0, -3f);
    private Rigidbody carRigidbody;
    private Transform[] cubes;
    private Rigidbody[] cubeRigidbodies;
    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            Debug.LogError("Car needs a Rigidbody!");
            return;
        }
        CreateRopeChain();
    }
    private void FixedUpdate()
    {
        if (cubes == null || cubes.Length == 0) return;
        // Pull cubes toward target position with gravity affecting them
        for (int i = 0; i < cubes.Length; i++)
        {
            Vector3 targetPos = transform.position + ropeOriginOffset
                + -transform.forward * segmentLength * i
                + Vector3.up * ropeHeight;
            Vector3 directionToTarget = targetPos - cubes[i].position;
            // Apply force toward target
            if (cubeRigidbodies[i] != null)
            {
                cubeRigidbodies[i].AddForce(directionToTarget * jointStiffness, ForceMode.Acceleration);
            }
        }
    }
    private void CreateRopeChain()
    {
        cubes = new Transform[cubeCount];
        cubeRigidbodies = new Rigidbody[cubeCount];
        Rigidbody previousRb = carRigidbody;
        for (int i = 0; i < cubeCount; i++)
        {
            Vector3 spawnPos = transform.position + ropeOriginOffset
                + -transform.forward * segmentLength * i
                + Vector3.up * ropeHeight;
            GameObject newCube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);
            cubes[i] = newCube.transform;
            Rigidbody rb = newCube.AddComponent<Rigidbody>();
            cubeRigidbodies[i] = rb;
            rb.mass = cubeMass;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.linearDamping = 0.5f; // Lower damping so they move more freely
            if (newCube.GetComponent<Collider>() == null)
            {
                newCube.AddComponent<BoxCollider>();
            }
            ConfigurableJoint joint = newCube.AddComponent<ConfigurableJoint>();
            joint.connectedBody = previousRb;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            SoftJointLimit limit = new SoftJointLimit
            {
                limit = segmentLength * 1.5f, // Looser limit to allow more swing
                bounciness = 0f
            };
            joint.linearLimit = limit;
            // Lower spring/damper so gravity wins when standing still
            joint.xDrive = new JointDrive { positionSpring = 10f, positionDamper = 2f, maximumForce = Mathf.Infinity };
            joint.yDrive = new JointDrive { positionSpring = 10f, positionDamper = 2f, maximumForce = Mathf.Infinity };
            joint.zDrive = new JointDrive { positionSpring = 10f, positionDamper = 2f, maximumForce = Mathf.Infinity };
            previousRb = rb;
        }
    }
}