using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween namespace

public class FeedbackSpriteFX : MonoBehaviour
{
    public enum FeedbackType { Good, Bad }
    public FeedbackType feedbackType = FeedbackType.Good;

    [Header("Animation Settings")]
    public float duration = 1.2f;
    public float popScale = 1.3f;
    public float floatUpDistance = 50f;
    public float shakeIntensity = 20f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        if (feedbackType == FeedbackType.Good)
        {
            PlayGoodAnimation();
        }
        else
        {
            PlayBadAnimation();
        }
    }

    private void PlayGoodAnimation()
    {
        Sequence seq = DOTween.Sequence();

        // Start pop
        rect.localScale = Vector3.one;
        seq.Append(rect.DOScale(popScale, 0.2f).SetEase(Ease.OutBack));

        // Scale back to 1
        seq.Append(rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

        // Float upward while fading
        seq.Join(rect.DOLocalMoveY(rect.localPosition.y + floatUpDistance, duration).SetEase(Ease.OutQuad));
        seq.Join(canvasGroup.DOFade(0f, duration));

        seq.OnComplete(() => Destroy(gameObject));
    }

    private void PlayBadAnimation()
    {
        Sequence seq = DOTween.Sequence();

        // Shake position
        seq.Append(rect.DOShakePosition(0.6f, new Vector3(shakeIntensity, 0, 0), 20, 90, false, true));

        // Fade out
        seq.Join(canvasGroup.DOFade(0f, duration));

        seq.OnComplete(() => Destroy(gameObject));
    }
}