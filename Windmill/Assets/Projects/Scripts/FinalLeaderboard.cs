using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FinalLeaderboard : MonoBehaviour
{
    public GameObject canvasServer;
    public GameObject Title;

    public Transform winnersParent;              // Parent contenant les gagnants (layout dynamique)
    public GameObject winnerModelPrefab;         // Prefab UI pour afficher un gagnant
    public GameObject celebrationPrefab;         // Prefab pour célébration (confettis, VFX…)

    public string nextSceneName;
    public List<PlayerLeaderBoardModel> playerModels;

    void Start()
    {
        Title.SetActive(true);
        if (PhotonLauncher.Instance != null && PhotonLauncher.Instance.isServer)
        {
            canvasServer.SetActive(true);
        }
        // Only MasterClient calculates winner
        if (PhotonNetwork.IsMasterClient)
        {
            CalculateAndSendWinner();
            StartCoroutine(NextGameAfterDely());
        }
    }

    // Requirement 1: Move winner calculation logic here
    void CalculateAndSendWinner()
    {
        List<PlayerLearderBoardStruct> players = new List<PlayerLearderBoardStruct>();
        foreach (var photonPlayer in PhotonNetwork.PlayerList)
        {
            if (!photonPlayer.IsMasterClient)
            {
                int characterId = photonPlayer.CustomProperties.TryGetValue("CharacterId", out object characterIdObj) ? (int)characterIdObj : 0;
                int score1 = photonPlayer.CustomProperties.TryGetValue("Score1", out object s1) ? (int)s1 : 0;
                int score2 = photonPlayer.CustomProperties.TryGetValue("Score2", out object s2) ? (int)s2 : 0;
                int score3 = photonPlayer.CustomProperties.TryGetValue("Score3", out object s3) ? (int)s3 : 0;
                players.Add(new PlayerLearderBoardStruct
                {
                    CharacterID = characterId,
                    Score = score1 + score2 + score3
                });
            }
        }

        if (players.Count != 0)
        {
            players.Sort((a, b) => b.Score.CompareTo(a.Score));
            int bestScore = players[0].Score;
            PlayerLearderBoardStruct winner = players[0];
            // Send winner to all clients via RPC
            PhotonView pv = PhotonView.Get(this);
            pv.RPC("RPC_SetFinalWinner", RpcTarget.AllBuffered, winner.CharacterID, winner.Score);
            // Assign other players for UI
            int[] characterIDs = new int[players.Count];
            int[] scores = new int[players.Count];

            for (int i = 0; i < players.Count; i++)
            {
                if(winner.CharacterID == players[i].CharacterID)
                {
                    continue; // Skip winner, already sent
                }
                characterIDs[i] = players[i].CharacterID;
                scores[i] = players[i].Score;
            }

            pv.RPC("RPC_DispalyOtherPlayers", RpcTarget.AllBuffered, characterIDs, scores);
        }
    }

    // Requirement 3: PunRPC method for winner result
    [PunRPC]
    void RPC_SetFinalWinner(int characterID, int score)
    {
        // Requirement 4: All UI and triggers inside RPC
        DisplayWinners(new PlayerLearderBoardStruct { CharacterID = characterID, Score = score });
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("LD_FINAL");
            switch (characterID)
            {
                case 0:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_YELLOW");
                    break;
                case 1:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_GREEN");
                    break;
                case 2:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_RED");
                    break;
                case 3:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_BLUE");
                    break;
                case 4:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_PINK");
                    break;
                default:
                    ShowControlTrigger.Instance?.SendTrigger("LD_FINAL_YELLOW");
                    break;
            }
        }

    }

    [PunRPC]
    void RPC_DispalyOtherPlayers(int[] characterIDs, int[] scores)
    {
        List<PlayerLearderBoardStruct> players = new List<PlayerLearderBoardStruct>();

        for (int i = 0; i < characterIDs.Length; i++)
        {
            players.Add(new PlayerLearderBoardStruct
            {
                CharacterID = characterIDs[i],
                Score = scores[i]
            });
        }

        StartCoroutine(DispalyOtherPlayers(players));
    }
    private IEnumerator DispalyOtherPlayers(List<PlayerLearderBoardStruct> players)
    {
        yield return new WaitForSeconds(1f);
        if (players.Count > 1)
        {
            for (int i = 1; i < players.Count; i++)
            {
                PlayerLeaderBoardModel model = playerModels[i - 1];
                model.gameObject.SetActive(true);
                WindmillCharacter character = GameManager.Instance.GetCharacterById(players[i].CharacterID);
                model.Initialize(character.characterName, players[i].Score, character.icon, Vector3.zero);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }


    void DisplayWinners(PlayerLearderBoardStruct winners)
    {
        // Nettoyer l’ancien contenu
        foreach (Transform child in winnersParent)
            Destroy(child.gameObject);

        int count = 1;

        // --- Ajustement dynamique de la grille selon le nombre de gagnants ---
        var grid = winnersParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null)
        {
            if (count == 1)
            {
                // Un seul gagnant → centré et plus grand
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 1;
                grid.cellSize = new Vector2(396, 396); // Très grand gagnant
            }
            else if (count == 2)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                grid.cellSize = new Vector2(396, 396);
            }
            else if (count == 3)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2; // triangle (2 au dessus, 1 en dessous)
                grid.cellSize = new Vector2(396, 396);
                winnersParent.position = new Vector3(winnersParent.position.x, winnersParent.transform.position.y + 200, winnersParent.position.z);
            }

            else if (count == 4)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2; // triangle (2 au dessus, 1 en dessous)
                grid.cellSize = new Vector2(396, 396);
                winnersParent.position = new Vector3(winnersParent.position.x, winnersParent.transform.position.y + 200, winnersParent.position.z);
            }
            else if (count == 5)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2; // triangle (2 au dessus, 1 en dessous)
                grid.cellSize = new Vector2(396, 396);
                winnersParent.position = new Vector3(winnersParent.position.x, winnersParent.transform.position.y + 400, winnersParent.position.z);
            }
        }

        // --- Affichage UI des gagnants ---

        WindmillCharacter character = GameManager.Instance.GetCharacterById(winners.CharacterID);

        GameObject modelGO = Instantiate(winnerModelPrefab, winnersParent);
        PlayerLeaderBoardModel model = modelGO.GetComponent<PlayerLeaderBoardModel>();
        if (count > 1)
        {
            model.Initialize(character.characterName, winners.Score, character.icon, Vector3.zero);
        }
        else
        {
            model.Initialize(character.characterName, winners.Score, character.icon, new Vector3(1.5f, 1.5f, 1.5f));
        }


        // --- Instancier un effet de célébration ---
        if (celebrationPrefab != null)
        {
            Instantiate(celebrationPrefab, transform);
        }
    }

    IEnumerator NextGameAfterDely()
    {
        yield return new WaitForSeconds(15);
        if (!string.IsNullOrEmpty(nextSceneName))
            StartNextGame();
    }

    public void StartNextGame()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(nextSceneName);
    }
}
