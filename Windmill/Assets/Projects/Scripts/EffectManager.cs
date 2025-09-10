using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public List<GameObject> BgEffects;
    // need time to activate this effect every 2 seconds

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

    private void DeactivateEffect()
    {
        foreach (var effect in BgEffects)
        {
            effect.SetActive(false);
        }
    }
    

}
