using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLeaderBoardModel : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text scoreText;

    public Image playerAvtar;

    public void Initialize(string playerName, int score, Sprite avatar)
    {
        playerNameText.text = "Hero: " + playerName;
        scoreText.text = "Score: " + score.ToString();
        playerAvtar.sprite = avatar;
    }

    
}
