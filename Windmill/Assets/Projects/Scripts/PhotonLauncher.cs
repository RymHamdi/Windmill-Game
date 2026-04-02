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
    public Button startButton;


    [Header("Runtime settings")]
    public int maxPlayers = 6;
    public int playerTtlMs = 60000; // 60 seconds: allows short reconnects without removing player
    public float heartbeatInterval = 5f; // seconds — sends a short event to keep connection alive
    public byte heartbeatEventCode = 99;

    public List<string> PlayerColorIds;

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
                //PhotonNetwork.NickName = "001" + UnityEngine.Random.Range(1000, 9999).ToString();

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

    void Update()
    {
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount > 1;
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
            // Create UI entries for existing players
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.IsLocal == false)
                {
                    CreatePlayerRoom(p);
                }
                
            }
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
        Debug.LogWarning($"[PhotonLauncher] Disconnected: {cause}");

        // stop heartbeat
        StopCoroutineSafe(HeartbeatLoop());
        
        joinedRoom = false;

        // start reconnect attempts
        if (!reconnecting)
        {
            if (isServer && !isForcedend)
            {
                StartCoroutine(ServerReconnectRoutine());
            }
            else
            {
                if (!isForcedend)
                {
                    StartCoroutine(ClientReconnectRoutine());
                }
                
            }
        }
    }

    private IEnumerator ServerReconnectRoutine()
    {
        reconnecting = true;

        float delay = 2f;
        int attempt = 0;
        const float maxDelay = 30f;

        Debug.Log("[PhotonLauncher] SERVER DISCONNECTED - Starting auto-reconnect...");
        if (statusText != null) statusText.text = "Server disconnected - reconnecting...";

        while (!PhotonNetwork.IsConnected || !joinedRoom)
        {
            attempt++;
            Debug.Log($"[PhotonLauncher] === SERVER RECONNECT ATTEMPT #{attempt} ===");
            
            // First, ensure we're fully disconnected
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("[PhotonLauncher] Disconnecting first...");
                PhotonNetwork.Disconnect();
                yield return new WaitForSeconds(1f);
            }

            Debug.Log($"[PhotonLauncher] Attempting to connect... (will wait {delay}s)");
            if (statusText != null) statusText.text = $"Reconnecting... (attempt {attempt})";

            try
            {
                // Try to connect to Photon
                PhotonNetwork.ConnectUsingSettings();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotonLauncher] ConnectUsingSettings failed: {ex.Message}");
            }

            // Wait for the delay period, checking connection status
            float waited = 0f;
            while (waited < delay)
            {
                // Check if we're connected AND in room
                if (PhotonNetwork.IsConnected && joinedRoom)
                {
                    Debug.Log("[PhotonLauncher] ✓ SERVER RECONNECTED SUCCESSFULLY!");
                    if (statusText != null) statusText.text = "Server reconnected!";
                    
                    // Restart heartbeat
                    StartCoroutine(HeartbeatLoop());
                    
                    // Check game state
                    StartCoroutine(CheckGameState());
                    
                    reconnecting = false;
                    yield break;
                }
                
                waited += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            // Not connected yet - increase delay with exponential backoff
            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogWarning($"[PhotonLauncher] Still not connected after {delay}s");
            }
            else if (!joinedRoom)
            {
                Debug.LogWarning($"[PhotonLauncher] Connected but not in room yet");
            }
            
            delay = Mathf.Min(delay * 1.5f, maxDelay);
        }

        Debug.Log("[PhotonLauncher] ✓ Reconnection complete!");
        reconnecting = false;
    }

    private IEnumerator ClientReconnectRoutine()
    {
        reconnecting = true;

        float delay = 2f;
        int attempt = 0;
        const float maxDelay = 30f;

        Debug.Log("[PhotonLauncher] CLIENT DISCONNECTED - Starting auto-reconnect...");
        if (statusText != null) statusText.text = "Disconnected - reconnecting...";

        while (!PhotonNetwork.IsConnected || !joinedRoom)
        {
            attempt++;
            Debug.Log($"[PhotonLauncher] === CLIENT RECONNECT ATTEMPT #{attempt} ===");
            
            // First, ensure we're fully disconnected
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("[PhotonLauncher] Disconnecting first...");
                PhotonNetwork.Disconnect();
                yield return new WaitForSeconds(1f);
            }

            Debug.Log($"[PhotonLauncher] Attempting to connect... (will wait {delay}s)");
            if (statusText != null) statusText.text = $"Reconnecting... (attempt {attempt})";

            try
            {
                // Try to connect to Photon
                PhotonNetwork.ConnectUsingSettings();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhotonLauncher] ConnectUsingSettings failed: {ex.Message}");
            }

            // Wait for the delay period, checking connection status
            float waited = 0f;
            while (waited < delay)
            {
                // Check if we're connected AND in room
                if (PhotonNetwork.IsConnected && joinedRoom)
                {
                    Debug.Log("[PhotonLauncher] ✓ CLIENT RECONNECTED SUCCESSFULLY!");
                    if (statusText != null) statusText.text = "Reconnected!";
                    
                    // Restart heartbeat
                    StartCoroutine(HeartbeatLoop());
                    
                    // Reload local player data
                    LoadLocalPlayerData();
                    
                    reconnecting = false;
                    yield break;
                }
                
                waited += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            // Not connected yet - increase delay with exponential backoff
            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogWarning($"[PhotonLauncher] Still not connected after {delay}s");
            }
            else if (!joinedRoom)
            {
                Debug.LogWarning($"[PhotonLauncher] Connected but not in room yet");
            }
            
            delay = Mathf.Min(delay * 1.5f, maxDelay);
        }

        Debug.Log("[PhotonLauncher] ✓ Client reconnection complete!");
        reconnecting = false;
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
        //PhotonNetwork.LoadLevel("VideoScene2");
        photonView.RPC("RPC_StartGameAll", RpcTarget.All);
        //Get Player List without the master Client
        List<Player> nonMasterPlayers = new List<Player>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (!PhotonNetwork.PlayerList[i].IsMasterClient)
            {
                nonMasterPlayers.Add(PhotonNetwork.PlayerList[i]);
            }
            
        }
        for(int i =0; i< nonMasterPlayers.Count; i++)
        {
            if (!nonMasterPlayers[i].IsMasterClient)
            {
                //photonView.RPC("AssignColorId", RpcTarget.AllBuffered, nonMasterPlayers[i].NickName, i);
            }
            

        }
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

    [PunRPC]
    void AssignColorId(string playerId, int colorId)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            //Update Custom propertie of ColorID of the player
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "CharacterId", colorId }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
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

    private bool isForcedend;

    [PunRPC]
    void RPC_RestartAll()
    {
        isForcedend = true;
        StartCoroutine(RestartConnectionRoutine());

    }

    private IEnumerator RestartConnectionRoutine()
    {
        Debug.Log("[PhotonLauncher] Restarting connection...");

        // Clients: fully disconnect then reload ServerConfig scene and reconnect
        if (!isServer)
        {
            yield return new WaitForSeconds(1);
            PhotonNetwork.LeaveRoom();
            yield return new WaitForSeconds(1);
            PhotonNetwork.Disconnect();

            // Wait until fully disconnected
            while (PhotonNetwork.IsConnected || PhotonNetwork.IsConnectedAndReady)
                yield return null;
                

            // Load ServerConfig scene
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(0);
            isForcedend = false;
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
            //Debug.Log("check this");
            //SceneManager.LoadSceneAsync("ServerConfig");
            isForcedend = true;
            yield return new WaitForSeconds(1.5f);
            PhotonNetwork.Disconnect();

            // Wait until fully disconnected
            while (PhotonNetwork.IsConnected || PhotonNetwork.IsConnectedAndReady)
                yield return null;
                DestroyImmediate(gameObject);

            // Load ServerConfig scene
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(0);
            
            /*while (!loadOp.isDone)
                yield return null;*/
                
            
        }
    }
}
