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

    public GameObject[] trashSprites;

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

        if (isTrash)
        {
            int index = Random.Range(0, trashSprites.Length);
            trashSprites[index].SetActive(true);
        }
    }

    void StartMovement()
    {
        transform.DOMove(endPosition, travelDuration).SetEase(Ease.Linear).OnComplete(OnReachEnd);
    }

    void OnReachEnd()
    {
        // Note reached the end position without being hit
        Debug.Log("Missed Note");
        WaterManager.Instance.RaiseWater(+0.1f, 0.5f);
        Destroy(gameObject);
    }

    public void Hit()
    {
        // Note was hit successfully
        Debug.Log("Hit Note");

        if (!isTrash)
        {
            //AudioManager.Instance.PlaySFX(AudioManager.Instance.splashSound);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("Water Splash");
            }
            PlaySplash();
            WaterManager.Instance.RaiseWater(-0.1f, 0.5f);
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(10);
        }
        else
        {
            WaterManager.Instance.RaiseWater(+0.1f, 0.5f);
            EffectManager.Instance.PlayBadEffect();
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
