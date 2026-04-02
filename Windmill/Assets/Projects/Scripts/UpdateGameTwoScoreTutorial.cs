using UnityEngine;

public class UpdateGameTwoScoreTutorial : MonoBehaviour
{
    public int id;
    public bool needToReset = false;
    void OnEnable()
    {
        if (CollectableUIManager.Instance!= null)
        {
            CollectableUIManager.Instance.CollectItem(id);
        }

        if (needToReset)
        {
            Invoke("ResetItems", 1f);
        }
        
    }

    private void ResetItems()
    {
        if (CollectableUIManager.Instance != null)
        {
            CollectableUIManager.Instance.CallLetsCollectAgain();
        }
    }
}
