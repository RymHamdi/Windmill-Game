using UnityEngine;

public class RunGameThree : MonoBehaviour
{
    public GameObject explainer;

    void Start()
    {
        explainer.SetActive(true);
        Invoke("CloseExplainer", 6);
        StartCoroutine(SecretLanguageManager.Instance.RunAfterDelay(6f));
    }

    private void CloseExplainer()
    {
        explainer.SetActive(false);
    }
}
