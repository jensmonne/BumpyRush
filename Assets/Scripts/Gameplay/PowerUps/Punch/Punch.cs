using UnityEngine;

public class Punch : MonoBehaviour
{
    [SerializeField] private GameObject fist;
    private GameObject spawnedFist;

    [Header("Fist Movement")]
    [SerializeField] private float spawnOffset = 2f;

    [Header("Cooldown")]
    [SerializeField] private float punchCooldown = 0.5f;
    private float lastPunchTime = -999f;

    public bool CanPunch => Time.time - lastPunchTime >= punchCooldown;

    private void Start()
    {
        PunchAttack();
    }

    public void PunchAttack()
    {
        if (!CanPunch)
        {
            Debug.Log("Punch is on cooldown!");
            return;
        }
        lastPunchTime = Time.time;
        Vector3 spawnPos = transform.position + (-transform.forward * spawnOffset);
        spawnedFist = Instantiate(fist, spawnPos, Quaternion.identity);
        spawnedFist.transform.rotation = Quaternion.LookRotation(-transform.forward);
    }
}
