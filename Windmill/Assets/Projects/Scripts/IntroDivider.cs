using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System;

public class IntroDivider : MonoBehaviour
{
    public List<IntroItem> introItems;
    public float fadeDuration = 1.0f;
    public float displayDuration = 2.0f;
    public float delayBetweenItems = 2f;
    private int currentIndex = 0;
    private bool isTransitioning = false;
    Action OnComplete;


    public void Init(Action onComlete)
    {
        OnComplete = onComlete;
        ResetCanvasGroups();
        StartCoroutine(PlayIntroSequence());
    }

    private void ResetCanvasGroups()
    {
        foreach (var item in introItems)
        {
            item.canvasGroup.alpha = 0;
        }
        currentIndex = 0;
    }

    private IEnumerator PlayIntroSequence()
    {
        while (true)
        {
            if (!isTransitioning && currentIndex < introItems.Count)
            {
                isTransitioning = true;
                var item = introItems[currentIndex];

                // Fade in
                yield return StartCoroutine(FadeCanvasGroup(item.canvasGroup, 0, 1, fadeDuration));

                // Wait for display duration
                yield return new WaitForSeconds(displayDuration);

                currentIndex++;
                isTransitioning = false;
                StartCoroutine(DispalyText(item.text));

                // Wait before next item
                yield return new WaitForSeconds(delayBetweenItems);
            }
            else if (currentIndex >= introItems.Count)
            {
                // All items displayed, stop the coroutine
                StartCoroutine(DisableAllCanvasGroups());
                yield break;
            }
            else
            {
                yield return null; // wait for next frame
            }
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        //Popup the transfrom of the canvas group
        Vector3 originalScale = cg.transform.localScale;


        cg.transform.DOScale(originalScale * 1.1f, duration / 2).SetEase(Ease.OutBack).OnComplete(() =>
        {
            cg.transform.DOScale(originalScale, duration / 2).SetEase(Ease.InBack);
        });
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end; // ensure it ends exactly at the target value
    }

    private IEnumerator DispalyText(TMP_Text tMP_Text)
    {
        string fullText = tMP_Text.text;
        tMP_Text.text = "";
        tMP_Text.gameObject.SetActive(true);
        for (int i = 0; i < fullText.Length; i++)
        {
            tMP_Text.text += fullText[i];
            yield return new WaitForSeconds(0.05f); // Adjust typing speed here
        }
    }

    private IEnumerator DisableAllCanvasGroups()
    {
        foreach (var item in introItems)
        {
            yield return StartCoroutine(FadeCanvasGroup(item.canvasGroup, item.canvasGroup.alpha, 0, fadeDuration / 2));
        }
        OnComplete?.Invoke();
        gameObject.SetActive(false);
    }

}

[System.Serializable]
public class IntroItem
{
    public CanvasGroup canvasGroup;
    public Image image;
    public TMP_Text text;
}
