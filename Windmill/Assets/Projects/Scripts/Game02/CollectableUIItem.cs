using UnityEngine;
using DG.Tweening;

public class CollectableUIItem : MonoBehaviour
{
    public int id;
    public bool collected = false;
    public GameObject collectedIcon;
    public GameObject iconToscale;
    public CanvasGroup canvasGroup;

    private void Start()
    {
        
        collectedIcon.SetActive(collected);
        CollectableUIManager.Instance.OnResetItems += ResetItem;
        CollectableUIManager.Instance.OnCollectItem += SetCollected;
        /*CollectableUIManager.Instance.OnCollectedItemCountChanged += (count) =>
        {
            Debug.Log($"Item ID: {id}, CollectedItems Count Changed: {count}");
            if (count + 1 == id)
            {
                iconToscale.transform.DOScale(Vector3.one * 1.2f, 0.3f);
            }
            else
            {
                iconToscale.transform.localScale = Vector3.one;
            }
        };*/
    }
    bool isAnimating = false;
    bool needTokill = false;
    void Update()
    {
        if (CollectableUIManager.Instance.CollectedItems + 1 == id && !collected)
        {
            if (!isAnimating)
            {
                canvasGroup.alpha = 1;
                iconToscale.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
                needTokill = false;
                isAnimating = true;
                iconToscale.transform.DOScale(new Vector3(1.5f, 1.5f, 15f), 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(iconToscale.transform).OnComplete(() => isAnimating = false);
            
            }
        }
        else
        {
            if (needTokill == false)
            {
                DOTween.Kill(iconToscale.transform);
                iconToscale.transform.localScale = Vector3.one;
                needTokill = true;
                isAnimating = false;
                if (!collected)
                {
                    iconToscale.transform.localScale = new Vector3(1,1,1);
                    canvasGroup.alpha = 0.8f;
                }

            }

        }
    }

    public bool SetCollected(int collectId)
    {
        if (collected && collectId == id) return false;
        if (collectId <= id && collected)
        {
            return false;
        }
        if (collectId == id)
        {
            collected = true;
            iconToscale.transform.localScale = new Vector3(1,1,1);
            collectedIcon.SetActive(true);
            return true;
        }
        return true;
    }

    public void ResetItem()
    {
        collected = false;
        collectedIcon.SetActive(false);
        canvasGroup.alpha = 0.8f;
        iconToscale.transform.localScale = new Vector3(1,1,1);
    }

    void OnDisable()
    {
        CollectableUIManager.Instance.OnResetItems -= ResetItem;
        CollectableUIManager.Instance.OnCollectItem -= SetCollected;
    }
}
