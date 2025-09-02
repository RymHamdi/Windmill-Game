using UnityEngine;

public class StarWarsCrawl : MonoBehaviour
{
    public float speed = 45f; // movement speed

    void Update()
    {
        // Move upward in local space
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        float scaleFactor = 0.01f;
        transform.localScale -= Vector3.one * scaleFactor * Time.deltaTime;
    }
}
