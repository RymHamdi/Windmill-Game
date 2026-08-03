using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin Instance;
    public bool isConnected;
    public string uniqueId;

    public string playFabId;

    bool checkStateCompleted;

    private bool loginInProgress;
    private const float LoginTimeoutSeconds = 15f;
    private Coroutine loginWatchdog;

    public bool isServer;

    public GameStateConfigManager gameStateConfigManager;

    void Awake()
    {
        // Singleton pattern, persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //gameState.lastGameState = "End";
        checkStateCompleted = true;

        // Don't rely solely on OnSceneLoaded: SceneManager.sceneLoaded is not raised for the
        // very first scene a build boots into, so on a fresh app launch login could otherwise
        // never be attempted at all, leaving the client stuck here indefinitely.
        if (!isConnected)
        {
            LoginToPlayFab();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            CancelInvoke(nameof(LoadAfterwhile));

            if (!isConnected)
            {
                LoginToPlayFab();
            }
            else
            {
                if (isServer)
                {
                    Invoke("LoadAfterwhile", 1f);
                }
                else
                {
                    Invoke("LoadAfterwhile", 1f);
                }
                
            }

        }
    }

    private void LoadAfterwhile()
    {
        SceneManager.LoadScene("ServerConfig");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✅ Login with a custom ID
    void LoginToPlayFab()
    {
        if (isConnected || loginInProgress)
        {
            return;
        }

        loginInProgress = true;

        if (loginWatchdog != null)
        {
            StopCoroutine(loginWatchdog);
        }
        loginWatchdog = StartCoroutine(LoginWatchdog());

        // Choose CustomId: use `uniqueId` when running as server, otherwise device unique id
        string customIdToUse = SystemInfo.deviceUniqueIdentifier + Random.Range(0, 10000); // Add random suffix to avoid collisions in testing
        if (isServer)
        {
            if (!string.IsNullOrEmpty(uniqueId))
            {
                customIdToUse = uniqueId;
            }
            else
            {
                Debug.LogWarning("isServer is true but uniqueId is empty. Falling back to deviceUniqueIdentifier.");
            }
        }

        Debug.Log($"Logging in to PlayFab with CustomId: {customIdToUse} (isServer={isServer})");

        var request = new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId, // Set in PlayFabSettings scriptable object
            CustomId = customIdToUse, // chosen unique id
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                loginInProgress = false;
                if (loginWatchdog != null)
                {
                    StopCoroutine(loginWatchdog);
                    loginWatchdog = null;
                }

                isConnected = true;
                Debug.Log("✅ PlayFab Login Success: " + result.PlayFabId);
                playFabId = result.PlayFabId;
                SceneManager.LoadScene("ServerConfig");
                // Start checking game state
                //InvokeRepeating(nameof(CheckGameState), 2f, 1f); // every 5 seconds
            },
            error =>
            {
                loginInProgress = false;
                if (loginWatchdog != null)
                {
                    StopCoroutine(loginWatchdog);
                    loginWatchdog = null;
                }

                Debug.LogError("❌ PlayFab Login Failed: " + error.GenerateErrorReport());
                Invoke(nameof(LoginToPlayFab), 3f);
            });
    }

    private IEnumerator LoginWatchdog()
    {
        yield return new WaitForSeconds(LoginTimeoutSeconds);

        if (loginInProgress && !isConnected)
        {
            Debug.LogWarning($"[PlayFabLogin] Login timed out after {LoginTimeoutSeconds}s with no response - retrying.");
            loginInProgress = false;
            loginWatchdog = null;
            LoginToPlayFab();
        }
    }

    float timeToCheck = 2;
    float currentTime;

    private void Update()
    {
        if (Time.time > currentTime + timeToCheck && PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient && checkStateCompleted)
        {
            //CheckGameState();
            currentTime = Time.time;
        }
    }

    // ✅ Poll GameState from PlayFab
    void CheckGameState()
    {
        checkStateCompleted = false;
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey("GameState"))
                {

                    foreach (var kvp in result.Data)
                    {
                        Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
                    }
                    string state = result.Data["GameState"];
                    string stateTime = result.Data.ContainsKey("GameStateTime") ? result.Data["GameStateTime"] : "";

                    checkStateCompleted = true;

                    GameStateConfig gameStateConfig = new GameStateConfig();
                    gameStateConfig.GameState = state;
                    gameStateConfig.GameStateTime = stateTime;
                    if (!gameStateConfigManager.Contains(gameStateConfig))
                    {
                        gameStateConfigManager.Add(gameStateConfig);
                        Debug.Log("🔄 GameState changed to " + state + " at " + stateTime);
                        //OnGameStateChanged(state);
                    }
                }
            },
            error =>
            {
                Debug.LogError("Error getting GameState: " + error.GenerateErrorReport());
                checkStateCompleted = true;
            });
    }

    // ✅ React to GameState changes
    private float lastStateChangeTime = 0f;
    private float stateChangeCooldown = 1f; // seconds

    /*void OnGameStateChanged(string state)
    {
        if (Time.time - lastStateChangeTime < stateChangeCooldown)
        {
            Debug.Log("⏳ Ignored state change due to cooldown: " + state);
            return;
        }

        lastStateChangeTime = Time.time;

        if (state == "Start")
        {
            Debug.Log("🚀 Game Started! Do something here...");
            var photonLauncher = FindAnyObjectByType<PhotonLauncher>();
            if (photonLauncher != null)
            {
                photonLauncher.StartMainMenu();
            }
        }
        else if (state == "End")
        {
            Debug.Log("🛑 Game Ended! Do something here...");
            if (MasterDisconnectAll.Instance != null)
            {
                MasterDisconnectAll.Instance.OnMasterDisconnectClicked();
            }
        }
    }*/

}

[System.Serializable]

public class GameStateConfig
{
    public string GameState;
    public string GameStateTime;
}

[System.Serializable]
public class GameStateConfigManager
{
    public List<GameStateConfig> gameStateConfigs;

    public bool Contains(GameStateConfig config)
    {
        return gameStateConfigs.Exists(c => c.GameState == config.GameState && c.GameStateTime == config.GameStateTime);
    }

    public void Add(GameStateConfig config)
    {
        if (!Contains(config))
        {
            gameStateConfigs.Add(config);
        }
    }
}
