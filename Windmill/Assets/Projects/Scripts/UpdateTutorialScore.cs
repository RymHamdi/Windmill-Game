using UnityEngine;

public class UpdateTutorialScore : MonoBehaviour
{
    void OnEnable()
    {
        ScoreManager.Instance.AddScore(10);
    }
}
