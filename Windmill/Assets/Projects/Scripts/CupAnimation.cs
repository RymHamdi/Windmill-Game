using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CupAnimation : MonoBehaviour
{
    public Image targetImage;          // UI Image to animate
    public Sprite[] frames;            // All frames of the animation
    public float frameRate = 0.05f;    // Time per frame
    public bool playOnStart = true;
    public bool isPingPong;

    private Coroutine animRoutine;

    private void Start()
    {

    }

    void OnEnable()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        if (isPingPong)
        {
            int index = 0;
            int direction = 1; // +1 forward, -1 backward

            while (true)
            {
                targetImage.sprite = frames[index];

                index += direction;

                // Ping-pong logic
                if (index >= frames.Length - 1)
                    direction = -1;
                else if (index <= 0)
                    direction = 1;

                yield return new WaitForSeconds(frameRate);
            }
        }
        else
        {
            int index = 0;
            while (true)
            {
                targetImage.sprite = frames[index];
                index = (index + 1) % frames.Length;
                yield return new WaitForSeconds(frameRate);
            }
        }

    }
}
