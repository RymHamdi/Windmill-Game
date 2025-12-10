using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerRoomManager : MonoBehaviour
{
    public List<PlayerRoomModel> playersInRoom = new List<PlayerRoomModel>();

    void OnEnable()
    {
        for (int i = 0; i < playersInRoom.Count; i++)
        {
            playersInRoom[i].Reset();
            playersInRoom[i].gameObject.SetActive(false);
        }
        LobbyManager.Instance.OnPlayerJoinedAndUpdateCharacter += CreateNewPlayer;
        LobbyManager.Instance.CheckPlayersInRoom();
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("SYS_PLAYERROOM");
        }

    }

    private void CreateNewPlayer(string photonId, int characterId, bool isLocalPlayer)
    {
        Debug.Log($"Creating/Updating PlayerRoomModel for Photon ID: {photonId}, Character ID: {characterId}, IsLocal: {isLocalPlayer}");
        // Try to find an inactive player model that is not assigned
        PlayerRoomModel availablePlayer = playersInRoom.Find(p => !p.gameObject.activeSelf && !p.hasAPlayer);

        if (availablePlayer != null)
        {
            availablePlayer.gameObject.SetActive(true);
            availablePlayer.Init(characterId, photonId, isLocalPlayer);
        }
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
