using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSetup : MonoBehaviour
{
    public Sprite EnglandSprite;
    public Sprite DetuchSprite;
    public Button langButton;

    public Transform container;

    public Button buttonPrefab;

    public List<ButtonLang> buttonLangs;

    public Image openButtonImage;

    public List<Button> childObjects;

    public void CreateLangButtons()
    {
        if (!container.gameObject.activeInHierarchy)
        {
            // Show container & create buttons
            container.gameObject.SetActive(true);

            // Clear old children just in case
            childObjects.Clear();

            foreach (var lang in buttonLangs)
            {
                if (lang.langState == Language.Instance.languageData.currentLangState)
                {
                    continue;
                }
                Button newButton = Instantiate(buttonPrefab, container);
                newButton.image.sprite = lang.sprite;
                newButton.onClick.AddListener(() => UpdateLangState(lang.langState, lang.sprite));
                childObjects.Add(newButton);
            }
        }
        else
        {
            List<Button> toDestroy = new List<Button>(childObjects);

            foreach (var btn in toDestroy)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }

            childObjects.Clear();
            container.gameObject.SetActive(false);
        }
    }

    private void UpdateLangState(LangState langState, Sprite sprite)
    {
        Language.Instance.languageData.currentLangState = langState;
        openButtonImage.sprite = sprite;
        CreateLangButtons();
        UpdateLang();
    }


    void Start()
    {

        if (Language.Instance != null)
        {
            OnLangUpdated(Language.Instance.languageData.currentLangState);
        }
    }

    private void OnLangUpdated(LangState langState)
    {
        //Find the correct image
        int index = buttonLangs.FindIndex(x => x.langState == langState);
        if (index != -1)
        {
            langButton.image.sprite = buttonLangs[index].sprite;
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
            
        }
    }
}

public enum LangState { English, Frensh, Netherland, Germand, Spanish, Chineese, Italian }

[System.Serializable]
public class ButtonLang
{
    public LangState langState;
    public Sprite sprite;
}
