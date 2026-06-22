using System;
using Steamworks;
using UnityEngine;

public class SteamAuthInitializer : MonoBehaviour
{
    public static SteamAuthInitializer Instance { get; private set; }
    public static bool IsAuthenticated { get; private set; } = false;
    public static event Action OnAuthenticated;

#if UNITY_EDITOR
    private const uint AppId = 480;
#else
    private const uint AppId = 4787370;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        Authenticate();
    }

    private static void Authenticate()
    {
        if (IsAuthenticated) return;

        try
        {
            SteamClient.Init(AppId, true);

            if (!SteamClient.IsValid)
            {
                throw new Exception("[Steam Auth] Steam client initialized but returned an invalid status flag.");
            }
            
            IsAuthenticated = true;
            Debug.Log($"[Steam Auth] Steam Authentication Successful! Player Name: {SteamClient.Name}, SteamID: {SteamClient.SteamId}");

            OnAuthenticated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("[Steam Auth] Critical failure initialization! " +
                $"Please ensure the Steam Desktop app is open, you are logged into an active account, Details: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        if (IsAuthenticated)
        {
            SteamClient.Shutdown();
        }
    }
}