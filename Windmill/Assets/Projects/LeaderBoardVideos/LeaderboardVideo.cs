using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Photon.Pun;
using System.Collections;

public class LeaderboardVideo : MonoBehaviourPun
{
    public List<LeaderboardVideoStruct> leaderboardVideos;
    public VideoPlayer winVideoPlayer;
    public VideoPlayer looseVideoPlayer;

    public GameObject WinPanel;
    public GameObject LoosePanel;

    public string ScoreKey = "Score";
    public string nextSceneName;
    public FadeCanvas fadeCanvas;
    public LanguageSetup LanguageSetup;
    public bool needToCloseForFirstTime = true;


    void OnEnable()
    {
        if (needToCloseForFirstTime)
        {
            needToCloseForFirstTime = false;
            Invoke("CloseAfterDelay", 2); // 
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            GetJustOneWinner();
        }
        LanguageSetup.UpdateImageVisibility(false);
    }

    private void CloseAfterDelay()
    {
        gameObject.SetActive(false);
    }


    private void GetJustOneWinner()
    {
        var players = new List<PlayerLearderBoardStruct>();
        // Example: Assuming you have Photon installed and using Photon.Realtime.Player
        foreach (var photonPlayer in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (!photonPlayer.IsMasterClient)
            {
                int characterId = photonPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
                int score = photonPlayer.CustomProperties.TryGetValue(ScoreKey, out object scoreObj) ? (int)scoreObj : 0;

                players.Add(new PlayerLearderBoardStruct
                {
                    CharacterID = characterId,
                    Score = score,
                    player = photonPlayer
                });
            }

        }

        if (players.Count == 0)
        {
            Debug.LogError("No players found for leaderboard.");
            return;
        }
        players.Sort((a, b) =>
        {
            int scoreCompare = b.Score.CompareTo(a.Score);
            if (scoreCompare != 0)
                return scoreCompare;

            // Tie breaker using ActorNumber (unique & consistent)
            return a.player.ActorNumber.CompareTo(b.player.ActorNumber);
        });
        PlayerLearderBoardStruct winnerPlayer = players[0];
        photonView.RPC("SendPlayerNickname", RpcTarget.AllBuffered, winnerPlayer.player.NickName);
        
        StartCoroutine(NextGameAfterDely(winnerPlayer.CharacterID));

    }

    IEnumerator NextGameAfterDely(int id)
    {
        yield return new WaitForSeconds(0.5f);
        ShowControlTrigger.Instance?.SendTrigger("LD_SHOW");

        switch (id)
        {
            case 0:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_YELLOW");
                break;
            case 1:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_GREEN");
                break;
            case 2:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_RED");
                break;
            case 3:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_BLUE");
                break;
            case 4:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_PINK");
                break;
            default:
                ShowControlTrigger.Instance?.SendTrigger("LD_SHOW_YELLOW");
                break;
        }
        yield return new WaitForSeconds(11f);
        fadeCanvas.FadeIn();
        yield return new WaitForSeconds(0.5f);
        if (nextSceneName != "")
        {
            StartNextGame();
        }
    }

    public void StartNextGame()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(nextSceneName); // syncs load for all
    }

    [PunRPC]
    public void SendPlayerNickname(string nickname)
    {
        //Check if local player has Characterid property if yes do that, if is not just take the loos screen
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("CharacterId"))
        {
            if (PhotonNetwork.LocalPlayer.NickName == nickname)
            {
                int characterId = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
                LeaderboardVideoStruct videoStruct = leaderboardVideos[characterId];
                winVideoPlayer.clip = videoStruct.winVideo;
                WinPanel.SetActive(true);
                // add cheering sound
                AudioManager.Instance.Play("Cheering");
            }
            else
            {
                int characterId = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
                LeaderboardVideoStruct videoStruct = leaderboardVideos[characterId];
                looseVideoPlayer.clip = videoStruct.looseVideo;
                LoosePanel.SetActive(true);

            }
        }
        else if (PhotonLauncher.Instance != null && !PhotonLauncher.Instance.isServer)
        {
            //Just play loose video
            int  characterId = 0; //Default character id
            LeaderboardVideoStruct videoStruct = leaderboardVideos[characterId];
            looseVideoPlayer.clip = videoStruct.looseVideo;
            LoosePanel.SetActive(true);
        }


    }

}

[System.Serializable]

public struct LeaderboardVideoStruct
{
    public string colorID;
    public VideoClip looseVideo;
    public VideoClip winVideo;

}
