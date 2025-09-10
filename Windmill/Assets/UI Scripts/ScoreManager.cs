using UnityEngine;
using UnityEngine.UI; // Needed for Text
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance; // Singleton for easy access

    [Header("UI")]
    public TMP_Text scoreText;

    private int score = 0;
    private const string ScoreKey = "Score";

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ScoreKey))
            score = (int)PhotonNetwork.LocalPlayer.CustomProperties[ScoreKey];
        else
        {
            score = 0;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props[ScoreKey] = score;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[ScoreKey] = score;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString() + "X";
    }
}
