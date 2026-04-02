using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BladesController : MonoBehaviour
{
    public List<BladeSlots> bladeSlots;

    void OnEnable()
    {
        ResetBlades();
    }


    public void ResetBlades()
    {
        foreach (var item in bladeSlots)
        {
            item.formModellUI.Reset();
        }
    }

    public FormModellUI GetNextOneNeedToBeUpdated()
    {
        if (bladeSlots == null || bladeSlots.Count == 0) return null;

        // If no blade image is active, return the first available FormModellUI
        /*if (GetActivatedImageCount() == 0)
        {
            foreach (var slot in bladeSlots)
                if (slot.formModellUI != null)
                    return slot.formModellUI;

            return null;
        }*/

        // Otherwise return the first FormModellUI whose bladeImage is not active
        foreach (var slot in bladeSlots)
        {
            var img = slot.bladeImage;
            bool active = img != null && img.gameObject != null && img.transform.parent.gameObject.activeSelf;
            if (!active && slot.formModellUI != null)
                return slot.formModellUI;
        }

        return null;
    }


    public int GetActivatedImageCount()
    {
        if (bladeSlots == null) return 0;
        int count = 0;
        for (int i = 0; i < bladeSlots.Count; i++)
        {
            var img = bladeSlots[i].bladeImage;
            if (img != null && img.gameObject != null && img.transform.parent.gameObject.activeSelf)
                count++;
        }
        return count;
    }


}

[System.Serializable]
public struct BladeSlots
{
    public FormModellUI formModellUI;
    public Image bladeImage;
}
