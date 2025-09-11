using UnityEngine;
using DG.Tweening;


public class Logo : MonoBehaviour
{
    public Transform logoTransfrom;
    public CanvasGroup canvasGroup;




    void OnEnable()
    {
        PlayLogoAnimation();
    }

    public void PlayLogoAnimation()
    {
        // Reset any previous tweens
        DOTween.Kill(logoTransfrom);
        DOTween.Kill(canvasGroup);

        // Scale "breathing" animation (looping)
        logoTransfrom.localScale = Vector3.one;
        logoTransfrom.DOScale(new Vector3(1.05f, 1.05f, 1f),1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(logoTransfrom);

        // Alpha fade animation (looping)
        canvasGroup.alpha = 1f;
        canvasGroup.DOFade(0.9f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(canvasGroup);
    }
}
