using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RopeWithBears : NetworkBehaviour
{
    [Header("Bear Settings")]
    [SerializeField] private GameObject bearPrefab;
    [SerializeField] private int initialBearCount = 0;

    [Header("Chain Settings")]
    [SerializeField] private float segmentLength = 1f;
    [SerializeField] private float ropeHeight = 1f;
    [SerializeField] private Vector3 ropeOriginOffset = new Vector3(0, 0, -3f);
    [SerializeField] private float recordDistance = 0.1f;
    [SerializeField] private float teleportThreshold = 5f;

    [SyncVar(hook = nameof(OnBearCountChanged))]
    private int bearCount;

    private readonly List<GameObject> bears = new List<GameObject>();
    private readonly List<Vector3> path = new List<Vector3>(); // [0] = most recent committed point

    private Vector3 AnchorPos => transform.TransformPoint(ropeOriginOffset + Vector3.up * ropeHeight);

    public override void OnStartServer()
    {
        bearCount = Mathf.Max(0, initialBearCount);
    }

    public override void OnStartClient()
    {
        SeedPath();
        UpdateBearVisuals();
    }

    public override void OnStopClient() => ClearBearVisuals();
    private void OnDestroy() => ClearBearVisuals();

    private void OnBearCountChanged(int oldCount, int newCount) => UpdateBearVisuals();

    private void LateUpdate()
    {
        if (bears.Count == 0) return;
        if (path.Count == 0) SeedPath();

        Vector3 anchor = AnchorPos;

        if (Vector3.Distance(anchor, path[0]) > teleportThreshold)
            SeedPath();
        else if (Vector3.Distance(anchor, path[0]) >= recordDistance)
        {
            path.Insert(0, anchor);
            TrimPath();
        }

        for (int i = 0; i < bears.Count; i++)
        {
            if (bears[i] == null) continue;

            Vector3 target = SamplePath((i + 1) * segmentLength);
            Vector3 leader = (i == 0) ? anchor : bears[i - 1].transform.position;

            bears[i].transform.position = target;

            Vector3 look = leader - target;
            if (look.sqrMagnitude > 0.0001f)
                bears[i].transform.rotation = Quaternion.LookRotation(look);
        }
    }

    private Vector3 SamplePath(float distance)
    {
        Vector3 prev = AnchorPos;
        float covered = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 cur = path[i];
            float seg = Vector3.Distance(prev, cur);
            if (seg > 0.0001f)
            {
                if (covered + seg >= distance)
                    return Vector3.Lerp(prev, cur, (distance - covered) / seg);
                covered += seg;
            }
            prev = cur;
        }

        Vector3 dir = path.Count >= 2 ? (path[path.Count - 1] - path[path.Count - 2]).normalized : -transform.forward;
        if (dir == Vector3.zero) dir = -transform.forward;
        return prev + dir * (distance - covered);
    }

    private void SeedPath()
    {
        path.Clear();
        Vector3 a = AnchorPos;
        Vector3 back = -transform.forward;
        if (back == Vector3.zero) back = Vector3.back;

        int pts = Mathf.CeilToInt(((bearCount + 2) * segmentLength) / Mathf.Max(0.01f, recordDistance)) + 1;
        for (int i = 1; i <= pts; i++)
            path.Add(a + back * recordDistance * i);
    }

    private void TrimPath()
    {
        float maxDist = (bearCount + 2) * segmentLength;
        Vector3 prev = AnchorPos;
        float covered = 0f;
        int keep = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            covered += Vector3.Distance(prev, path[i]);
            prev = path[i];
            if (covered > maxDist) { keep = i + 1; break; }
        }

        if (keep < path.Count)
            path.RemoveRange(keep, path.Count - keep);
    }

    private void UpdateBearVisuals()
    {
        if (bearPrefab == null) return;
        if (path.Count == 0) SeedPath();

        while (bears.Count < bearCount) SpawnBearVisual();
        while (bears.Count > bearCount) DespawnLastBearVisual();
    }

    private void SpawnBearVisual()
    {
        Vector3 pos = SamplePath((bears.Count + 1) * segmentLength);
        GameObject bear = Instantiate(bearPrefab, pos, Quaternion.identity);

        Rigidbody rb = bear.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        foreach (Collider c in bear.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        bears.Add(bear);
    }

    private void DespawnLastBearVisual()
    {
        int last = bears.Count - 1;
        if (bears[last] != null) Destroy(bears[last]);
        bears.RemoveAt(last);
    }

    private void ClearBearVisuals()
    {
        foreach (GameObject b in bears)
            if (b != null) Destroy(b);
        bears.Clear();
    }

    [Server] public void AddBear() => bearCount++;
    [Server] public void RemoveBear() => bearCount = Mathf.Max(0, bearCount - 1);
    [Server] public void ClearBears() => bearCount = 0;
    [Server] public void SetBearCount(int n) => bearCount = Mathf.Max(0, n);

    public int BearCount => bearCount;
}