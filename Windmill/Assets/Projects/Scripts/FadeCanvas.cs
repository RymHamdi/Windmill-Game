using DG.Tweening;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void OnEnable()
    {
        //canvasGroup.alpha = 0;
        if (PhotonLauncher.Instance != null)
        {
            if (PhotonLauncher.Instance.isServer)
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    public void FadeIn()
    {
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1,0.5f);
    }

    public void FadeOut()
    {
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0,0.5f);
    }


}
