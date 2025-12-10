using UnityEngine;
using DG.Tweening;

public class ScaleAnimationForInfo : MonoBehaviour
{
    public Transform info;

    public float timeToHide = 2;

    public void Popup()
    {
        info.localScale = Vector3.zero;
        info.DOScale(Vector3.one, 0.8f).SetEase(Ease.InOutBack);
        Invoke(nameof(Hide), timeToHide);
    }

    public void Hide()
    {
        info.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
    }

    void OnEnable()
    {
        Popup();
    }
}
