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

    public Action<string, int, bool> OnPlayerJoinedAndUpdateCharacter;
    public Action<string> OnPlayerLeft;

    public GameObject CharacterPanels;
    public GameObject RoomPanel;

    public int localPlayerindex;

    public float waitTime = 15f;
    private float startTime;
    private bool hasUpdated = false;

    //Create instance for easy access
    public static LobbyManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        Debug.Log("LobbyManager Awake");
        startTime = Time.time;
        waitTime += startTime;
    }

    void Start()
    {
        CharacterPanels.SetActive(true);
    }

    void Update()
    {
        if (playersCountText != null)
        {
            playersCountText.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + 5;
        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);

        // No just after the wait time update player prop with UpdateLocalPlayerCharacter
        if (Time.time >= waitTime && !hasUpdated)
        {
            hasUpdated = true;
            UpdateLocalPlayerCharacter();
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Game2"); // syncs load for all
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
                OnPlayerJoinedAndUpdateCharacter?.Invoke(p.UserId, charId, p.IsLocal);
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
            OnPlayerJoinedAndUpdateCharacter?.Invoke(targetPlayer.UserId, newId, targetPlayer.IsLocal);
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
