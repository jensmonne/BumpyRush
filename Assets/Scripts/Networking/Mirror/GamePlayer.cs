using System.Collections.Generic;
using Mirror;
using UnityEngine;
using TMPro;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(HandlePlayerDataChanged))]
    public PlayerNetworkData networkData;

    [Header("Local Only Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private List<MonoBehaviour> localOnlyScripts;
    [SerializeField] private List<GameObject> localOnlyObjects;
    [SerializeField] private TMP_Text nameText;
    private Vector3 originalSpawnPosition;
    
    private Renderer[] characterRenderers;

    private void Start()
    {
        originalSpawnPosition = transform.position;

        characterRenderers = GetComponentsInChildren<Renderer>();

        if (!isLocalPlayer)
        {
            DisableRemoteComponents();
        }
        else
        {
            SetupLocalPlayer();
        }

        UpdateVisuals(networkData);
    }

    private void HandlePlayerDataChanged(PlayerNetworkData oldData, PlayerNetworkData newData)
    {
        UpdateVisuals(newData);
    }

    private void UpdateVisuals(PlayerNetworkData data)
    {
        if (nameText != null) 
        {
            nameText.text = data.playerName;
        }

        if (SkinCustomization.Instance != null && characterRenderers != null)
        {
            Material networkedSkin = SkinCustomization.Instance.GetSkinMaterial(data.skinIndex);
            
            if (networkedSkin != null)
            {
                foreach (var renderer in characterRenderers)
                {
                    if (renderer != null)
                    {
                        if (nameText != null && renderer.gameObject == nameText.gameObject) continue;

                        renderer.material = networkedSkin;
                    }
                }
            }
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
        if (!isLocalPlayer) return;

        CmdUnstuckPlayer();
    }

    [Command]
    private void CmdUnstuckPlayer()
    {
        transform.position = originalSpawnPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        TargetTeleportPlayer(originalSpawnPosition, Quaternion.Euler(0f, 0f, 0f));
    }

    [TargetRpc]
    private void TargetTeleportPlayer(Vector3 targetPosition, Quaternion targetRotation)
    {
        transform.position = targetPosition;
        transform.rotation = targetRotation;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
}