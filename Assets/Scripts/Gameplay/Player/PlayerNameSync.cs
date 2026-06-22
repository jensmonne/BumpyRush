using Mirror;
using UnityEngine;
public class PlayerNameSync : NetworkBehaviour
{
    [SerializeField] private string playerPrefsKey = "PlayerName";
    [SyncVar] public string steamName;
    public override void OnStartLocalPlayer()
    {
        CmdSetName(PlayerPrefs.GetString(playerPrefsKey, ""));
    }
    [Command]
    private void CmdSetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = $"Player {netId}";
        steamName = name;
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayerName(netId, name);
    }
}