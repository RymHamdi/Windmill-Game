using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    public List<GameObject> BgEffects;

    public GameObject badEffect;

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

    void Start()
    {
        DeactivateEffect();
        InvokeRepeating("ActivateEffect", 2.0f, 2.0f);
    }

    public void ActivateEffect()
    {
        int index = Random.Range(0, BgEffects.Count);
        CancelInvoke(nameof(DeactivateEffect));
        if (index >= 0 && index < BgEffects.Count)
        {
            BgEffects[index].SetActive(true);
            Invoke("DeactivateEffect", 1.0f);
        }
    }

    public void PlayBadEffect()
    {
        badEffect.SetActive(true);
        CancelInvoke(nameof(DeactivateEffect));
        Invoke("DesactivateBadEffect", 1.0f);
    }

    private void DeactivateEffect()
    {
        foreach (var effect in BgEffects)
        {
            effect.SetActive(false);
        }
    }
    
    public void DesactivateBadEffect()
    {
         badEffect.SetActive(false);
    }
    

}
