using UnityEngine;

public class CollectableUIItem : MonoBehaviour
{
    public int id;
    public bool collected = false;
    public GameObject collectedIcon;

    private void Start()
    {
        collectedIcon.SetActive(collected);
        CollectableUIManager.Instance.OnResetItems += ResetItem;
        CollectableUIManager.Instance.OnCollectItem += SetCollected;
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
            Debug.Log("Collected item ID: " + id);
            return true;
        }
        Debug.Log("Something go to collect item ID: " + id);
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
