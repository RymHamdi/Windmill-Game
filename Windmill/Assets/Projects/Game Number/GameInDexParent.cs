using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;


public class GameInDexParent : MonoBehaviour
{
    public static GameInDexParent instance;

    public RawImage icon;

    public CanvasGroup canvasGroup;
    public int gameIndex = 0;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public System.Collections.IEnumerator EnableCanvas(System.Action onComplete = null)
    {
        canvasGroup.DOFade(1, 0.01f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ShowControlTrigger.Instance?.SendTrigger("VO_GAME_" + gameIndex);
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(icon.DOFade(1, 0.5f).SetEase(Ease.InOutSine));
            seq.Join(icon.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0), 0.5f, 10, 1).SetEase(Ease.InOutSine));
        });

        yield return new WaitForSeconds(5);
        canvasGroup.DOFade(0, 0.3f);
        onComplete?.Invoke();
    }

}
