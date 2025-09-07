using UnityEngine;
using DG.Tweening;


public class Note : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;

    private float travelDuration;

    [Header("VFX")]
    public GameObject waterSplashPrefab;

    public void Initialize(Vector3 start, Vector3 end, float duration)
    {
        startPosition = start;
        endPosition = end;
        travelDuration = duration;

        transform.position = startPosition;
        StartMovement();
    }

    void StartMovement()
    {
        transform.DOMove(endPosition, travelDuration).SetEase(Ease.Linear).OnComplete(OnReachEnd);
    }

    void OnReachEnd()
    {
        // Note reached the end position without being hit
        Debug.Log("Missed Note");
        Destroy(gameObject);
        PlaySplash();
    }

    public void Hit()
    {
        // Note was hit successfully
        Debug.Log("Hit Note");
        Destroy(gameObject);
        PlaySplash();
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(10);
    }

    private void PlaySplash()
    {
        if (waterSplashPrefab != null)
        {
            GameObject splash = Instantiate(
                waterSplashPrefab,
                transform.position, // spawn at current position
                Quaternion.identity // no rotation, or use prefab’s rotation
            );

            // Optionally destroy splash after some time
            Destroy(splash, 2f);
        }
    }
}
