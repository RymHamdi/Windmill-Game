using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatsBar : MonoBehaviour
{
    public Image[] statImages;        // drag the main bulb images here in order
    public float delay = 0.3f;        // time between each "light on"
    public float fadeDuration = 0.5f; // fade in duration

    void OnEnable()
    {
        // Reset bulbs + auras to invisible
        foreach (var img in statImages)
        {
            img.gameObject.SetActive(true);
            img.canvasRenderer.SetAlpha(0f);

            // Check if this bulb has a child aura image
            Image aura = img.GetComponentInChildren<Image>(true);
            if (aura != null && aura != img)
            {
                aura.gameObject.SetActive(true);
                aura.canvasRenderer.SetAlpha(0f);
            }
        }

        StartCoroutine(LightUpSequence());
    }

    IEnumerator LightUpSequence()
    {
        for (int i = 0; i < statImages.Length; i++)
        {
            // Fade in bulb
            statImages[i].CrossFadeAlpha(1f, fadeDuration, false);

            // Fade in aura if found
            Image aura = statImages[i].GetComponentInChildren<Image>(true);
            if (aura != null && aura != statImages[i])
            {
                aura.CrossFadeAlpha(1f, fadeDuration, false);
            }

            yield return new WaitForSeconds(delay);
        }
    }
}
