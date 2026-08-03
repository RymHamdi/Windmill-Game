using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Photon bootstrap for both the event server build and the player builds.
/// It keeps one persistent launcher alive during a round and rebuilds it cleanly
/// when the app intentionally returns to the login/config scenes.
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
    public int playerTtlMs = 60000; // allows temporary reconnects during a round
    public int emptyRoomTtlMs = 0; // destroy empty rooms immediately between rounds
    public float heartbeatInterval = 5f;
    public byte heartbeatEventCode = 99;

    public List<string> PlayerColorIds;

    private bool reconnecting;
    private bool joinedRoom;
    private bool isForcedend;
    private Coroutine heartbeatCoroutine;

    public List<PlayerRoom> playerRooms = new List<PlayerRoom>();
    private string localPlayerNickName;
    private string lastRestartToken;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        // This object is created locally from a prefab, so it needs a stable view id for RPCs.
        photonView.ViewID = 999;

        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 15;
        Application.targetFrameRate = 30;

#if UNITY_STANDALONE
        SystemSleepBlocker.BlockSleep(true);
#endif
    }

    void Start()
    {
        if (ServerConfigManager != null) ServerConfigManager.SetActive(isServer);
        if (ClientConfigManager != null) ClientConfigManager.SetActive(!isServer);

        try
        {
            PhotonNetwork.KeepAliveInBackground = 86400000;
        }
        catch
        {
            // Older PUN versions do not expose this property.
        }

        Connect();
    }

    private void OnDestroy()
    {
        StopHeartbeatLoop();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Application.runInBackground = true;
    }

    void OnApplicationPause(bool pause)
    {
        Application.runInBackground = true;
    }

    public void Connect()
    {
        if (PhotonNetwork.InRoom)
        {
            joinedRoom = true;
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinConfiguredRoom();
            return;
        }

        ClientState state = PhotonNetwork.NetworkClientState;
        if (state != ClientState.Disconnected && state != ClientState.PeerCreated)
        {
            Debug.Log($"[PhotonLauncher] Connect skipped while Photon is in state: {state}");
            return;
        }

        ConfigureIdentity();

        Debug.Log("[PhotonLauncher] ConnectUsingSettings()");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonLauncher] Connected to Master");

        ConfigureIdentity();

        Debug.Log("[PhotonLauncher] NickName: " + PhotonNetwork.NickName);
        JoinConfiguredRoom();
    }

    public override void OnJoinedLobby()
    {
        // Fallback only. The main flow joins the fixed room directly from master.
        Debug.Log("[PhotonLauncher] Joined Lobby - joining configured room");
        JoinConfiguredRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonLauncher] JoinRandomFailed: " + message);
        RetryJoinConfiguredRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("[PhotonLauncher] CreateRoomFailed: " + message);
        RetryJoinConfiguredRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[PhotonLauncher] JoinRoomFailed ({GetConfiguredRoomName()}): {message}");
        RetryJoinConfiguredRoom();
    }

    void Update()
    {
        if (startButton != null)
        {
            startButton.interactable = PhotonNetwork.IsMasterClient
                && PhotonNetwork.CurrentRoom != null
                && PhotonNetwork.CurrentRoom.PlayerCount > 1;
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonLauncher] Joined Room: " + PhotonNetwork.CurrentRoom.Name);

        joinedRoom = true;
        reconnecting = false;
        CancelInvoke(nameof(Connect));

        if (statusText != null)
        {
            statusText.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;
        }

        lastRestartToken = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("RestartToken", out object restartTokenObj)
            ? restartTokenObj?.ToString()
            : null;

        if (isServer && !PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }

        if (isServer)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.IsLocal)
                {
                    CreatePlayerRoom(player);
                }
            }
        }

        StartHeartbeatLoop();
        LoadLocalPlayerData();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnRoomPropertiesUpdate(changedProps);

        if (!changedProps.TryGetValue("RestartToken", out object restartTokenObj))
        {
            return;
        }

        string restartToken = restartTokenObj?.ToString();
        if (string.IsNullOrWhiteSpace(restartToken) || restartToken == lastRestartToken)
        {
            return;
        }

        lastRestartToken = restartToken;
        Debug.Log($"[PhotonLauncher] Restart token received: {restartToken}");
        BeginForcedRestart();
    }

    public override void OnLeftRoom()
    {
        joinedRoom = false;
        StopHeartbeatLoop();
    }

    private void StartHeartbeatLoop()
    {
        StopHeartbeatLoop();
        heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
    }

    private void StopHeartbeatLoop()
    {
        if (heartbeatCoroutine == null)
        {
            return;
        }

        StopCoroutine(heartbeatCoroutine);
        heartbeatCoroutine = null;
    }

    private IEnumerator HeartbeatLoop()
    {
        while (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            try
            {
                PhotonNetwork.RaiseEvent(
                    heartbeatEventCode,
                    null,
                    new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                    new ExitGames.Client.Photon.SendOptions { Reliability = false }
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[PhotonLauncher] Heartbeat send failed: " + ex.Message);
            }

            yield return new WaitForSeconds(heartbeatInterval);
        }

        heartbeatCoroutine = null;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonLauncher] Disconnected: {cause}");

        StopHeartbeatLoop();
        joinedRoom = false;

        if (reconnecting || isForcedend)
        {
            return;
        }

        StartCoroutine(isServer ? ServerReconnectRoutine() : ClientReconnectRoutine());
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
            if (statusText != null) statusText.text = $"Reconnecting... (attempt {attempt})";

            TryRecoverConnection();

            float waited = 0f;
            while (waited < delay)
            {
                if (PhotonNetwork.IsConnected && joinedRoom)
                {
                    Debug.Log("[PhotonLauncher] Server reconnected successfully");
                    if (statusText != null) statusText.text = "Server reconnected!";
                    StartCoroutine(CheckGameState());
                    reconnecting = false;
                    yield break;
                }

                TryRecoverConnection();
                waited += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogWarning($"[PhotonLauncher] Still not connected after {delay}s");
            }
            else if (!joinedRoom)
            {
                Debug.LogWarning("[PhotonLauncher] Connected but not in room yet");
            }

            delay = Mathf.Min(delay * 1.5f, maxDelay);
        }

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
            if (statusText != null) statusText.text = $"Reconnecting... (attempt {attempt})";

            TryRecoverConnection();

            float waited = 0f;
            while (waited < delay)
            {
                if (PhotonNetwork.IsConnected && joinedRoom)
                {
                    Debug.Log("[PhotonLauncher] Client reconnected successfully");
                    if (statusText != null) statusText.text = "Reconnected!";
                    LoadLocalPlayerData();
                    reconnecting = false;
                    yield break;
                }

                TryRecoverConnection();
                waited += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }

            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogWarning($"[PhotonLauncher] Still not connected after {delay}s");
            }
            else if (!joinedRoom)
            {
                Debug.LogWarning("[PhotonLauncher] Connected but not in room yet");
            }

            delay = Mathf.Min(delay * 1.5f, maxDelay);
        }

        reconnecting = false;
    }

    IEnumerator CheckGameState()
    {
        yield return new WaitForSeconds(1f);

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            yield break;
        }

        bool isRunning = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameRunning", out object gameRunningObj)
            && gameRunningObj is bool running
            && running;

        if (StartGameButton != null) StartGameButton.gameObject.SetActive(!isRunning);
        if (EndGameButton != null) EndGameButton.gameObject.SetActive(isRunning);
        if (playerPanel != null) playerPanel.SetActive(false);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //let check before the switch master if the game is runing true
        bool isRunning = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameRunning", out object gameRunningObj)            && gameRunningObj is bool running
            && running;
        if (isForcedend)
        {
            Debug.Log("[PhotonLauncher] Ignoring master switch because forced restart is in progress.");
            return;
        }
        if (!isRunning)
        {
            return;
        }

        Debug.Log("[PhotonLauncher] MasterClient switched to: " + (newMasterClient != null ? newMasterClient.NickName : "null"));

        if (!isServer)
        {
            Debug.Log("[PhotonLauncher] Client: master changed; staying alive.");
            return;
        }

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[PhotonLauncher] Player Entered: " + newPlayer.NickName);

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
        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

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

        if (playerRoomEntryPrefab == null || contentParent == null)
            return;

        GameObject entryObj = Instantiate(playerRoomEntryPrefab, contentParent);
        PlayerRoom room = entryObj.GetComponent<PlayerRoom>();
        if (room == null)
            return;

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
        if (PhotonNetwork.LocalPlayer.NickName != playerId)
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(false);
        }

        Application.Quit();
    }

    private void OnFlashPlayer(string playerId)
    {
        photonView.RPC("RPC_FlashDevice", RpcTarget.All, playerId);
    }

    [PunRPC]
    void RPC_FlashDevice(string playerId)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            StartCoroutine(FlashScreen());
        }
    }

    private IEnumerator FlashScreen()
    {
        if (imageToFlash == null)
            yield break;

        imageToFlash.SetActive(true);
        yield return new WaitForSeconds(2f);
        imageToFlash.SetActive(false);
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "GameRunning", true } }
        );

        if (!PhotonNetwork.InRoom)
            return;

        PhotonNetwork.LoadLevel("VideoScene");
        photonView.RPC("RPC_StartGameAll", RpcTarget.All);

        List<Player> nonMasterPlayers = new List<Player>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (!PhotonNetwork.PlayerList[i].IsMasterClient)
            {
                nonMasterPlayers.Add(PhotonNetwork.PlayerList[i]);
            }
        }

        for (int i = 0; i < nonMasterPlayers.Count; i++)
        {
            if (!nonMasterPlayers[i].IsMasterClient)
            {
                // photonView.RPC("AssignColorId", RpcTarget.AllBuffered, nonMasterPlayers[i].NickName, i);
            }
        }

        if (playerPanel != null) playerPanel.SetActive(false);
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
        if (PhotonNetwork.LocalPlayer.NickName != playerId)
            return;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "CharacterId", colorId }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.02f);
        if (gameObjectToDisableOnStart != null) gameObjectToDisableOnStart.SetActive(false);
    }

    

    public void EndGame()
    {
        if (!isServer || PhotonNetwork.CurrentRoom == null)
            return;

        string restartToken = DateTime.UtcNow.Ticks.ToString();

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable
            {
                { "GameRunning", false },
                { "RestartToken", restartToken }
            }
        );

        lastRestartToken = restartToken;
        StartCoroutine(EndGameRoutine());

        if (playerPanel != null) playerPanel.SetActive(true);
        if (StartGameButton != null) StartGameButton.gameObject.SetActive(true);
        if (EndGameButton != null) EndGameButton.gameObject.SetActive(false);
    }

    private IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        photonView.RPC("RPC_RestartAll", RpcTarget.All);
        yield return new WaitForSeconds(0.8f);
        BeginForcedRestart();
    }

    [PunRPC]
    void RPC_RestartAll()
    {
        Debug.Log("[PhotonLauncher] RPC_RestartAll received");
        BeginForcedRestart();
    }

    private void BeginForcedRestart()
    {
        if (isForcedend)
        {
            return;
        }

        isForcedend = true;
        StartCoroutine(RestartConnectionRoutine());
    }

    private IEnumerator RestartConnectionRoutine()
    {
        Debug.Log("[PhotonLauncher] Restarting connection...");

        StopHeartbeatLoop();
        CancelInvoke(nameof(Connect));

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[PhotonLauncher] Leaving current room...");

            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            PhotonNetwork.LeaveRoom(false);

            float leaveDeadline = Time.realtimeSinceStartup + 8f;
            while (PhotonNetwork.InRoom && Time.realtimeSinceStartup < leaveDeadline)
            {
                yield return null;
            }

            if (PhotonNetwork.InRoom)
            {
                Debug.LogWarning("[PhotonLauncher] LeaveRoom timed out, forcing disconnect.");
            }
        }

        ClientState state = PhotonNetwork.NetworkClientState;
        if (PhotonNetwork.IsConnected || (state != ClientState.Disconnected && state != ClientState.PeerCreated))
        {
            Debug.Log($"[PhotonLauncher] Disconnecting from Photon. Current state: {state}");
            PhotonNetwork.Disconnect();

            float disconnectDeadline = Time.realtimeSinceStartup + 10f;
            while ((PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Disconnected)
                && Time.realtimeSinceStartup < disconnectDeadline)
            {
                yield return null;
            }

            if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            {
                Debug.LogWarning($"[PhotonLauncher] Disconnect timed out. Final state before scene reload: {PhotonNetwork.NetworkClientState}");
            }
        }

        reconnecting = false;
        joinedRoom = false;
        isForcedend = false;

        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    private void ConfigureIdentity()
    {
        string userId = ResolveStableUserId();

        if (PhotonNetwork.AuthValues == null)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
        }

        PhotonNetwork.AuthValues.UserId = userId;
        PhotonNetwork.NickName = userId.Substring(0, Math.Min(8, userId.Length));
        localPlayerNickName = PhotonNetwork.NickName;
    }

    private string ResolveStableUserId()
    {
        if (PlayFabLogin.Instance != null && !string.IsNullOrWhiteSpace(PlayFabLogin.Instance.playFabId))
        {
            return PlayFabLogin.Instance.playFabId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(localPlayerNickName))
        {
            return localPlayerNickName.Trim();
        }

        string fallbackId = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrWhiteSpace(fallbackId))
        {
            fallbackId = "001" + UnityEngine.Random.Range(1000, 9999);
        }

        return fallbackId.Trim();
    }

    private string GetConfiguredRoomName()
    {
        return string.IsNullOrWhiteSpace(roomName) ? "eventRoom" : roomName.Trim();
    }

    private RoomOptions BuildRoomOptions()
    {
        return new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers,
            PublishUserId = true,
            PlayerTtl = playerTtlMs,
            EmptyRoomTtl = emptyRoomTtlMs,
            CleanupCacheOnLeave = true
        };
    }

    private void JoinConfiguredRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            joinedRoom = true;
            return;
        }

        string targetRoom = GetConfiguredRoomName();
        Debug.Log($"[PhotonLauncher] Joining configured room '{targetRoom}' (isServer={isServer})");

        if (isServer)
        {
            PhotonNetwork.JoinOrCreateRoom(targetRoom, BuildRoomOptions(), TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.JoinRoom(targetRoom);
        }
    }

    private void RetryJoinConfiguredRoom()
    {
        if (statusText != null)
        {
            statusText.text = isServer ? "Recovering server room..." : "Waiting for server to start...";
        }

        CancelInvoke(nameof(Connect));
        Invoke(nameof(Connect), 2f);
    }

    private void TryRecoverConnection()
    {
        if (PhotonNetwork.InRoom)
        {
            joinedRoom = true;
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinConfiguredRoom();
            return;
        }

        ClientState state = PhotonNetwork.NetworkClientState;
        if (state == ClientState.Disconnected || state == ClientState.PeerCreated)
        {
            Connect();
        }
    }
}
