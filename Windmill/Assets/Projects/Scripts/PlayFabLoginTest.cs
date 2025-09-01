using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLoginTest : MonoBehaviour
{
    // Replace with your Title ID from PlayFab Game Manager
    [SerializeField] private string playFabTitleId = "YOUR_TITLE_ID";

    void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = playFabTitleId;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier, // or any unique ID
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ PlayFab login successful! Player ID: " + result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("❌ PlayFab login failed: " + error.GenerateErrorReport());
    }
}
