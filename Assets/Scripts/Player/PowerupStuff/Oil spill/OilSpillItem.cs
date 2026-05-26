using UnityEngine;

public class OilSpillItem : MonoBehaviour
{
    [SerializeField] private GameObject oilSpillPrefab;

    private void Start()
    {
        ActivateOilSpill();
    }
    public void ActivateOilSpill()
    {
        Instantiate(oilSpillPrefab, new Vector3(transform.position.x, transform.position.y - .8f, transform.position.z) + Vector3.forward * 3f, Quaternion.identity);
        Destroy(gameObject);
    }


}
