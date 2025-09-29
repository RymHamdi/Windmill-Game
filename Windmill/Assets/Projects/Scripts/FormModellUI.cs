using UnityEngine;
using UnityEngine.UI;

public class FormModellUI : MonoBehaviour
{
    public Image iconImage;
    public Image countourImage;

    public Color normalColor;
    public Color rightColor;
    public Color wrongColor;

    public void InitModellUI(Sprite iconSprite, FormModellState state, float rotation)
    {
        switch (state)
        {
            case FormModellState.Normal:
                countourImage.color = normalColor;
                break;
            case FormModellState.Right:
                countourImage.color = rightColor;
                break;
            case FormModellState.Wrong:
                countourImage.color = wrongColor;
                break;
        }

        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.gameObject.SetActive(true);
            iconImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);
        }
    }

    public void Reset()
    {
        countourImage.color = normalColor;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
    }

}

    public enum FormModellState
    {
        Normal,
        Right,
        Wrong
    }
