using UnityEngine;

public class RunGameThree : MonoBehaviour
{
    public GameObject explainer;
    public GameObject[] blaffObjects;

    void Start()
    {
        Invoke("ActivateExplainer", 2f);
        Invoke("CloseExplainer", 17.5f);
        StartCoroutine(SecretLanguageManager.Instance.RunAfterDelay(18));
    }

    private void ActivateExplainer()
    {
        explainer.SetActive(true);
    }

    private void CloseExplainer()
    {
        explainer.SetActive(false);
        foreach (var item in blaffObjects)
        {
            item.SetActive(false);
        }
    }
}
