using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    public string roomName = "eventRoom";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        gameVersion = "1.3";
    }

    void Start()
    {
        Connect();
        ShowControlTrigger.Instance?.SendTrigger("Launcher");
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            //PhotonNetwork.JoinRoom(roomName);
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {


            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.gameVersion;
        }
    }

    public void StartMainMenu()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("MainMenu"); // syncs load for all
            ShowControlTrigger.Instance.SendTrigger("MainMenu");
        }

    }

    public override void OnConnectedToMaster()
    {
        // Join random room (or create if none exist)
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Failed. No rooms available, creating room");
        PhotonNetwork.CreateRoom(roomName, new Photon.Realtime.RoomOptions { MaxPlayers = 5, PublishUserId = true });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        // SceneManager.LoadScene("MainMenu");
        // Let
        // 's open the Gameobject of choose Character
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby");
        //SceneManager.LoadScene("MainMenu");
        PhotonNetwork.JoinRandomRoom();
    }

}
