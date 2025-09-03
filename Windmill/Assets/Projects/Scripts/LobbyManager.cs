using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text playersCountText;
    public GameObject startButton;

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
}
