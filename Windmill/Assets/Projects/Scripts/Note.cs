using UnityEngine;
using DG.Tweening;


public class Note : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 hitZonePosition;

    private float travelDuration;

    [Header("VFX")]
    public GameObject waterSplashPrefab;

    public bool isTrash;

    public void Initialize(Vector3 start, Vector3 end, Vector3 hitzone, float duration)
    {
        startPosition = start;
        endPosition = end;
        hitZonePosition = hitzone;
        travelDuration = duration;

        Vector3 localScal = transform.localScale;
        localScal *= Random.Range(0.8f, 1.2f);
        transform.localScale = localScal;

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
    }

    public void Hit()
    {
        // Note was hit successfully
        Debug.Log("Hit Note");

        if (!isTrash)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.splashSound);
            PlaySplash();
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(10);
        }

        Destroy(gameObject);

    }

    private void PlaySplash()
    {
        if (waterSplashPrefab != null)
        {
            GameObject splash = Instantiate(
                waterSplashPrefab,
                hitZonePosition, // spawn at current position
                Quaternion.identity // no rotation, or use prefab�s rotation
            );

            // Optionally destroy splash after some time
            Destroy(splash, 2f);
        }
    }
}
