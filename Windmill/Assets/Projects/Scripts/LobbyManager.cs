using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using ExitGames.Client.Photon;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

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
    private bool isGameStarted;

    public FadeCanvas fadIn;
    private bool isFadeIn;

    //Create instance for easy access
    public static LobbyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("LobbyManager Awake");
        startTime = Time.time;
        waitTime += startTime;
    }

    public GameObject TexteServer;
    void Start()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                TexteServer.SetActive(true);
                if (PhotonNetwork.IsMasterClient)
                {
                    ShowControlTrigger.Instance.SendTrigger("SYS_MAINMENU");
                }
                return;
            }
        }
        CharacterPanels.SetActive(true);
    }

    void Update()
    {
        if (playersCountText != null)
        {
            // playersCountText.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + 5;
        }

        //startButton.SetActive(PhotonNetwork.IsMasterClient);

        // No just after the wait time update player prop with UpdateLocalPlayerCharacter
        if (Time.time >= waitTime - 2 && !hasUpdated && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(AutoAssignPlayer());
            hasUpdated = true;

        }

        if (Time.time >= waitTime + 4.5f && PhotonNetwork.IsMasterClient && !isFadeIn)
        {
            isFadeIn = true;
            fadIn.FadeIn();

        }

        if (Time.time >= waitTime + 5 && PhotonNetwork.IsMasterClient && !isGameStarted)
        {
            isGameStarted = true;
            StartGame();
        }
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
        //SceneManager.LoadScene(0);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
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

    public bool UpdateLocalPlayerCharacter(int index)
    {
        if (hasUpdated)
        {
            return false;
        }
        if (!CheckIFCharacterSelectedByAnotherPlayer(index))
        {
            localPlayerindex = index;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "CharacterId", localPlayerindex }
        };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            return true;
        }
        else
        {
            switch (Language.Instance.languageData.currentLangState)
            {
                case LangState.English:
                    PopupManager.Instance.ShowPopup("Character already taken!");
                    break;

                case LangState.Frensh:
                    PopupManager.Instance.ShowPopup("Personnage déjà pris!");
                    break;

                case LangState.Netherland:
                    PopupManager.Instance.ShowPopup("Personage is al gekozen!");
                    break;

                case LangState.Germand:
                    PopupManager.Instance.ShowPopup("Charakter bereits vergeben!");
                    break;

                case LangState.Spanish:
                    PopupManager.Instance.ShowPopup("Personaje ya seleccionado!");
                    break;

                case LangState.Chineese:
                    PopupManager.Instance.ShowPopup("角色已被选择!");
                    break;

                case LangState.Italian:
                    PopupManager.Instance.ShowPopup("Personaggio già scelto!");
                    break;

                default:
                    PopupManager.Instance.ShowPopup("Character already taken!");
                    break;
            }

            return false;
        }

    }

    public bool CheckIFCharacterSelectedByAnotherPlayer(int characterIndex, Player excludePlayer = null)
    {
        // If no player is specified to exclude, use the local player
        if (excludePlayer == null)
            excludePlayer = PhotonNetwork.LocalPlayer;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            // Skip the excluded player (either local player or specified player)
            if (p == excludePlayer)
                continue;

            if (p.CustomProperties.ContainsKey("CharacterId"))
            {
                int charId = (int)p.CustomProperties["CharacterId"];
                if (charId == characterIndex)
                {
                    Debug.Log($"Character {characterIndex} is already selected by {p.NickName}");
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerator AutoAssignPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                // Skip master client
                if (p.IsMasterClient)
                    continue;

                // Check if player doesn't have a character selected
                if (!p.CustomProperties.ContainsKey("CharacterId"))
                {
                    // Find an available character
                    for (int i = 0; i < 5; i++) // 5 available characters
                    {
                        // Check if this character is available for this specific player
                        if (!CheckIFCharacterSelectedByAnotherPlayer(i, p))
                        {
                            // Assign this character to the player
                            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                        {
                            { "CharacterId", i }
                        };
                            p.SetCustomProperties(props);
                            Debug.Log($"Auto-assigned character {i} to player {p.NickName}");
                            break;
                        }
                    }
                }
                yield return new WaitForSeconds(0.35f);

            }
            photonView.RPC("RPC_UpdatePanel", RpcTarget.All);
        }


    }

    private List<Player> GetNonMasterPlayers()
    {
        List<Player> players = new List<Player>();

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.IsMasterClient)
                players.Add(p);
        }

        // Sort by ActorNumber (ensures stable order)
        players.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        return players;
    }

    public int GetPlayerGameIndex(Player player)
    {
        var list = GetNonMasterPlayers();
        return list.IndexOf(player);  // returns 0…4, master = -1
    }

    public int GetLocalPlayerIndex()
    {
        int index = GetPlayerGameIndex(PhotonNetwork.LocalPlayer);
        return index;
    }


    [PunRPC]
    private void RPC_UpdatePanel()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                return;
            }
        }
        CharacterPanels.SetActive(false);
        RoomPanel.SetActive(true);
    }


}
