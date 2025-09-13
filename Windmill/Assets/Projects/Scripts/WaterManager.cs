using DG.Tweening;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance;

    public Transform waterSurface;

    public float maxWaterHeight = 2.5f;
    public float minWaterHeight = -1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RaiseWater(float amount, float duration)
    {
        // Let shake the water 
        // Let scale the y axis of the water surface
        float targetHeight = Mathf.Clamp(waterSurface.localScale.y + amount, minWaterHeight, maxWaterHeight);
        waterSurface.DOScaleY(targetHeight, duration).SetEase(Ease.OutSine);
        waterSurface.DOShakeScale(0.1f, amount, 10, 90, false);
    }
}
