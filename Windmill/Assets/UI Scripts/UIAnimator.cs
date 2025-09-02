using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;



[System.Serializable]
public class ExtraBar
{
    public Image barImage;
    public float fillDuration = 1f;
    public float startDelay = 0f; // optional: wait before filling
}
public class UIAnimator : MonoBehaviour
{
    [Header("Main Flesh Bar")]
    public Image fleshImage;
    public TMP_Text fleshText;
    public float fleshFillDuration = 2f;
    public bool fadeInText = true;

    [Header("Extra Bars")]
    public ExtraBar[] extraBars;

    void OnEnable()
    {
        AnimateFlesh();
        AnimateExtraBars();
    }

    private void AnimateFlesh()
    {
        if (fleshImage == null) return;

        // reset
        fleshImage.fillAmount = 0f;

        if (fleshText != null)
        {
            if (fadeInText)
                fleshText.alpha = 0f;
            else
                fleshText.gameObject.SetActive(false);
        }

        // animate flesh bar
        fleshImage.DOFillAmount(1f, fleshFillDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (fleshText != null && !fadeInText)
                    fleshText.gameObject.SetActive(true);
            });

        // fade text if enabled
        if (fadeInText && fleshText != null)
            fleshText.DOFade(1f, 0.5f).SetDelay(fleshFillDuration - 0.5f);
    }

    private void AnimateExtraBars()
    {
        foreach (var bar in extraBars)
        {
            if (bar.barImage == null) continue;

            bar.barImage.fillAmount = 0f;

            bar.barImage.DOFillAmount(1f, bar.fillDuration)
                .SetEase(Ease.Linear)
                .SetDelay(bar.startDelay); // lets you stagger them
        }
    }
}