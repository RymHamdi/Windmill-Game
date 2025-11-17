using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    // Singleton
    public static PhotonLauncher Instance;

    string gameVersion = "1";
    public string roomName = "eventRoom";
    public bool isServer;

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

    public List<PlayerRoom> playerRooms = new List<PlayerRoom>();
    public GameObject imageToFlash;
    public GameObject playerPanel;

    private string localPlayerNickName;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        photonView.ViewID = 999;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        gameVersion = "1.8";
    }



    void Start()
    {
        Application.runInBackground = true;
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 15;
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (isServer)
            ServerConfigManager.SetActive(true);
        else
            ClientConfigManager.SetActive(true);

        Connect();
        PhotonNetwork.KeepAliveInBackground = 86400;
    }

    // CLEANED — no useless keepalive spam
    void Update()
    {
        
    }

    public override void OnDisconnected(DisconnectCause cause)
{
    Debug.LogError("DISCONNECTED! Reason: " + cause);

    if (isServer)
    {
        // If SERVER loses connection → shutdown fully
        Debug.LogError("SERVER lost connection. Closing server app...");
        Application.Quit();
    }
    
}

    public void Connect()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
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

    public void StartMainMenu()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("MainMenu");
            ShowControlTrigger.Instance.SendTrigger("MainMenu");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        if (PlayFabLogin.Instance != null)
            PhotonNetwork.NickName = PlayFabLogin.Instance.playFabId.Substring(0, 8);
        else if (!string.IsNullOrEmpty(localPlayerNickName))
            PhotonNetwork.NickName = localPlayerNickName;
        else
        {
            PhotonNetwork.NickName = "001" + UnityEngine.Random.Range(1000, 9999).ToString();
            localPlayerNickName = PhotonNetwork.NickName;
        }
            

        Debug.Log("Photon Nickname: " + PhotonNetwork.NickName);

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("JoinRandom Failed: " + message);

        if (isServer)
        {
            PhotonNetwork.CreateRoom(roomName, new RoomOptions
            {
                MaxPlayers = 6,
                PublishUserId = true,
                PlayerTtl = 0
            });
        }
        else
        {
            statusText.text = "Waiting for server to start...";
            Invoke("Connect", 2f);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("CreateRoom Failed: " + message);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        statusText.text = "Joined Room: ";
        LoadLocalPlayerData();
    }

    public void LoadLocalPlayerData()
    {
        InfoRoleText.text = "Device ID: " +
            PlayerPrefs.GetString($"{PhotonNetwork.LocalPlayer.NickName}_role", "No Role");

        InfoVideoText.text = "Video ID: " +
            PlayerPrefs.GetString($"{PhotonNetwork.LocalPlayer.NickName}_video", "No Video");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player Joined: " + newPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
            CreatePlayerRoom(newPlayer);
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

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!isServer)
            Application.Quit();
    }

    // === GAME FLOW ===

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "GameRunning", true } }
        );

        PhotonNetwork.LoadLevel("VideoScene");
        //Disable the photon laucherCanvas UI if you are not the server
        photonView.RPC("RPC_StartGameAll", RpcTarget.All);
        //ShowControlTrigger.Instance.SendTrigger("VideoScene");
        playerPanel.SetActive(false);

        StartGameButton.gameObject.SetActive(false);
        EndGameButton.gameObject.SetActive(true);
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
        gameObjectToDisableOnStart.SetActive(false);

    }

    public void EndGame()
    {
        if (!isServer) return;

        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "GameRunning", false } }
        );

        photonView.RPC("RPC_RestartAll", RpcTarget.All);
        playerPanel.SetActive(true);
        StartGameButton.gameObject.SetActive(true);
        EndGameButton.gameObject.SetActive(false);
    }

    [PunRPC]
    void RPC_RestartAll()
    {
        StartCoroutine(RestartConnectionRoutine());
    }

    private IEnumerator RestartConnectionRoutine()
    {
        Debug.Log("Restarting connection...");

        // 1. Disconnect cleanly
        if (!isServer)
        {
            PhotonNetwork.Disconnect();

            // 2. Wait until fully disconnected
            while (PhotonNetwork.IsConnected || PhotonNetwork.IsConnectedAndReady)
                yield return null;

            // 3. Load ServerConfig scene
            AsyncOperation loadOp = SceneManager.LoadSceneAsync("ServerConfig");
            while (!loadOp.isDone)
                yield return null;

            // 4. Wait 1 frame for the scene to initialize
            yield return null;

            // 5. Re-enable UI only for clients
            if (!isServer)
            {
                gameObjectToDisableOnStart.SetActive(true);
            }

            // 6. NOW reconnect (important: AFTER scene loaded)
            yield return new WaitForSeconds(0.2f);

            Connect();
        }
        else
        {
            SceneManager.LoadSceneAsync("ServerConfig");
        }

    }

    

    // === CONFIG PLAYER UI ===

    private void CreatePlayerRoom(Player player)
    {
        if (playerRooms.Exists(x => x.playerID.text == player.NickName))
            return;

        GameObject entryObj = Instantiate(playerRoomEntryPrefab, contentParent);
        PlayerRoom room = entryObj.GetComponent<PlayerRoom>();

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
        photonView.RPC("RPC_RemovePlayer", RpcTarget.All, playerId);
    }

    [PunRPC]
    void RPC_RemovePlayer(string playerId)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            Application.Quit();
            PhotonNetwork.LeaveRoom();
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
        imageToFlash.SetActive(true);
        yield return new WaitForSeconds(2f);
        imageToFlash.SetActive(false);
    }
}
