using UnityEngine;
using UnityEngine.UI;

public class BingoFX : MonoBehaviour
{
    public enum SliceType { Good, Bad }
    public SliceType sliceType = SliceType.Good;

    public float duration = 1.2f;
    public float popScale = 1.3f;
    public float shakeIntensity = 10f;
    public Color badColor = Color.red;

    private float timer;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector3 startScale;
    private Vector3 startPos;
    private Text text;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        startScale = rect.localScale;
        startPos = rect.localPosition;

        text = GetComponent<Text>();
        if (sliceType == SliceType.Bad && text != null)
        {
            text.color = badColor;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (sliceType == SliceType.Good)
        {
            // Pop scale at start
            float scale = Mathf.Lerp(popScale, 1f, t);
            rect.localScale = startScale * scale;
        }
        else if (sliceType == SliceType.Bad)
        {
            // Shake horizontally
            float offsetX = Mathf.Sin(Time.time * 50f) * shakeIntensity * (1f - t);
            rect.localPosition = startPos + new Vector3(offsetX, 0, 0);
        }

        // Fade out
        canvasGroup.alpha = 1f - t;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
