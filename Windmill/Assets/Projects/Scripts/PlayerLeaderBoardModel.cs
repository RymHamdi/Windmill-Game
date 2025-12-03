using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerLeaderBoardModel : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text scoreText;

    public Image playerAvtar;

    [Header("Animation Settings")]
    public float scaleUp = 1.2f;      // how big it gets
    public float scaleDuration = 0.2f; // time to scale up
    public float scaleDownDuration = 0.2f; // time to return

    private Vector3 originalScale;
    private Vector3 bigScale;


    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Initialize(string playerName, int score, Sprite avatar, Vector3 bigScale)
    {
        playerNameText.text = "Hero: " + playerName;
        scoreText.text = "Score: " + score.ToString();
        playerAvtar.sprite = avatar;
        this.bigScale = bigScale;
        PlayPopup();
    }

    public void PlayPopup()
    {
        // Kill any running tweens to avoid stacking
        transform.DOKill();
        Debug.Log("Playing popup for " + bigScale.ToString());

        // Reset scale in case something weird happened
        transform.localScale = originalScale;
        if (bigScale == Vector3.zero)
        {
            // Pop up then back down
            transform
                .DOScale(originalScale * scaleUp, scaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    transform.DOScale(originalScale, scaleDownDuration).SetEase(Ease.InBack);
                });
        }
        else
        {
            // let bigg it to bigsccale and stay on big scale with nice animation
            transform
            .DOScale(bigScale, scaleDuration).SetEase(Ease.OutBack);
        }

    }


}
