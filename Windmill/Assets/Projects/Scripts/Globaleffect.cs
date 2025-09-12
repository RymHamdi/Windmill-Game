using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class Globaleffect : MonoBehaviour
{
    public static Globaleffect Instance;

    public List<EffectData> effects = new List<EffectData>();

    void Awake()
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

    public void PlayEffect(EffestType type)
    {
        EffectData effect = effects.Find(e => e.type == type);
        if (effect != null)
        {
            StartCoroutine(PlayEffectCoroutine(effect));
        }
    }

    private IEnumerator PlayEffectCoroutine(EffectData effect)
    {
        if (effect.profile == null || effect.duration <= 0 || effect.intensity <= 0) yield break;

        Volume volume = effect.profile;
        for (int i = 0; i < effect.loopCount; i++)
        {
            float elapsed = 0f;
            float loopDuration = effect.duration / effect.loopCount;
            while (elapsed < loopDuration / 2)
            {
            // Animate the volume weight from 0 to effect.intensity over the loop duration
            volume.weight = Mathf.Lerp(0f, effect.intensity, elapsed / loopDuration);
            elapsed += Time.deltaTime;
            yield return null;
            }

            while (elapsed < loopDuration)
            {
            // Animate the volume weight from 0 to effect.intensity over the loop duration
            volume.weight = Mathf.Lerp(effect.intensity, 0 , elapsed / loopDuration);
            elapsed += Time.deltaTime;
            yield return null;
            }
            volume.weight = 0f; // Reset after each loop
        }
        volume.weight = 0f; // Ensure it's reset at the end

    }
}

public enum EffestType
{
    None,
    Good,
    Bad
}

[System.Serializable]
public class EffectData
{
    public EffestType type = EffestType.None;
    public Volume profile;
    public float duration = 1.0f;
    public float loopCount = 1;
    public float intensity = 1.0f;
}
