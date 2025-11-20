using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FinalLeaderboard : MonoBehaviour
{
    public List<PlayerLeaderBoardModel> playerModels;

    private static readonly WaitForSeconds leaderboardDelay = new WaitForSeconds(2f);

    public GameObject canvasServer;
    public GameObject Title;

    void Start()
    {
        Title.SetActive(true);
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                canvasServer.SetActive(true);
            }
        }
        var players = new List<PlayerLearderBoardStruct>();
        // Example: Assuming you have Photon installed and using Photon.Realtime.Player
        foreach (var photonPlayer in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (!photonPlayer.IsMasterClient)
            {
                int characterId = photonPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
            int score1 = photonPlayer.CustomProperties.TryGetValue("Score1", out object scoreObj1) ? (int)scoreObj1 : 0;
            int score2 = photonPlayer.CustomProperties.TryGetValue("Score2", out object scoreObj2) ? (int)scoreObj2 : 0;
            int score3 = photonPlayer.CustomProperties.TryGetValue("Score3", out object scoreObj3) ? (int)scoreObj3 : 0;
            int score = score1 + score2 + score3;

            players.Add(new PlayerLearderBoardStruct
            {
                CharacterID = characterId,
                Score = score
            });
            }
            
        }


        // Sort players by score descending
        players.Sort((a, b) => b.Score.CompareTo(a.Score));

        int index = 0;
        foreach (var leaderBoardModel in players)
        {
            if (index >= playerModels.Count)
                break;

            WindmillCharacter windmillCharacter = GameManager.Instance.GetCharacterById(leaderBoardModel.CharacterID);
            PlayerLeaderBoardModel model = playerModels[index];
            StartCoroutine(ActivateModelWithDelay(model, windmillCharacter.characterName, leaderBoardModel.Score, windmillCharacter.icon));
            index++;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("ShowFinalLeaderboard");
        }
        StartCoroutine(NextGameAfterDely());
    }

    IEnumerator ActivateModelWithDelay(PlayerLeaderBoardModel model, string playerName, int score, Sprite avatar)
    {
        yield return new WaitForSeconds(1f);
        model.gameObject.SetActive(true);
        model.Initialize(playerName, score, avatar);
    }

    public string nextSceneName;
    IEnumerator NextGameAfterDely()
    {
        yield return new WaitForSeconds(15);
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

}
