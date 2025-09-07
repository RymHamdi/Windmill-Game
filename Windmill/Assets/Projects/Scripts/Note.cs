using UnityEngine;
using DG.Tweening;


public class Note : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;

    private float travelDuration;

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
    }

    public void Hit()
    {
        // Note was hit successfully
        Debug.Log("Hit Note");
        Destroy(gameObject);
    }
}
