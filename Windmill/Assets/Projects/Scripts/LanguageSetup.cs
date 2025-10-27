using System;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSetup : MonoBehaviour
{
    public Sprite EnglandSprite;
    public Sprite DetuchSprite;
    public Button langButton;

    void Start()
    {
        
        if (Language.Instance != null)
        {
            OnLangUpdated(Language.Instance.languageData.isEnglish);
            Language.Instance.onLangUpdated += OnLangUpdated;
        }
    }

    private void OnLangUpdated(bool isEnglish)
    {
        if (isEnglish)
        {
            langButton.image.sprite = EnglandSprite;
        }
        else
        {
            langButton.image.sprite = DetuchSprite;
        }
    }

    public void UpdateLang()
    {
        if (Language.Instance != null)
        {
            Language.Instance.ChangeLanguage();
        }
    }

    void OnDisable()
    {
        if (Language.Instance != null)
        {
            Language.Instance.onLangUpdated -= OnLangUpdated;
        }
    }
}
