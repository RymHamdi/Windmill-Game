using System;
using UnityEngine;

public class Language : MonoBehaviour
{

    public EnglishDeutchObject languageData;

    public static Language Instance;
    public Action onLangUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetWord(int index)
    {
        string word = "";
        switch (languageData.currentLangState)
        {
            case LangState.English:
                word = languageData.wordPairs.Find(x => x.index == index).englishWord;


                break;
            case LangState.Frensh:
            word = languageData.wordPairs.Find(x => x.index == index).French;
                break;
            case LangState.Netherland:
            //Keep in mind please
                word = languageData.wordPairs.Find(x => x.index == index).germanWord;
                break;
            case LangState.Germand:
            word = languageData.wordPairs.Find(x => x.index == index).Allmand;
                break;
            case LangState.Spanish:
            word = languageData.wordPairs.Find(x => x.index == index).Spanish;
                break;
            case LangState.Chineese:
            word = languageData.wordPairs.Find(x => x.index == index).Chinese;
                break;
            case LangState.Italian:
            word = languageData.wordPairs.Find(x => x.index == index).Italian;
            break;
            default:
                break;
        }

        return word;
    }

    public void ChangeLanguage()
    {
        //if true => so the current lang is english
        onLangUpdated?.Invoke();
    }


}
