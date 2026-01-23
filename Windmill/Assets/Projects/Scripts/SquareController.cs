using UnityEngine;
using DG.Tweening;

public class SquareController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color color;


    void OnEnable()
    {
        Invoke("SetAlphaToZero", 4.5f);
    }

    public void SetAlphaToZero()
    {
        spriteRenderer.DOColor(new Color(color.r, color.g, color.b, 0), 1f);

    }

    void OnDisable()
    {
        DOTween.KillAll();
    }
}
