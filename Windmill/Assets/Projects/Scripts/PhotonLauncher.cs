using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Robust Photon launcher that supports:
/// - Server mode (isServer = true) that tries to remain MasterClient and never sleeps
/// - Client mode that reconnects automatically and doesn't quit on master change
/// - Heartbeat to avoid idle timeouts
/// - Prevent system sleep on Windows
/// - Exponential backoff reconnect
/// 
/// NOTE: Attach a PhotonView to the same GameObject (no need to manually set ViewID).
/// </summary>
public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    public static PhotonLauncher Instance;

    [Header("General")]
    [SerializeField] private string gameVersion = "2.1";
    [SerializeField] public string roomName = "eventRoom";
    [Tooltip("Set true on your PC 'server' build (admin UI).")]
    public bool isServer = false;

    [Header("UI (optional)")]
    public GameObject ServerConfigManager;
    public GameObject ClientConfigManager;
    public Transform contentParent;
    public GameObject playerRoomEntryPrefab;
    public Text InfoVideoText;
    public Text InfoRoleText;
    public Text statusText;
    public Button StartGameButton;
    public Button EndGameButton;
    public GameObject gameObjectToDisableOnStart;
    public GameObject imageToFlash;
    public GameObject playerPanel;

    [Header("Runtime settings")]
    public int maxPlayers = 6;
    public int playerTtlMs = 60000; // 60 seconds: allows short reconnects without removing player
    public float heartbeatInterval = 5f; // seconds — sends a short event to keep connection alive
    public byte heartbeatEventCode = 99;

    // reconnect/backoff
    private bool reconnecting = false;
    private bool joinedRoom = false;

    // player UI list
    public List<PlayerRoom> playerRooms = new List<PlayerRoom>();
    private string localPlayerNickName;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Photon behavior
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        photonView.ViewID = 999;

        // Performance / keepalive tuning
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 15;
        Application.targetFrameRate = 60;

#if UNITY_STANDALONE
        SystemSleepBlocker.BlockSleep(true); // prevents Windows from sleeping / turning off display
#endif
    }

    void Start()
    {
        // Show the UI depending on role
        if (ServerConfigManager != null) ServerConfigManager.SetActive(isServer);
        if (ClientConfigManager != null) ClientConfigManager.SetActive(!isServer);

        // Photon keep alive setting (if available in your PUN version)
        try { PhotonNetwork.KeepAliveInBackground = 86400000; } catch { /* ignore if missing */ }

        Connect();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pause)
    {
        Application.runInBackground = true;
    }

    // -------------------------
    // CONNECTION
    // -------------------------
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            return;
        }

        Debug.Log("[PhotonLauncher] ConnectUsingSettings()");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonLauncher] Connected to Master");

        // Choose a nickname
        if (PlayFabLogin.Instance != null)
            PhotonNetwork.NickName = Mathf.Clamp(PlayFabLogin.Instance.playFabId.Length, 1, 32) > 0
                ? PlayFabLogin.Instance.playFabId.Substring(0, Math.Min(8, PlayFabLogin.Instance.playFabId.Length))
                : "001" + UnityEngine.Random.Range(1000, 9999).ToString();
        else if (!string.IsNullOrEmpty(localPlayerNickName))
            PhotonNetwork.NickName = localPlayerNickName;
        else
        {
            PhotonNetwork.NickName = "001" + UnityEngine.Random.Range(1000, 9999).ToString();
            localPlayerNickName = PhotonNetwork.NickName;
        }

        Debug.Log("[PhotonLauncher] NickName: " + PhotonNetwork.NickName);

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[PhotonLauncher] Joined Lobby — joining/creating room");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonLauncher] JoinRandomFailed: " + message);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers,
            PublishUserId = true,
            PlayerTtl = playerTtlMs
        };

        // If server build: create the room, otherwise wait + retry
        if (isServer)
        {
            PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        }
        else
        {
            if (statusText != null) statusText.text = "Waiting for server to start...";
            Invoke(nameof(Connect), 2f);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonLauncher] CreateRoomFailed: " + message);
        // Try to join random room again (rare race condition)
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonLauncher] Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        joinedRoom = true;
        if (statusText != null) statusText.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;

        // If server, ensure we are master
        if (isServer && !PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }

        // Start heartbeat when in room
        StartCoroutine(HeartbeatLoop());

        // Load local player saved data (UI)
        LoadLocalPlayerData();
    }

    // -------------------------
    // HEARTBEAT (keep connection alive with Photon)
    // -------------------------
    private IEnumerator HeartbeatLoop()
    {
        while (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            try
            {
                // send a very small unreliable event so Photon doesn't idle-timeout the connection
                PhotonNetwork.RaiseEvent(heartbeatEventCode, null, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PhotonLauncher] Heartbeat send failed: " + ex.Message);
            }

            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    // -------------------------
    // DISCONNECT / RECONNECT
    // -------------------------
    public override void OnDisconnected(DisconnectCause cause)
    {
        // Debug.LogError("[PhotonLauncher] Disconnected: " + cause);

        // stop heartbeat
        StopCoroutineSafe(HeartbeatLoop());

        // start reconnect attempts (server will try ReconnectAndRejoin; clients will try Reconnect)
        if (!reconnecting)
            StartCoroutine(ReconnectRoutine(isServer));
    }

    private IEnumerator ReconnectRoutine(bool isServerRoutine)
    {
        reconnecting = true;

        float delay = 1f;
        int attempt = 0;
        const float maxDelay = 30f;
        if (isServerRoutine)
        {
            while (!PhotonNetwork.IsConnected)
            {
                attempt++;
                Debug.Log($"[PhotonLauncher] Reconnect attempt #{attempt}, delay {delay}s");

                try
                {
                    if (isServerRoutine)
                    {
                        // prefer ReconnectAndRejoin (will try to rejoin previous room & keep actor number)
                        PhotonNetwork.ReconnectAndRejoin();
                        StartCoroutine(CheckGameState());
                    }

                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[PhotonLauncher] Reconnect call failed: " + ex.Message);
                }

                if (isServerRoutine)
                {
                    // Wait and check
                    float waited = 0f;
                    while (waited < delay)
                    {
                        if (PhotonNetwork.IsConnected)
                            break;
                        waited += 0.5f;
                        yield return new WaitForSeconds(0.5f);
                    }

                    if (PhotonNetwork.IsConnected)
                        break;

                    // exponential backoff (capped)
                    delay = Mathf.Min(delay * 2f, maxDelay);
                    if (PhotonNetwork.IsConnected)
                        Debug.Log("[PhotonLauncher] Reconnected successfully!");
                    else
                        Debug.LogError("[PhotonLauncher] Reconnect loop ended (connection not reestablished).");

                    reconnecting = false;
                    yield break;
                }
            }

        }
        else
        {
            // clients can try simple reconnect
            yield return new WaitForSeconds(1);
            //SceneManager.LoadScene(0);
            //DestroyImmediate(gameObject);
            //PhotonNetwork.Reconnect();
        }


    }

    // Utility to stop coroutine safely by starting a new enumerator to stop it by name
    private void StopCoroutineSafe(IEnumerator enumerator)
    {
        try { StopCoroutine(enumerator); } catch { /* ignore */ }
    }

    IEnumerator CheckGameState()
    {
        yield return new WaitForSeconds(1);
        if (PhotonNetwork.InRoom)
        {
            //Read game property from photon
            bool isRunning = (bool)PhotonNetwork.CurrentRoom.CustomProperties["GameRunning"];

            if (StartGameButton != null) StartGameButton.gameObject.SetActive(!isRunning);
            if (EndGameButton != null) EndGameButton.gameObject.SetActive(isRunning);

            playerPanel.SetActive(false);


        }
    }

    // -------------------------
    // MASTER SWITCH — do NOT force clients to quit
    // -------------------------
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("[PhotonLauncher] MasterClient switched to: " + (newMasterClient != null ? newMasterClient.NickName : "null"));

        // If this machine is the server build, try to reclaim master if possible
        if (isServer)
        {
            // If server isn't master anymore, attempt to set master back to local player
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Debug.Log("[PhotonLauncher] Server reclaiming master...");
                try
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[PhotonLauncher] Failed to SetMasterClient(): " + ex.Message);
                }
            }
        }
        else
        {
            // clients remain running — do not quit
            Debug.Log("[PhotonLauncher] Client: master changed; staying alive.");
        }
    }

    // -------------------------
    // PLAYER / UI management — adapted from your original code
    // -------------------------
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[PhotonLauncher] Player Entered: " + newPlayer.NickName);

        // Only create UI on the MasterClient (server)
        if (PhotonNetwork.IsMasterClient)
        {
            CreatePlayerRoom(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = playerRooms.FindIndex(x => x.playerID.text == otherPlayer.NickName);
        if (index != -1)
        {
            Destroy(playerRooms[index].gameObject);
            playerRooms.RemoveAt(index);
        }
    }

    public void LoadLocalPlayerData()
    {
        if (InfoRoleText != null)
        {
            InfoRoleText.text = "Device ID: " + PlayerPrefs.GetString($"{PhotonNetwork.LocalPlayer.NickName}_role", "No Role");
        }

        if (InfoVideoText != null)
        {
            InfoVideoText.text = "Video ID: " + PlayerPrefs.GetString($"{PhotonNetwork.LocalPlayer.NickName}_video", "No Video");
        }
    }

    private void CreatePlayerRoom(Player player)
    {
        if (playerRooms.Exists(x => x.playerID.text == player.NickName))
            return;

        if (playerRoomEntryPrefab == null || contentParent == null) return;

        GameObject entryObj = Instantiate(playerRoomEntryPrefab, contentParent);
        PlayerRoom room = entryObj.GetComponent<PlayerRoom>();
        if (room == null) return;

        PlayerRoomData data = new PlayerRoomData
        {
            playerId = player.NickName,
            role = "",
            video = "",
            roleOptions = new List<string> { "Device1", "Device2", "Device3", "Device4", "Device5" },
            videoOptions = new List<string> { "Video1", "Video2", "Video3", "Video4", "Video5" },

            onAssign = OnAssignPlayer,
            onRemove = OnRemovePlayer,
            onFlash = OnFlashPlayer
        };

        room.Initialize(data);
        room.LoadPlayerPrefData();

        playerRooms.Add(room);
    }

    private void OnAssignPlayer(PlayerRoomData data)
    {
        photonView.RPC("RPC_AssignPlayer", RpcTarget.All, data.playerId, data.role, data.video);
    }

    [PunRPC]
    void RPC_AssignPlayer(string playerId, string role, string video)
    {
        if (PhotonNetwork.LocalPlayer.NickName != playerId)
            return;

        PlayerPrefs.SetString($"{playerId}_role", role);
        PlayerPrefs.SetString($"{playerId}_video", video);
        PlayerPrefs.Save();

        LoadLocalPlayerData();
    }

    private void OnRemovePlayer(string playerId)
    {
        PlayerPrefs.SetString($"{playerId}_role", "");
        PlayerPrefs.SetString($"{playerId}_video", "");
        photonView.RPC("RPC_RemovePlayer", RpcTarget.All, playerId);
    }

    [PunRPC]
    void RPC_RemovePlayer(string playerId)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            // Delete all player pref data for role and device
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Keep original behavior: quit app for removed device (if you want different behavior, change here)
            PhotonNetwork.LeaveRoom();
            Application.Quit();
        }
    }

    private void OnFlashPlayer(string playerId)
    {
        photonView.RPC("RPC_FlashDevice", RpcTarget.All, playerId);
    }

    [PunRPC]
    void RPC_FlashDevice(string playerId)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
            StartCoroutine(FlashScreen());
    }

    private IEnumerator FlashScreen()
    {
        if (imageToFlash == null) yield break;
        imageToFlash.SetActive(true);
        yield return new WaitForSeconds(2f);
        imageToFlash.SetActive(false);
    }

    // === GAME FLOW ===
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "GameRunning", true } }
        );
        if (!PhotonNetwork.InRoom)
            return;
        PhotonNetwork.LoadLevel("VideoScene");
        //PhotonNetwork.LoadLevel("MainMenu");
        photonView.RPC("RPC_StartGameAll", RpcTarget.All);
        playerPanel.SetActive(false);

        if (StartGameButton != null) StartGameButton.gameObject.SetActive(false);
        if (EndGameButton != null) EndGameButton.gameObject.SetActive(true);
    }

    [PunRPC]
    void RPC_StartGameAll()
    {
        if (!isServer)
        {
            StartCoroutine(StartGameRoutine());
        }
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.02f);
        if (gameObjectToDisableOnStart != null) gameObjectToDisableOnStart.SetActive(false);
    }

    public void EndGame()
    {
        if (!isServer) return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "GameRunning", false } }
        );

        photonView.RPC("RPC_RestartAll", RpcTarget.All);
        if (playerPanel != null) playerPanel.SetActive(true);
        if (StartGameButton != null) StartGameButton.gameObject.SetActive(true);
        if (EndGameButton != null) EndGameButton.gameObject.SetActive(false);
    }

    [PunRPC]
    void RPC_RestartAll()
    {
        StartCoroutine(RestartConnectionRoutine());
    }

    private IEnumerator RestartConnectionRoutine()
    {
        Debug.Log("[PhotonLauncher] Restarting connection...");

        // Clients: fully disconnect then reload ServerConfig scene and reconnect
        if (!isServer)
        {
            PhotonNetwork.Disconnect();

            // Wait until fully disconnected
            while (PhotonNetwork.IsConnected || PhotonNetwork.IsConnectedAndReady)
                yield return null;
                

            // Load ServerConfig scene
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(0);
            while (!loadOp.isDone)
                yield return null;
                DestroyImmediate(gameObject);

            // small wait
            yield return null;

            // Re-enable UI only for clients
            if (!isServer && gameObjectToDisableOnStart != null)
                gameObjectToDisableOnStart.SetActive(true);

            // Reconnect
            yield return new WaitForSeconds(0.2f);
            //Connect();
        }
        else
        {
            // server just loads scene
            Debug.Log("check this");
            SceneManager.LoadSceneAsync("ServerConfig");
        }
    }
}
