using UnityEngine;

public class RunGameThree : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SecretLanguageManager.Instance.RunAfterDelay(2f));
    }
}
