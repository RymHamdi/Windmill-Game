using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_Text playersCountText;
    public GameObject startButton;

    public Action<string, int> OnPlayerJoinedAndUpdateCharacter;
    public Action<string> OnPlayerLeft;

    public GameObject CharacterPanels;
    public GameObject RoomPanel;

    public int localPlayerindex;

    //Create instance for easy access
    public static LobbyManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CharacterPanels.SetActive(true);
    }

    void Update()
    {
        playersCountText.text = "Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount;
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Game1"); // syncs load for all
    }

    public bool IsAnyPlayerInRoom()
    {
        return PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount > 0;
    }

    public void CheckPlayersInRoom()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int charId = -1;

            if (p.CustomProperties.ContainsKey("CharacterId"))
                charId = (int)p.CustomProperties["CharacterId"];

            Debug.Log($"Player {p.NickName} has character {charId}");

            if (charId >= 0)
            {
                OnPlayerJoinedAndUpdateCharacter?.Invoke(p.UserId, charId);
            }
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered: " + newPlayer.NickName);

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.UserId);
        OnPlayerLeft?.Invoke(otherPlayer.UserId);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (changedProps.ContainsKey("CharacterId"))
        {
            int newId = (int)changedProps["CharacterId"];
            Debug.Log($"Player {targetPlayer.NickName} updated CharacterId to {newId}");
            OnPlayerJoinedAndUpdateCharacter?.Invoke(targetPlayer.UserId, newId);
            // Update UI here
        }

    }

    public void UpdateLocalPlayerCharacter()
    {
        Hashtable props = new Hashtable
        {
            { "CharacterId", localPlayerindex }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        CharacterPanels.SetActive(false);
        RoomPanel.SetActive(true);
        
    }

}
