using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> currentItems = new List<GameObject>();
    [SerializeField] private float itemUseCooldown = 0.5f;
    private float lastItemUseTime = -999f;

    public void AddItem(GameObject item)
    {
        currentItems.Add(item);
        Debug.Log("Item added: " + item.name);
    }

    public void UseItem(int index)
    {
        if (Time.time - lastItemUseTime < itemUseCooldown)
        {
            Debug.Log("Item use is on cooldown!");
            return;
        }
        if (index < 0 || index >= currentItems.Count)
        {
            Debug.Log("Invalid item index: " + index);
            return;
        }

        GameObject item = currentItems[index];
        Instantiate(item, transform.position, Quaternion.identity, gameObject.transform);

        Debug.Log("Using item: " + item.name);
        currentItems.RemoveAt(index);
        lastItemUseTime = Time.time;
    }
}
