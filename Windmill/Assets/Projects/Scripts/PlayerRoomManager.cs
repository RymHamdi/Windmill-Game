using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoomManager : MonoBehaviour
{
    public PlayerRoomModel playerPrefab;
    public Transform contentParent;

    public List<PlayerRoomModel> playersInRoom = new List<PlayerRoomModel>();

    void OnEnable()
    {
        LobbyManager.Instance.OnPlayerJoinedAndUpdateCharacter += CreateNewPlayer;
        LobbyManager.Instance.CheckPlayersInRoom();
    }

    private void CreateNewPlayer(string photonId, int characterId)
    {
        PlayerRoomModel newPlayer = Instantiate(playerPrefab, contentParent);
        newPlayer.Init(characterId, photonId);
        playersInRoom.Add(newPlayer);
    }

    void OnDisable()
    {
        foreach (var player in playersInRoom)
        {
            if (player != null)
                Destroy(player.gameObject);
        }
    }
}
