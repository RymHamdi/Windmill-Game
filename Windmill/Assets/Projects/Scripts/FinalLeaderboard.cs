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

    void Start()
    {
        Title.SetActive(true);

        if (PhotonLauncher.Instance != null && PhotonLauncher.Instance.isServer)
        {
            canvasServer.SetActive(true);
        }

        // --- Récupération des scores ---
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
            // --- On trie du plus grand au plus petit ---
            players.Sort((a, b) => b.Score.CompareTo(a.Score));

            // Score du gagnant
            int bestScore = players[0].Score;

            // --- Récupérer tous les gagnants (ex æquo inclus) ---
            List<PlayerLearderBoardStruct> winners = players.FindAll(p => p.Score == bestScore);

            // --- Affichage dynamique des gagnants ---
            DisplayWinners(winners);
            // --- Envoi du trigger serveur ---
            if (PhotonNetwork.IsMasterClient)
            {
                ShowControlTrigger.Instance?.SendTrigger("LD_FINAL");
                foreach (var item in winners)
                {
                    switch (item.CharacterID)
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

                //StartCoroutine(NextGameAfterDely());
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(NextGameAfterDely());
        }
    }


    void DisplayWinners(List<PlayerLearderBoardStruct> winners)
    {
        // Nettoyer l’ancien contenu
        foreach (Transform child in winnersParent)
            Destroy(child.gameObject);

        int count = winners.Count;

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
        foreach (var winner in winners)
        {
            WindmillCharacter character = GameManager.Instance.GetCharacterById(winner.CharacterID);

            GameObject modelGO = Instantiate(winnerModelPrefab, winnersParent);
            PlayerLeaderBoardModel model = modelGO.GetComponent<PlayerLeaderBoardModel>();
            if (count > 1)
            {
                model.Initialize(character.characterName, winner.Score, character.icon, Vector3.zero);
            }
            else
            {
                model.Initialize(character.characterName, winner.Score, character.icon, new Vector3(1.5f, 1.5f, 1.5f));
            }
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
