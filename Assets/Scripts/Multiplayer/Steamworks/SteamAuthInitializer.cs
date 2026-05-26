using System;
using Steamworks;
using UnityEngine;

public class SteamAuthInitializer : MonoBehaviour
{
    public static bool IsAuthenticated { get; private set; } = false;
    public static event Action OnAuthenticated;

    [SerializeField] private static uint appId = 480;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Authenticate();
    }

    private static void Authenticate()
    {
        if (IsAuthenticated) return;

        try
        {
            SteamClient.Init(appId, true);

            if (!SteamClient.IsValid)
            {
                throw new Exception("Steam client initialized but returned an invalid status flag.");
            }
            
            IsAuthenticated = true;
            Debug.Log($"Steam Authentication Successful! Player Name: {SteamClient.Name}, SteamID: {SteamClient.SteamId}");

            OnAuthenticated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UNKNOWN ERROR] Something else went wrong: {e.GetType().Name} - {e.Message}");
            Debug.LogError("[STEAM ERROR] Critical failure initialization! " +
                $"Please ensure the Steam Desktop app is open, you are logged into an active account, Details: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (IsAuthenticated)
        {
            SteamClient.Shutdown();
        }
    }
}