using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Data")]
    public string PlayerName = "Player";

    [Header("Local Only Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private List<MonoBehaviour> localOnlyScripts;
    [SerializeField] private List<GameObject> localOnlyObjects;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            DisableRemoteComponents();
        }
        else
        {
            SetupLocalPlayer();
        }
    }

    private void DisableRemoteComponents()
    {
        if (playerRigidbody != null) playerRigidbody.isKinematic = true;
        foreach (var obj in localOnlyObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
        foreach (var script in localOnlyScripts)
        {
            if (script != null) script.enabled = false;
        }
    }

    private void SetupLocalPlayer()
    {
        Debug.Log($"Configuring local controls for {PlayerName}");
        
        if (playerRigidbody != null) playerRigidbody.isKinematic = false;
        foreach (var obj in localOnlyObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        foreach (var script in localOnlyScripts)
        {
            if (script != null) script.enabled = true;
        }
    }
}
