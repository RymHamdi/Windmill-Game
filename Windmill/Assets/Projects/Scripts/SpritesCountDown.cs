using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Photon.Pun;


public class SpritesCountDown : MonoBehaviour
{
    public Image[] countdownSprites; // Assign your countdown sprites in the inspector
    public float displayDuration = 1f; // Duration each sprite is displayed

    public GameObject currentActiveObject;
    public List<GameObject> objectsToActivate;

    public CanvasGroup textCanvasGroup;

    private void Start()
    {
        StartCoroutine(PlayCountdown());
    }

    private IEnumerator PlayCountdown()
    {
        textCanvasGroup.alpha = 0f;
        textCanvasGroup.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(1f); // Optional delay before starting
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("ShowCountdown");
        }
        foreach (var sprite in countdownSprites)
        {
            sprite.gameObject.SetActive(true);
            CanvasGroup canvasGroup = sprite.GetComponent<CanvasGroup>();
            // Make a smooth dowenn of fade in using canvas group and scale up as popup effect
            if (canvasGroup == null)
            {
                canvasGroup = sprite.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            sprite.transform.localScale = Vector3.zero;
            // Fade in and scale up
            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, 0.5f));
            seq.Join(sprite.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            yield return seq.WaitForCompletion();
            yield return new WaitForSeconds(displayDuration);
            sprite.gameObject.SetActive(false);
        }
        ActivateNextObjects();
    }

    private void ActivateNextObjects()
    {
        if (objectsToActivate.Count == 0) return;



        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
        
         if (currentActiveObject != null)
        {
            currentActiveObject.SetActive(false);
        }
    }
}
