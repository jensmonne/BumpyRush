using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityAuthInitializer : MonoBehaviour
{
    public static bool IsAuthenticated { get; private set; } = false;
    private static bool isAuthenticating = false;

    public static event Action OnAuthenticated;

    private async void Awake()
    {
        await Authenticate();
    }

    private static async Task Authenticate()
    {
        if (IsAuthenticated || isAuthenticating) return;

        isAuthenticating = true;

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                // Debug.Log("Unity Services Initialized");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                // Debug.Log($"Signed in! PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            
            IsAuthenticated = true;

            Debug.Log("Unity Services Authentication Successful");

            OnAuthenticated?.Invoke();
        }
        catch (RequestFailedException e) when (e.ErrorCode == CommonErrorCodes.TransportError)
        {
            Debug.LogWarning($"[OFFLINE MODE] You are not connected to the internet. Multiplayer is disabled. Details: {e.Message}");
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"[SERVICE ERROR] Unity Authentication failed (Status: {e.ErrorCode}). Your Project ID might be wrong or the service is down. Details: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UNKNOWN ERROR] Something else went wrong: {e.GetType().Name} - {e.Message}");
        }
        finally
        {
            isAuthenticating = false;
        }
    }
}