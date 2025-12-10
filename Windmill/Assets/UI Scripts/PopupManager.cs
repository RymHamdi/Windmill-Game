using UnityEngine;
using TMPro;
using DG.Tweening;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    public GameObject popupPanel;
    public TextMeshProUGUI popupText;

    private void Awake()
    {
        Instance = this;
        popupPanel.SetActive(false);
    }

    public void ShowPopup(string message)
    {
        CanvasGroup canvasGroup =  popupText.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        popupText.text = message;
        popupPanel.SetActive(true);
        canvasGroup.DOFade(1,0.5f);
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), 1.5f);
    }

    private void Hide()
    {
        popupPanel.SetActive(false);
    }
}
