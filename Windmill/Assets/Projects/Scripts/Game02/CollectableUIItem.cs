using UnityEngine;
using DG.Tweening;

public class CollectableUIItem : MonoBehaviour
{
    public int id;
    public bool collected = false;
    public GameObject collectedIcon;
    public GameObject iconToscale;

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
                needTokill = false;
                isAnimating = true;
                iconToscale.transform.DOScale(new Vector3(1.3f, 1.3f, 13f), 1f)
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
            collectedIcon.SetActive(true);
            return true;
        }
        return true;
    }

    public void ResetItem()
    {
        collected = false;
        collectedIcon.SetActive(false);
    }

    void OnDisable()
    {
        CollectableUIManager.Instance.OnResetItems -= ResetItem;
        CollectableUIManager.Instance.OnCollectItem -= SetCollected;
    }
}
