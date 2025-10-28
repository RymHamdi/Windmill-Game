using System;
using UnityEngine;
using UnityEngine.UI;
public class ClothController : MonoBehaviour
{
    public Image ClothImage;

    void OnEnable()
    {
        Invoke("Subscribe", 0.2f);
    }

    private void Subscribe()
    {
        SecretLanguageManager.Instance.OnClothChange += OnClothAlter;
        OnClothAlter(SecretLanguageManager.Instance.IsCloth);
    }

    void OnDisable()
    {
        SecretLanguageManager.Instance.OnClothChange -= OnClothAlter;
    }

    private void OnClothAlter(bool obj)
    {
        ClothImage.enabled = obj;
    }
}
