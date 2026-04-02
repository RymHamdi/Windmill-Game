using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;
using System.Linq;


public class PickAScreen : MonoBehaviour
{
    public Image screenImage;

    public Image animatedImage;
    public CanvasGroup canvasGroup;

    public ScreenSpriteData[] screenSprites;

    void OnEnable()
    {
        LoadPlayerProperty();
        Invoke(nameof(AnimateScreenPick), 1);
        
    }

    public void LoadPlayerProperty()
    {
        canvasGroup.DOFade(1,0.3f);
        if ( PhotonLauncher.Instance == null)
        {
            // let just choose the first screen if no player found
            screenImage.sprite = screenSprites[0].sprite;
            return;
        }
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("CharacterId"))
        {
            int pickedScreenIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["CharacterId"];
            if (pickedScreenIndex >= 0 && pickedScreenIndex < screenSprites.Length)
            {
                screenImage.sprite = screenSprites[pickedScreenIndex].sprite;
            }
            else
            {
                screenImage.sprite = screenSprites[screenSprites.Length - 1].sprite;
            }
        }
        else
        {
            screenImage.sprite = screenSprites[screenSprites.Length - 1].sprite;
        }
    }

    public void AnimateScreenPick()
    {
        animatedImage.gameObject.SetActive(true);
        animatedImage.transform.localScale = Vector3.one;
        animatedImage.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f),1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(animatedImage.transform);

        // Alpha fade animation (looping)
        animatedImage.DOFade(1,0.1f).OnComplete(() =>
        {
            animatedImage.DOFade(0.99f, 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(animatedImage);
        });
    }
}

[System.Serializable]
public struct ScreenSpriteData
{
    public Sprite sprite;
    public string screenName;
}
