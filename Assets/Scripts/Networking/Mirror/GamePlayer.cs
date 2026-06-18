using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandleNameChanged))]
    public string PlayerName = "Player";

    [Header("Local Only Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private List<MonoBehaviour> localOnlyScripts;
    [SerializeField] private List<GameObject> localOnlyObjects;
    [SerializeField] private TMP_Text nameText;

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

    private void HandleNameChanged(string oldName, string newName)
    {
        if (nameText != null) 
        {
            nameText.text = newName;
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

        if (nameText != null) nameText.gameObject.SetActive(true);
    }

    private void SetupLocalPlayer()
    {        
        if (playerRigidbody != null) playerRigidbody.isKinematic = false;
        foreach (var obj in localOnlyObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        foreach (var script in localOnlyScripts)
        {
            if (script != null) script.enabled = true;
        }

        if (nameText != null) nameText.gameObject.SetActive(false);
    }

    public void UnstuckPlayer()
    {
        transform.position = new Vector3(13f, 10f, -100f);
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}