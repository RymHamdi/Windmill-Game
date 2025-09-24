using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class Leaderboard : MonoBehaviour
{
    public List<PlayerLeaderBoardModel> playerModels;

    private static readonly WaitForSeconds leaderboardDelay = new WaitForSeconds(1f);

    public string ScoreKey = "Score";

    public GameObject NextButton;

    public string nextSceneName;

    private void OnEnable()
    {
        NextButton.SetActive(false);
    }

    void Start()
    {
        var players = new List<PlayerLearderBoardStruct>();
        // Example: Assuming you have Photon installed and using Photon.Realtime.Player
        foreach (var photonPlayer in Photon.Pun.PhotonNetwork.PlayerList)
        {
            int characterId = photonPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
            int score = photonPlayer.CustomProperties.TryGetValue(ScoreKey, out object scoreObj) ? (int)scoreObj : 0;

            players.Add(new PlayerLearderBoardStruct
            {
                CharacterID = characterId,
                Score = score
            });
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
        if (PhotonNetwork.IsMasterClient && nextSceneName != "")
        {
            ShowControlTrigger.Instance?.SendTrigger("ShowLeaderboard");
        }
        StartCoroutine(NextGameAfterDely());
    }


    void Update()
    {
        /*if (PhotonNetwork.IsMasterClient)
        {
            NextButton.SetActive(true);
        }
        else
        {
            NextButton.SetActive(false);
        }*/
    }

    IEnumerator NextGameAfterDely()
    {
        yield return new WaitForSeconds(5);
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


    IEnumerator ActivateModelWithDelay(PlayerLeaderBoardModel model, string playerName, int score, Sprite avatar)
    {
        yield return leaderboardDelay; // Use cached WaitForSeconds
        model.gameObject.SetActive(true);
        model.Initialize(playerName, score, avatar);
    }
}

public struct PlayerLearderBoardStruct
{
    public int Score;
    public int CharacterID;
}
